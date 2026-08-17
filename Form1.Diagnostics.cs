using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public partial class Form1
    {
        private TabPage tabDiagnostics = null!;
        private ListView lstDiagnostics = null!;
        private Label lblDiagnosticSummary = null!;
        private readonly List<DiagnosticFinding> diagnosticFindings = new();
        private byte[]? diagnosticSnapshot;
        private SaveFile? diagnosticSnapshotOwner;

        private void ConfigureDiagnosticsHub()
        {
            tabDiagnostics = new TabPage("Diagnostics");
            Panel header = new Panel { Dock = DockStyle.Top, Height = 78, Padding = new Padding(14) };
            Label title = new Label { Text = "SAVE HEALTH CHECK", AutoSize = true, Location = new System.Drawing.Point(14, 10), Font = new System.Drawing.Font("Segoe UI Semibold", 11F), ForeColor = ModernTheme.Cyan };
            lblDiagnosticSummary = new Label { Text = "Load a save to run diagnostics.", AutoSize = true, Location = new System.Drawing.Point(15, 40) };
            header.Controls.AddRange(new Control[] { title, lblDiagnosticSummary });

            lstDiagnostics = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = true, HideSelection = false };
            lstDiagnostics.Columns.Add("Severity", 90);
            lstDiagnostics.Columns.Add("Area", 145);
            lstDiagnostics.Columns.Add("Finding", 520);
            lstDiagnostics.Columns.Add("Repair", 260);

            Panel actions = new Panel { Dock = DockStyle.Bottom, Height = 72, Padding = new Padding(14) };
            Button recheck = MakeDiagnosticButton("Recheck", 14, (_, _) => RunDiagnostics());
            Button repairSelected = MakeDiagnosticButton("Repair selected", 154, (_, _) => RepairSelectedDiagnostics());
            Button repairAll = MakeDiagnosticButton("Repair all safe", 314, (_, _) => RepairAllDiagnostics());
            Button revert = MakeDiagnosticButton("Revert repairs", 474, (_, _) => RevertDiagnosticRepairs());
            actions.Controls.AddRange(new Control[] { recheck, repairSelected, repairAll, revert });
            tabDiagnostics.Controls.Add(lstDiagnostics);
            tabDiagnostics.Controls.Add(actions);
            tabDiagnostics.Controls.Add(header);
            tabMain.TabPages.Add(tabDiagnostics);
        }

        private static Button MakeDiagnosticButton(string text, int x, EventHandler handler)
        {
            Button button = new Button { Text = text, Location = new System.Drawing.Point(x, 14), Size = new System.Drawing.Size(145, 34) };
            button.Click += handler;
            return button;
        }

        private void ResetAndRunDiagnostics()
        {
            diagnosticSnapshot = null;
            diagnosticSnapshotOwner = null;
            RunDiagnostics();
        }

        private void RunDiagnostics()
        {
            if (lstDiagnostics == null) return;
            diagnosticFindings.Clear();
            lstDiagnostics.Items.Clear();
            if (currentSave == null)
            {
                lblDiagnosticSummary.Text = "Load a save to run diagnostics.";
                return;
            }
            if (diagnosticSnapshot == null || !ReferenceEquals(diagnosticSnapshotOwner, currentSave))
            {
                diagnosticSnapshot = (byte[])currentSave.DecryptedData.Clone();
                diagnosticSnapshotOwner = currentSave;
            }

            if (currentSave.Zeni > 999999999u)
                AddDiagnostic("Warning", "Currency", $"Zeni is {currentSave.Zeni:N0}, above the game-safe limit.", "Clamp to 999,999,999", () => currentSave.Zeni = 999999999u);
            if (currentSave.TPMedals > 999999999u)
                AddDiagnostic("Warning", "Currency", $"TP Medals are {currentSave.TPMedals:N0}, above the game-safe limit.", "Clamp to 999,999,999", () => currentSave.TPMedals = 999999999u);

            foreach (XV2Character character in currentSave.Characters.Where(character => !character.IsEmpty))
            {
                int slot = character.Slot - 1;
                if (character.Race is < 0 or > 7)
                    AddDiagnostic("Error", $"CaC {character.Slot}", $"{character.Name} has invalid race ID {character.Race}.", "Manual review required", null);
                if (!LevelExperience.IsValidLevel(character.Level))
                    AddDiagnostic("Error", $"CaC {character.Slot}", $"{character.Name} has invalid level {character.Level}.", "Manual review required", null);
                else
                {
                    int expectedXp = LevelExperience.ExperienceForLevel(character.Level);
                    if (character.Experience != expectedXp)
                    {
                        AddDiagnostic("Warning", $"CaC {character.Slot}", $"{character.Name}: XP {character.Experience:N0} does not match level {character.Level} ({expectedXp:N0}).",
                            $"Set XP to {expectedXp:N0}", () => WriteCharacterInt(slot, 176, expectedXp));
                    }
                }
            }

            byte[] flagsApplied = (byte[])currentSave.DecryptedData.Clone();
            LevelCapFlagValidator.Apply(flagsApplied);
            if (!flagsApplied.SequenceEqual(currentSave.DecryptedData))
                AddDiagnostic("Warning", "Level Caps", "One or more verified Guru/Whis level-cap flags are missing for the current CaC levels.", "Apply verified cap flags", () => LevelCapFlagValidator.Apply(currentSave.DecryptedData));

            foreach (XV2Character character in currentSave.Characters.Where(character => !character.IsEmpty))
            {
                foreach (XV2QuestProgress quest in QuestProgressReader.Read(currentSave.DecryptedData, character.Slot - 1))
                {
                    if (quest.State is < 0 or > 3)
                        AddQuestDiagnostic(character, quest, $"invalid state {quest.State}", () => quest.State = Math.Clamp(quest.State, 0, 3));
                    if (quest.Rank is < 0 or > 7)
                        AddQuestDiagnostic(character, quest, $"invalid rank {quest.Rank}", () => quest.Rank = Math.Clamp(quest.Rank, 0, 7));
                    if (quest.Score < 0)
                        AddQuestDiagnostic(character, quest, $"negative score {quest.Score:N0}", () => quest.Score = 0);
                }
            }

            foreach (DiagnosticFinding finding in diagnosticFindings)
            {
                ListViewItem row = new ListViewItem(finding.Severity) { Tag = finding };
                row.SubItems.Add(finding.Area); row.SubItems.Add(finding.Message); row.SubItems.Add(finding.RepairDescription);
                row.ForeColor = finding.Repair == null ? ModernTheme.Muted : ModernTheme.Text;
                lstDiagnostics.Items.Add(row);
            }
            int repairable = diagnosticFindings.Count(finding => finding.Repair != null);
            lblDiagnosticSummary.Text = diagnosticFindings.Count == 0
                ? "No issues found by the verified checks."
                : $"{diagnosticFindings.Count} finding(s)  •  {repairable} safe automatic repair(s)";
        }

        private void AddQuestDiagnostic(XV2Character character, XV2QuestProgress quest, string problem, Action edit)
        {
            AddDiagnostic("Warning", $"CaC {character.Slot} Quests", $"{quest.Category} {quest.ID:D3} has {problem}.", "Normalize verified field", () => { edit(); QuestProgressWriter.Write(currentSave!.DecryptedData, quest); });
        }

        private void AddDiagnostic(string severity, string area, string message, string repairDescription, Action? repair)
            => diagnosticFindings.Add(new DiagnosticFinding(severity, area, message, repairDescription, repair));

        private void WriteCharacterInt(int slot, int relativeOffset, int value)
        {
            int offset = CharacterReader.CharacterSectionOffset + CharacterReader.CharacterStride * slot + relativeOffset;
            BitConverter.GetBytes(value).CopyTo(currentSave!.DecryptedData, offset);
        }

        private void RepairSelectedDiagnostics()
        {
            List<DiagnosticFinding> selected = lstDiagnostics.SelectedItems.Cast<ListViewItem>()
                .Select(item => item.Tag).OfType<DiagnosticFinding>()
                .Where(item => item.Repair != null).ToList();
            ApplyDiagnosticRepairs(selected);
        }

        private void RepairAllDiagnostics() => ApplyDiagnosticRepairs(diagnosticFindings.Where(finding => finding.Repair != null).ToList());

        private void ApplyDiagnosticRepairs(List<DiagnosticFinding> repairs)
        {
            if (currentSave == null || repairs.Count == 0) return;
            if (MessageBox.Show($"Apply {repairs.Count} verified repair(s)?\n\nThe changes remain unsaved until you create an edited save.", "Save Diagnostics", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            foreach (DiagnosticFinding finding in repairs) finding.Repair!();
            nudZeni.Value = Math.Min(currentSave.Zeni, 999999999u);
            nudTPMedals.Value = Math.Min(currentSave.TPMedals, 999999999u);
            MarkUnsaved();
            RefreshLoadedSave(cmbCharacters.SelectedIndex < 0 ? 0 : cmbCharacters.SelectedIndex, preserveDirty: true);
        }

        private void RevertDiagnosticRepairs()
        {
            if (currentSave == null || diagnosticSnapshot == null || !ReferenceEquals(diagnosticSnapshotOwner, currentSave)) return;
            diagnosticSnapshot.CopyTo(currentSave.DecryptedData, 0);
            MarkUnsaved();
            RefreshLoadedSave(cmbCharacters.SelectedIndex < 0 ? 0 : cmbCharacters.SelectedIndex, preserveDirty: true);
            RunDiagnostics();
        }

        private sealed record DiagnosticFinding(string Severity, string Area, string Message, string RepairDescription, Action? Repair);
    }
}
