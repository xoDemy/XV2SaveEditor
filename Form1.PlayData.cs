using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public partial class Form1
    {
        private TabPage tabPlayData = null!;
        private ComboBox cmbPlayDataCharacter = null!;
        private Label lblPlayDataChanges = null!;
        private Label lblOnlineBattles = null!, lblOnlineWins = null!, lblOnlineLosses = null!, lblOnlineWinRate = null!;
        private readonly Dictionary<PlayDataField, NumericUpDown> playDataEditors = new();
        private readonly Dictionary<PlayDataField, ComboBox> playDataNamedEditors = new();
        private byte[]? playDataSnapshot;
        private bool isLoadingPlayData;

        private void ConfigurePlayDataEditor()
        {
            tabPlayData = new TabPage("Play Data");
            int progressIndex = tabMain.TabPages.IndexOf(tabProgress);
            tabMain.TabPages.Insert(progressIndex + 1, tabPlayData);

            Panel top = new() { Dock = DockStyle.Top, Height = 112, Padding = new Padding(14) };
            top.Controls.Add(new Label { Text = "CaC:", Location = new Point(14, 13), AutoSize = true });
            cmbPlayDataCharacter = new ComboBox
            {
                Location = new Point(58, 9), Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            lblPlayDataChanges = new Label
            {
                Text = "No save loaded", Location = new Point(360, 13), AutoSize = true,
                ForeColor = ModernTheme.Muted
            };
            Label note = new()
            {
                Text = "Only fields mapped by the verified Xv2CoreLib PlayData structure are editable.",
                Location = new Point(14, 45), AutoSize = true, ForeColor = ModernTheme.Muted
            };
            top.Controls.AddRange(new Control[] { cmbPlayDataCharacter, lblPlayDataChanges, note });
            lblOnlineBattles = MakeOnlineSummaryLabel("Online Battles: —", 14);
            lblOnlineWins = MakeOnlineSummaryLabel("Online Wins: —", 204);
            lblOnlineLosses = MakeOnlineSummaryLabel("Online Losses: —", 394);
            lblOnlineWinRate = MakeOnlineSummaryLabel("Win Rate: —", 584);
            top.Controls.AddRange(new Control[] { lblOnlineBattles, lblOnlineWins, lblOnlineLosses, lblOnlineWinRate });

            Panel bottom = new() { Dock = DockStyle.Bottom, Height = 58, Padding = new Padding(14, 10, 14, 10) };
            Button revertSelected = new() { Text = "Revert selected CaC", Location = new Point(14, 10), Size = new Size(180, 34) };
            Button revertAll = new() { Text = "Revert all Play Data", Location = new Point(208, 10), Size = new Size(180, 34) };
            revertSelected.Click += (_, _) => RevertSelectedPlayData();
            revertAll.Click += (_, _) => RevertAllPlayData();
            bottom.Controls.AddRange(new Control[] { revertSelected, revertAll });

            Panel groupsHost = new()
            {
                Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(14)
            };
            TableLayoutPanel groups = new()
            {
                Dock = DockStyle.Top, AutoSize = false, Height = 950,
                ColumnCount = 2, RowCount = 3, Margin = Padding.Empty, Padding = Padding.Empty
            };
            groups.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            groups.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            groups.RowStyles.Add(new RowStyle(SizeType.Absolute, 210F));
            groups.RowStyles.Add(new RowStyle(SizeType.Absolute, 500F));
            groups.RowStyles.Add(new RowStyle(SizeType.Absolute, 230F));
            Dictionary<string, List<PlayDataField>> groupedFields = PlayDataAccess.Fields
                .GroupBy(field => field.Group).ToDictionary(group => group.Key, group => group.ToList());
            groups.Controls.Add(BuildPlayDataGroup("Training", groupedFields["Training"]), 0, 0);
            groups.Controls.Add(BuildPlayDataGroup("Other", groupedFields["Other"]), 1, 0);
            groups.Controls.Add(BuildPlayDataGroup("Base Activity", groupedFields["Base Activity"]), 0, 1);
            groups.Controls.Add(BuildPlayDataGroup("Play Trends", groupedFields["Play Trends"]), 1, 1);
            GroupBox onlineGroup = BuildOnlineRecordGroup(groupedFields["Online Record"]);
            groups.Controls.Add(onlineGroup, 0, 2);
            groups.SetColumnSpan(onlineGroup, 2);
            groupsHost.Controls.Add(groups);

            tabPlayData.Controls.Add(groupsHost);
            tabPlayData.Controls.Add(bottom);
            tabPlayData.Controls.Add(top);
            cmbPlayDataCharacter.SelectedIndexChanged += (_, _) => LoadSelectedPlayData();
        }

        private GroupBox BuildPlayDataGroup(string title, IReadOnlyList<PlayDataField> fields)
        {
            GroupBox box = new()
            {
                Text = title, Dock = DockStyle.Fill, AutoSize = false,
                Margin = new Padding(8), Padding = new Padding(14, 12, 14, 14)
            };
            TableLayoutPanel layout = new()
            {
                Dock = DockStyle.Top, AutoSize = false, Height = fields.Count * 42 + 8,
                ColumnCount = 2, RowCount = fields.Count, Padding = new Padding(0, 6, 0, 0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            ToolTip tips = new();
            for (int i = 0; i < fields.Count; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
                PlayDataField field = fields[i];
                Label label = new()
                {
                    Text = field.Name, Dock = DockStyle.Fill, AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(4, 4, 14, 7)
                };
                Control editor;
                if (IsNamedPlayDataField(field))
                {
                    ComboBox names = new()
                    {
                        Dock = DockStyle.Fill, Margin = new Padding(4, 5, 4, 7),
                        DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                        AutoCompleteSource = AutoCompleteSource.ListItems, Tag = field
                    };
                    foreach (PlayDataNamedValue value in GetPlayDataChoices(field)) names.Items.Add(value);
                    names.SelectedIndexChanged += PlayDataNamedValueChanged;
                    playDataNamedEditors[field] = names;
                    editor = names;
                }
                else
                {
                    NumericUpDown numeric = new()
                    {
                        Dock = DockStyle.Fill, Margin = new Padding(4, 5, 4, 7),
                        Minimum = field.Minimum, Maximum = field.Maximum, ThousandsSeparator = true,
                        Tag = field
                    };
                    numeric.ValueChanged += PlayDataValueChanged;
                    playDataEditors[field] = numeric;
                    editor = numeric;
                }
                if (!string.IsNullOrWhiteSpace(field.Description))
                {
                    tips.SetToolTip(label, field.Description);
                    tips.SetToolTip(editor, field.Description);
                }
                layout.Controls.Add(label, 0, i);
                layout.Controls.Add(editor, 1, i);
            }
            box.Controls.Add(layout);
            return box;
        }

        private static Label MakeOnlineSummaryLabel(string text, int left) => new()
        {
            Text = text, Location = new Point(left, 73), Size = new Size(176, 27),
            TextAlign = ContentAlignment.MiddleCenter, BackColor = ModernTheme.SurfaceRaised,
            ForeColor = ModernTheme.Cyan, Font = new Font("Segoe UI Semibold", 9F)
        };

        private GroupBox BuildOnlineRecordGroup(IReadOnlyList<PlayDataField> fields)
        {
            GroupBox outer = new()
            {
                Text = "Online Record", Dock = DockStyle.Fill, Margin = new Padding(8),
                Padding = new Padding(12, 14, 12, 12)
            };
            TableLayoutPanel modes = new()
            {
                Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(0, 4, 0, 0)
            };
            for (int i = 0; i < 3; i++) modes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            string[] modeNames = { "Player", "Ranked", "Endless" };
            for (int modeIndex = 0; modeIndex < modeNames.Length; modeIndex++)
            {
                string mode = modeNames[modeIndex];
                List<PlayDataField> modeFields = fields.Where(field => field.Name.StartsWith(mode, StringComparison.Ordinal)).ToList();
                GroupBox card = new()
                {
                    Text = mode, Dock = DockStyle.Fill, Margin = new Padding(8), Padding = new Padding(10, 12, 10, 10)
                };
                TableLayoutPanel rows = new() { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = modeFields.Count };
                rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
                rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
                for (int row = 0; row < modeFields.Count; row++)
                {
                    rows.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / modeFields.Count));
                    PlayDataField field = modeFields[row];
                    string shortName = field.Name.Substring(mode.Length).Trim();
                    Label label = new()
                    {
                        Text = shortName, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                        Margin = new Padding(4, 4, 12, 4)
                    };
                    NumericUpDown editor = new()
                    {
                        Dock = DockStyle.Fill, Minimum = field.Minimum, Maximum = field.Maximum,
                        ThousandsSeparator = true, Tag = field, Margin = new Padding(4, 7, 4, 7)
                    };
                    editor.ValueChanged += PlayDataValueChanged;
                    playDataEditors[field] = editor;
                    rows.Controls.Add(label, 0, row);
                    rows.Controls.Add(editor, 1, row);
                }
                card.Controls.Add(rows);
                modes.Controls.Add(card, modeIndex, 0);
            }
            outer.Controls.Add(modes);
            return outer;
        }

        private void RefreshPlayDataEditor()
        {
            isLoadingPlayData = true;
            try
            {
                int selected = cmbPlayDataCharacter.SelectedIndex;
                cmbPlayDataCharacter.Items.Clear();
                if (currentSave == null)
                {
                    lblPlayDataChanges.Text = "No save loaded";
                    return;
                }
                playDataSnapshot ??= (byte[])currentSave.DecryptedData.Clone();
                foreach (XV2Character character in currentSave.Characters)
                    cmbPlayDataCharacter.Items.Add(character);
                if (cmbPlayDataCharacter.Items.Count > 0)
                    cmbPlayDataCharacter.SelectedIndex = Math.Clamp(selected, 0, cmbPlayDataCharacter.Items.Count - 1);
            }
            finally { isLoadingPlayData = false; }
            LoadSelectedPlayData();
        }

        private void LoadSelectedPlayData()
        {
            if (isLoadingPlayData || currentSave == null || cmbPlayDataCharacter.SelectedItem is not XV2Character character)
                return;
            isLoadingPlayData = true;
            try
            {
                foreach ((PlayDataField field, NumericUpDown editor) in playDataEditors)
                {
                    int value = PlayDataAccess.Read(currentSave.DecryptedData, character.Slot - 1, field);
                    editor.Value = Math.Clamp((decimal)value, editor.Minimum, editor.Maximum);
                }
                foreach ((PlayDataField field, ComboBox editor) in playDataNamedEditors)
                {
                    int value = PlayDataAccess.Read(currentSave.DecryptedData, character.Slot - 1, field);
                    PlayDataNamedValue? match = editor.Items.Cast<PlayDataNamedValue>().FirstOrDefault(item => item.Id == value);
                    if (match == null)
                    {
                        match = new PlayDataNamedValue(value, $"Unknown / Modded ({value})");
                        editor.Items.Insert(0, match);
                    }
                    editor.SelectedItem = match;
                    editor.SelectionStart = 0;
                    editor.SelectionLength = 0;
                }
                UpdatePlayDataChangeCount();
                UpdateOnlineSummary(character.Slot - 1);
            }
            finally { isLoadingPlayData = false; }
        }

        private void PlayDataValueChanged(object? sender, EventArgs e)
        {
            if (isLoadingPlayData || currentSave == null || sender is not NumericUpDown editor ||
                editor.Tag is not PlayDataField field || cmbPlayDataCharacter.SelectedItem is not XV2Character character) return;
            PlayDataAccess.Write(currentSave.DecryptedData, character.Slot - 1, field, decimal.ToInt32(editor.Value));
            if (field.RelativeOffset is 88 or 92 or 96)
                PlayDataAccess.SynchronizeOnlineWins(currentSave.DecryptedData, character.Slot - 1);
            MarkUnsaved();
            UpdatePlayDataChangeCount();
            UpdateOnlineSummary(character.Slot - 1);
        }

        private void PlayDataNamedValueChanged(object? sender, EventArgs e)
        {
            if (isLoadingPlayData || currentSave == null || sender is not ComboBox editor ||
                editor.Tag is not PlayDataField field || editor.SelectedItem is not PlayDataNamedValue value ||
                cmbPlayDataCharacter.SelectedItem is not XV2Character character) return;
            PlayDataAccess.Write(currentSave.DecryptedData, character.Slot - 1, field, value.Id);
            MarkUnsaved();
            UpdatePlayDataChangeCount();
        }

        private static bool IsNamedPlayDataField(PlayDataField field) =>
            field.RelativeOffset is 0 or 1 or 136 or 140 or 144 or 148 or 152;

        private IReadOnlyList<PlayDataNamedValue> GetPlayDataChoices(PlayDataField field) =>
            field.RelativeOffset switch
            {
                0 => PlayDataNameDatabase.Mentors,
                1 => PlayDataNameDatabase.TrainingClasses,
                136 => PlayDataNameDatabase.Characters,
                _ => PlayDataNameDatabase.Skills(nameDatabase, field.RelativeOffset)
            };

        private void RevertSelectedPlayData()
        {
            if (currentSave == null || playDataSnapshot == null || cmbPlayDataCharacter.SelectedItem is not XV2Character character) return;
            int offset = PlayDataAccess.RecordOffset(character.Slot - 1);
            Array.Copy(playDataSnapshot, offset, currentSave.DecryptedData, offset, PlayDataAccess.RecordLength);
            MarkUnsaved();
            LoadSelectedPlayData();
        }

        private void RevertAllPlayData()
        {
            if (currentSave == null || playDataSnapshot == null) return;
            for (int slot = 0; slot < CharacterReader.CharacterCount; slot++)
            {
                int offset = PlayDataAccess.RecordOffset(slot);
                Array.Copy(playDataSnapshot, offset, currentSave.DecryptedData, offset, PlayDataAccess.RecordLength);
            }
            MarkUnsaved();
            LoadSelectedPlayData();
        }

        private void UpdatePlayDataChangeCount()
        {
            int count = currentSave == null ? 0 : PlayDataAccess.CountChangedFields(currentSave.DecryptedData, playDataSnapshot);
            lblPlayDataChanges.Text = currentSave == null ? "No save loaded" : $"{count} verified field(s) changed";
        }

        private void UpdateOnlineSummary(int characterSlot)
        {
            if (currentSave == null) return;
            var summary = PlayDataAccess.ReadOnlineSummary(currentSave.DecryptedData, characterSlot);
            lblOnlineBattles.Text = $"Online Battles: {summary.Battles:N0}";
            lblOnlineWins.Text = $"Online Wins: {summary.Wins:N0}";
            lblOnlineLosses.Text = $"Online Losses: {summary.Losses:N0}";
            lblOnlineWinRate.Text = $"Win Rate: {summary.WinRate:0.##}%";
        }

        private int CountChangedPlayDataFields() =>
            currentSave == null ? 0 : PlayDataAccess.CountChangedFields(currentSave.DecryptedData, playDataSnapshot);
    }
}
