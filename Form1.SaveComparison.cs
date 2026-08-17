using System;
using System.Linq;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public partial class Form1
    {
        private TabPage tabSaveComparison = null!;
        private ListView lstSaveComparison = null!;
        private Label lblComparisonSummary = null!;
        private SaveFile? comparisonSave;

        private void ConfigureSaveComparison()
        {
            tabSaveComparison = new TabPage("Save Comparison");
            Panel header = new Panel { Dock = DockStyle.Top, Height = 82, Padding = new Padding(14) };
            Label title = new Label { Text = "READ-ONLY SAVE COMPARISON", AutoSize = true, Location = new System.Drawing.Point(14, 10), Font = new System.Drawing.Font("Segoe UI Semibold", 11F), ForeColor = ModernTheme.Cyan };
            lblComparisonSummary = new Label { Text = "Load an active save, then choose another save to compare.", AutoSize = true, Location = new System.Drawing.Point(15, 41) };
            header.Controls.AddRange(new Control[] { title, lblComparisonSummary });
            lstSaveComparison = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false };
            lstSaveComparison.Columns.Add("Slot", 60); lstSaveComparison.Columns.Add("Active Save", 330); lstSaveComparison.Columns.Add("Comparison Save", 330); lstSaveComparison.Columns.Add("Differences", 260);
            Panel actions = new Panel { Dock = DockStyle.Bottom, Height = 72, Padding = new Padding(14) };
            Button open = ComparisonButton("Choose comparison", 14, (_, _) => ChooseComparisonSave());
            Button import = ComparisonButton("Import selected CaC", 184, (_, _) => ImportComparedCac());
            Button clear = ComparisonButton("Clear comparison", 374, (_, _) => { comparisonSave = null; RefreshSaveComparison(); });
            actions.Controls.AddRange(new Control[] { open, import, clear });
            tabSaveComparison.Controls.Add(lstSaveComparison); tabSaveComparison.Controls.Add(actions); tabSaveComparison.Controls.Add(header);
            tabMain.TabPages.Add(tabSaveComparison);
        }

        private static Button ComparisonButton(string text, int x, EventHandler handler) { Button button = new Button { Text = text, Location = new System.Drawing.Point(x, 14), Size = new System.Drawing.Size(170, 34) }; button.Click += handler; return button; }

        private void ChooseComparisonSave()
        {
            if (currentSave == null) return;
            using OpenFileDialog dialog = new OpenFileDialog { Title = "Choose read-only comparison save", Filter = "Xenoverse 2 Saves (*.sav;*.dat;*.bin)|*.sav;*.dat;*.bin|All Files (*.*)|*.*", CheckFileExists = true };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            try { comparisonSave = new SaveFile(dialog.FileName); RefreshSaveComparison(); }
            catch (Exception ex) { MessageBox.Show($"Could not open the comparison save:\n\n{ex.Message}", "Save Comparison", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void RefreshSaveComparison()
        {
            if (lstSaveComparison == null) return;
            lstSaveComparison.Items.Clear();
            if (currentSave == null || comparisonSave == null) { lblComparisonSummary.Text = "Load an active save, then choose another save to compare."; return; }
            for (int slot = 0; slot < 8; slot++)
            {
                XV2Character active = currentSave.Characters[slot], other = comparisonSave.Characters[slot];
                string differences = DescribeCharacterDifferences(active, other);
                ListViewItem row = new ListViewItem((slot + 1).ToString()) { Tag = slot };
                row.SubItems.Add(active.IsEmpty ? "Empty" : $"{active.Name} • {active.RaceName} • Lv {active.Level}");
                row.SubItems.Add(other.IsEmpty ? "Empty" : $"{other.Name} • {other.RaceName} • Lv {other.Level}");
                row.SubItems.Add(differences);
                if (differences == "Same overview") row.ForeColor = ModernTheme.Muted;
                lstSaveComparison.Items.Add(row);
            }
            lblComparisonSummary.Text = $"Active: {System.IO.Path.GetFileName(currentSave.FilePath)}  •  Read-only: {System.IO.Path.GetFileName(comparisonSave.FilePath)}  •  CaC transfer uses verified .excac blocks";
            lstSaveComparison.Items[0].Selected = true;
        }

        private static string DescribeCharacterDifferences(XV2Character active, XV2Character other)
        {
            if (active.IsEmpty != other.IsEmpty) return active.IsEmpty ? "Only comparison occupied" : "Only active occupied";
            if (active.IsEmpty) return "Both empty";
            string[] changes = { active.Name != other.Name ? "name" : "", active.Race != other.Race ? "race" : "", active.Level != other.Level ? "level" : "", active.Experience != other.Experience ? "XP" : "", active.AttributePoints != other.AttributePoints ? "points" : "" };
            string text = string.Join(", ", changes.Where(change => change.Length > 0));
            return text.Length == 0 ? "Same overview" : text;
        }

        private void ImportComparedCac()
        {
            if (currentSave == null || comparisonSave == null || lstSaveComparison.SelectedItems.Count == 0 || lstSaveComparison.SelectedItems[0].Tag is not int slot) return;
            XV2Character source = comparisonSave.Characters[slot];
            if (source.IsEmpty) { MessageBox.Show("The selected comparison slot is empty.", "Import Compared CaC", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show($"Replace active slot {slot + 1} with {source.Name} from the comparison save?\n\nThe change remains unsaved and can be undone through Change History.", "Import Compared CaC", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            ExcacFile.FromSave(comparisonSave.DecryptedData, slot, source).ImportInto(currentSave.DecryptedData, slot);
            RefreshLoadedSave(slot, preserveDirty: true); MarkUnsaved(); RefreshSaveComparison();
        }
    }
}
