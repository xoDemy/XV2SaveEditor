using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public partial class Form1
    {
        private const int MaximumHistoryEntries = 40;
        private TabPage tabChangeHistory = null!;
        private ListView lstChangeHistory = null!;
        private Label lblHistorySummary = null!;
        private Button btnHistoryUndo = null!, btnHistoryRedo = null!;
        private readonly List<HistorySnapshot> changeHistory = new();
        private readonly System.Windows.Forms.Timer historyTimer = new() { Interval = 650 };
        private int historyIndex = -1;
        private bool restoringHistory;
        private SaveFile? historyOwner;

        private void ConfigureChangeHistory()
        {
            tabChangeHistory = new TabPage("Change History");
            Panel header = new Panel { Dock = DockStyle.Top, Height = 78, Padding = new Padding(14) };
            Label title = new Label { Text = "UNDO / REDO TIMELINE", AutoSize = true, Location = new System.Drawing.Point(14, 10), Font = new System.Drawing.Font("Segoe UI Semibold", 11F), ForeColor = ModernTheme.Cyan };
            lblHistorySummary = new Label { Text = "Load a save to begin tracking changes.", AutoSize = true, Location = new System.Drawing.Point(15, 40) };
            header.Controls.AddRange(new Control[] { title, lblHistorySummary });

            lstChangeHistory = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false };
            lstChangeHistory.Columns.Add("#", 55);
            lstChangeHistory.Columns.Add("Time", 110);
            lstChangeHistory.Columns.Add("Workspace", 190);
            lstChangeHistory.Columns.Add("Change", 560);

            Panel actions = new Panel { Dock = DockStyle.Bottom, Height = 72, Padding = new Padding(14) };
            btnHistoryUndo = MakeHistoryButton("Undo", 14, (_, _) => UndoHistory());
            btnHistoryRedo = MakeHistoryButton("Redo", 164, (_, _) => RedoHistory());
            Button baseline = MakeHistoryButton("Restore load state", 314, (_, _) => RestoreHistory(0));
            Button clear = MakeHistoryButton("Clear timeline", 494, (_, _) => ResetChangeHistory());
            actions.Controls.AddRange(new Control[] { btnHistoryUndo, btnHistoryRedo, baseline, clear });

            tabChangeHistory.Controls.Add(lstChangeHistory);
            tabChangeHistory.Controls.Add(actions);
            tabChangeHistory.Controls.Add(header);
            tabMain.TabPages.Add(tabChangeHistory);
            historyTimer.Tick += (_, _) => CapturePendingHistory();
            UpdateHistoryView();
        }

        private static Button MakeHistoryButton(string text, int x, EventHandler handler)
        {
            Button button = new Button { Text = text, Location = new System.Drawing.Point(x, 14), Size = new System.Drawing.Size(140, 34) };
            button.Click += handler;
            return button;
        }

        private void ResetChangeHistory()
        {
            historyTimer.Stop();
            changeHistory.Clear();
            historyIndex = -1;
            historyOwner = currentSave;
            if (currentSave != null)
            {
                CommitVisibleControlsForHistory();
                changeHistory.Add(new HistorySnapshot((byte[])currentSave.DecryptedData.Clone(), DateTime.Now, "Save", "Load-time state"));
                historyIndex = 0;
            }
            UpdateHistoryView();
        }

        private void ScheduleHistoryCapture()
        {
            if (restoringHistory || currentSave == null || !ReferenceEquals(historyOwner, currentSave)) return;
            historyTimer.Stop();
            historyTimer.Start();
        }

        private void CapturePendingHistory()
        {
            historyTimer.Stop();
            if (restoringHistory || currentSave == null || !ReferenceEquals(historyOwner, currentSave)) return;
            CommitVisibleControlsForHistory();
            byte[] data = (byte[])currentSave.DecryptedData.Clone();
            if (historyIndex >= 0 && data.SequenceEqual(changeHistory[historyIndex].Data)) return;
            if (historyIndex + 1 < changeHistory.Count) changeHistory.RemoveRange(historyIndex + 1, changeHistory.Count - historyIndex - 1);
            string workspace = tabMain.SelectedIndex >= 0 ? tabMain.SelectedTab?.Text ?? "Editor" : "Editor";
            string character = cmbCharacters.SelectedItem is XV2Character cac && !cac.IsEmpty ? $" for {txtCharacterName.Text}" : "";
            changeHistory.Add(new HistorySnapshot(data, DateTime.Now, workspace, $"Edited {workspace}{character}"));
            if (changeHistory.Count > MaximumHistoryEntries)
            {
                changeHistory.RemoveAt(1); // Always retain the load-time baseline.
            }
            historyIndex = changeHistory.Count - 1;
            UpdateHistoryView();
        }

        private void CommitVisibleControlsForHistory()
        {
            if (currentSave == null) return;
            currentSave.Zeni = (uint)nudZeni.Value;
            currentSave.TPMedals = (uint)nudTPMedals.Value;
            StoreCurrentCharacterControls();
        }

        private void UndoHistory()
        {
            CapturePendingHistory();
            if (historyIndex > 0) RestoreHistory(historyIndex - 1);
        }

        private void RedoHistory()
        {
            CapturePendingHistory();
            if (historyIndex >= 0 && historyIndex < changeHistory.Count - 1) RestoreHistory(historyIndex + 1);
        }

        private void RestoreHistory(int index)
        {
            if (currentSave == null || index < 0 || index >= changeHistory.Count) return;
            historyTimer.Stop();
            restoringHistory = true;
            try
            {
                changeHistory[index].Data.CopyTo(currentSave.DecryptedData, 0);
                historyIndex = index;
                int slot = Math.Max(0, cmbCharacters.SelectedIndex);
                RefreshLoadedSave(slot, preserveDirty: index != 0);
                hasUnsavedChanges = index != 0;
                UpdateWindowTitle();
            }
            finally { restoringHistory = false; }
            UpdateHistoryView();
        }

        private void UpdateHistoryView()
        {
            if (lstChangeHistory == null) return;
            lstChangeHistory.BeginUpdate();
            lstChangeHistory.Items.Clear();
            for (int i = 0; i < changeHistory.Count; i++)
            {
                HistorySnapshot snapshot = changeHistory[i];
                ListViewItem row = new ListViewItem(i.ToString()) { Tag = i };
                row.SubItems.Add(snapshot.Time.ToString("HH:mm:ss"));
                row.SubItems.Add(snapshot.Workspace);
                row.SubItems.Add((i == historyIndex ? "CURRENT • " : "") + snapshot.Description);
                if (i != historyIndex) row.ForeColor = ModernTheme.Muted;
                lstChangeHistory.Items.Add(row);
            }
            lstChangeHistory.EndUpdate();
            btnHistoryUndo.Enabled = historyIndex > 0;
            btnHistoryRedo.Enabled = historyIndex >= 0 && historyIndex < changeHistory.Count - 1;
            lblHistorySummary.Text = currentSave == null ? "Load a save to begin tracking changes."
                : $"{Math.Max(0, changeHistory.Count - 1)} checkpoint(s)  •  Position {Math.Max(0, historyIndex)}  •  Rapid edits are grouped automatically";
            if (historyIndex >= 0 && historyIndex < lstChangeHistory.Items.Count)
                lstChangeHistory.Items[historyIndex].EnsureVisible();
        }

        private sealed record HistorySnapshot(byte[] Data, DateTime Time, string Workspace, string Description);
    }
}
