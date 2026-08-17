using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public partial class Form1
    {
        private TabPage tabCacManagement = null!;
        private ListView lstCacManagement = null!;
        private ComboBox cmbCacCopyTarget = null!;
        private ComboBox cmbCacLibraryRace = null!;
        private Label lblCacManagementStatus = null!;

        private void ConfigureCacManagementHub()
        {
            tabCacManagement = new TabPage("CaC Management");
            Panel intro = new Panel { Dock = DockStyle.Top, Height = 72, Padding = new Padding(14, 10, 14, 8) };
            Label heading = new Label { Text = "CHARACTER SLOT CONTROL", AutoSize = true, Location = new System.Drawing.Point(14, 10), Font = new System.Drawing.Font("Segoe UI Semibold", 11F), ForeColor = ModernTheme.Cyan };
            lblCacManagementStatus = new Label { Text = "Load a save to inspect its CaC slots.", AutoSize = true, Location = new System.Drawing.Point(15, 39) };
            intro.Controls.AddRange(new Control[] { heading, lblCacManagementStatus });

            lstCacManagement = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                HideSelection = false
            };
            lstCacManagement.Columns.Add("Slot", 60);
            lstCacManagement.Columns.Add("Name", 245);
            lstCacManagement.Columns.Add("Race", 180);
            lstCacManagement.Columns.Add("Level", 90);
            lstCacManagement.Columns.Add("Status", 180);
            lstCacManagement.SelectedIndexChanged += (_, _) => UpdateCacManagementActions();
            lstCacManagement.DoubleClick += (_, _) => OpenSelectedManagedCac();

            Panel actions = new Panel { Dock = DockStyle.Bottom, Height = 166, Padding = new Padding(14) };
            cmbCacCopyTarget = new ComboBox { Location = new System.Drawing.Point(14, 18), Size = new System.Drawing.Size(190, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            Button duplicate = MakeCacManagementButton("Duplicate to empty", 218, 16, (_, _) => DuplicateManagedCac());
            Button moveUp = MakeCacManagementButton("Move up", 378, 16, (_, _) => MoveManagedCac(-1));
            Button moveDown = MakeCacManagementButton("Move down", 498, 16, (_, _) => MoveManagedCac(1));
            Button delete = MakeCacManagementButton("Delete safely", 638, 16, (_, _) => DeleteManagedCac());
            Button open = MakeCacManagementButton("Open in editor", 778, 16, (_, _) => OpenSelectedManagedCac());
            Button importMany = MakeCacManagementButton("Batch import", 14, 65, (_, _) => BatchImportCacs());
            Button exportMany = MakeCacManagementButton("Export all", 154, 65, (_, _) => BatchExportCacs());
            Label libraryLabel = new Label { Text = "CaC Library:", AutoSize = true, Location = new System.Drawing.Point(310, 73) };
            cmbCacLibraryRace = new ComboBox { Location = new System.Drawing.Point(394, 68), Size = new System.Drawing.Size(160, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCacLibraryRace.Items.Add("All races");
            cmbCacLibraryRace.Items.AddRange(CacLibrary.Races.Cast<object>().ToArray());
            cmbCacLibraryRace.SelectedIndex = 0;
            Button storeLibrary = MakeCacManagementButton("Store selected", 14, 112, (_, _) => StoreSelectedInLibrary());
            Button importLibrary = MakeCacManagementButton("Import library", 154, 112, (_, _) => ImportFromLibrary());
            Button openLibrary = MakeCacManagementButton("Open library", 294, 112, (_, _) => OpenCacLibrary());
            Button storeAllLibrary = MakeCacManagementButton("Store all", 434, 112, (_, _) => StoreAllInLibrary());
            actions.Controls.AddRange(new Control[] { cmbCacCopyTarget, duplicate, moveUp, moveDown, delete, open, importMany, exportMany, libraryLabel, cmbCacLibraryRace, storeLibrary, importLibrary, openLibrary, storeAllLibrary });

            tabCacManagement.Controls.Add(lstCacManagement);
            tabCacManagement.Controls.Add(actions);
            tabCacManagement.Controls.Add(intro);
            tabMain.TabPages.Insert(1, tabCacManagement);
        }

        private static Button MakeCacManagementButton(string text, int x, int y, EventHandler action)
        {
            Button button = new Button { Text = text, Location = new System.Drawing.Point(x, y), Size = new System.Drawing.Size(126, 32) };
            button.Click += action;
            return button;
        }

        private int SelectedManagedSlot() => lstCacManagement.SelectedItems.Count == 0 ? -1 : lstCacManagement.SelectedItems[0].Tag is int slot ? slot : -1;

        private void RefreshCacManagementHub(int selectSlot = -1)
        {
            if (lstCacManagement == null) return;
            lstCacManagement.BeginUpdate();
            try
            {
                lstCacManagement.Items.Clear();
                cmbCacCopyTarget.Items.Clear();
                if (currentSave == null)
                {
                    lblCacManagementStatus.Text = "Load a save to inspect its CaC slots.";
                    return;
                }
                int used = currentSave.Characters.Count(character => !character.IsEmpty);
                lblCacManagementStatus.Text = $"{used} occupied  /  {8 - used} empty  •  Double-click a character to open it";
                foreach (XV2Character character in currentSave.Characters)
                {
                    ListViewItem item = new ListViewItem(character.Slot.ToString()) { Tag = character.Slot - 1 };
                    item.SubItems.Add(character.IsEmpty ? "—" : character.Name);
                    item.SubItems.Add(character.IsEmpty ? "—" : character.RaceName);
                    item.SubItems.Add(character.IsEmpty ? "—" : character.Level.ToString());
                    item.SubItems.Add(character.IsEmpty ? "AVAILABLE" : "OCCUPIED");
                    if (character.IsEmpty) item.ForeColor = ModernTheme.Muted;
                    lstCacManagement.Items.Add(item);
                    if (character.IsEmpty) cmbCacCopyTarget.Items.Add(new CacSlotChoice(character.Slot - 1, $"Slot {character.Slot} — Empty"));
                    if (character.Slot - 1 == selectSlot) item.Selected = true;
                }
                if (cmbCacCopyTarget.Items.Count > 0) cmbCacCopyTarget.SelectedIndex = 0;
            }
            finally { lstCacManagement.EndUpdate(); }
            UpdateCacManagementActions();
        }

        private void UpdateCacManagementActions()
        {
            if (cmbCacCopyTarget == null) return;
            int slot = SelectedManagedSlot();
            cmbCacCopyTarget.Enabled = currentSave != null && slot >= 0 && !currentSave.Characters[slot].IsEmpty && cmbCacCopyTarget.Items.Count > 0;
        }

        private void DuplicateManagedCac()
        {
            int source = SelectedManagedSlot();
            if (currentSave == null || source < 0 || cmbCacCopyTarget.SelectedItem is not CacSlotChoice target) return;
            StoreCurrentCharacterControls();
            CopyVerifiedCacBlocks(source, target.Slot);
            RefreshLoadedSave(target.Slot, preserveDirty: true);
            MarkUnsaved();
        }

        private void MoveManagedCac(int direction)
        {
            if (currentSave == null) return;
            int source = SelectedManagedSlot();
            int target = source + direction;
            if (source < 0 || target < 0 || target >= 8) return;
            StoreCurrentCharacterControls();
            SwapVerifiedCacBlocks(source, target);
            RefreshLoadedSave(target, preserveDirty: true);
            MarkUnsaved();
        }

        private void DeleteManagedCac()
        {
            if (currentSave == null) return;
            int target = SelectedManagedSlot();
            if (target < 0 || currentSave.Characters[target].IsEmpty) return;
            int empty = currentSave.Characters.FindIndex(character => character.IsEmpty && character.Slot - 1 != target);
            if (empty < 0)
            {
                MessageBox.Show("Safe deletion needs one existing empty CaC slot to use as a verified blank template.", "Delete CaC", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            XV2Character character = currentSave.Characters[target];
            if (MessageBox.Show($"Delete {character.Name} from slot {character.Slot}?\n\nThis remains unsaved until you create an edited save.", "Delete CaC", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;
            CopyVerifiedCacBlocks(empty, target);
            RefreshLoadedSave(target, preserveDirty: true);
            MarkUnsaved();
        }

        private void OpenSelectedManagedCac()
        {
            int slot = SelectedManagedSlot();
            if (slot < 0 || currentSave == null) return;
            tabMain.SelectedIndex = 0;
            cmbCharacters.SelectedIndex = slot;
            UpdateModernNavigation();
        }

        private void BatchImportCacs()
        {
            if (currentSave == null) return;
            using OpenFileDialog dialog = new OpenFileDialog { Title = "Choose the CaCs to batch import", Filter = "Exported CaCs (*.excac)|*.excac", Multiselect = true, CheckFileExists = true };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            if (dialog.FileNames.Length > 8)
            {
                MessageBox.Show("A save has eight CaC slots, so at most eight characters can be imported at once.", "Batch Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                List<ExcacFile> imports = dialog.FileNames.Select(ExcacFile.Load).ToList();
                List<int>? slots = SelectBatchImportSlots(imports);
                if (slots == null) return;
                string mapping = string.Join("\n", imports.Select((character, index) =>
                {
                    XV2Character target = currentSave.Characters[slots[index]];
                    string occupied = target.IsEmpty ? "empty" : $"replaces {target.Name}";
                    return $"{character.Name}  →  Slot {slots[index] + 1} ({occupied})";
                }));
                if (MessageBox.Show($"Import these {imports.Count} CaC(s)?\n\n{mapping}\n\nChanges remain unsaved until you create an edited save.", "Confirm Batch Import", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;
                for (int i = 0; i < imports.Count; i++) imports[i].ImportInto(currentSave.DecryptedData, slots[i]);
                RefreshLoadedSave(slots[0], preserveDirty: true);
                MarkUnsaved();
            }
            catch (Exception ex) { MessageBox.Show($"Could not import the CaCs:\n\n{ex.Message}", "Batch Import - Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private List<int>? SelectBatchImportSlots(IReadOnlyList<ExcacFile> imports)
        {
            using Form picker = new Form
            {
                Text = "Choose Batch Import Slots",
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new System.Drawing.Size(520, 430),
                MinimizeBox = false,
                MaximizeBox = false,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                BackColor = ModernTheme.Window,
                ForeColor = ModernTheme.Text
            };
            Label instruction = new Label
            {
                Text = $"You selected {imports.Count} CaC(s). Choose exactly {imports.Count} destination slot(s).\nFiles are assigned to slots from top to bottom.",
                Location = new System.Drawing.Point(18, 16),
                Size = new System.Drawing.Size(480, 48)
            };
            CheckedListBox slots = new CheckedListBox
            {
                Location = new System.Drawing.Point(18, 72),
                Size = new System.Drawing.Size(484, 270),
                CheckOnClick = true,
                BackColor = ModernTheme.Surface,
                ForeColor = ModernTheme.Text,
                BorderStyle = BorderStyle.FixedSingle
            };
            foreach (XV2Character character in currentSave!.Characters)
                slots.Items.Add($"Slot {character.Slot}  —  {(character.IsEmpty ? "Empty" : $"{character.Name} · {character.RaceName} · Lv. {character.Level}")}");
            Label count = new Label { Text = $"0 / {imports.Count} selected", Location = new System.Drawing.Point(18, 354), AutoSize = true, ForeColor = ModernTheme.Cyan };
            Button confirm = new Button { Text = "Use selected slots", Location = new System.Drawing.Point(294, 382), Size = new System.Drawing.Size(128, 32), DialogResult = DialogResult.OK, Enabled = false };
            Button cancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(430, 382), Size = new System.Drawing.Size(72, 32), DialogResult = DialogResult.Cancel };
            slots.ItemCheck += (_, _) => picker.BeginInvoke(() =>
            {
                count.Text = $"{slots.CheckedItems.Count} / {imports.Count} selected";
                confirm.Enabled = slots.CheckedItems.Count == imports.Count;
            });
            picker.Controls.AddRange(new Control[] { instruction, slots, count, confirm, cancel });
            ModernTheme.Apply(picker);
            picker.AcceptButton = confirm;
            picker.CancelButton = cancel;
            if (picker.ShowDialog(this) != DialogResult.OK) return null;
            return slots.CheckedIndices.Cast<int>().OrderBy(slot => slot).ToList();
        }

        private void BatchExportCacs()
        {
            if (currentSave == null) return;
            using FolderBrowserDialog dialog = new FolderBrowserDialog { Description = "Choose a folder for the exported CaCs", UseDescriptionForTitle = true };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                StoreCurrentCharacterControls();
                int count = 0;
                foreach (XV2Character character in currentSave.Characters.Where(character => !character.IsEmpty))
                {
                    string safeName = string.Concat(character.Name.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
                    string path = Path.Combine(dialog.SelectedPath, $"{character.Slot:00}_{safeName}.excac");
                    ExcacFile.FromSave(currentSave.DecryptedData, character.Slot - 1, character).Save(path);
                    count++;
                }
                MessageBox.Show($"Exported {count} CaC(s).", "Export All CaCs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Could not export the CaCs:\n\n{ex.Message}", "Export All - Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void StoreSelectedInLibrary()
        {
            int slot = SelectedManagedSlot();
            if (currentSave == null || slot < 0 || currentSave.Characters[slot].IsEmpty) return;
            try
            {
                StoreCurrentCharacterControls();
                XV2Character character = currentSave.Characters[slot];
                string path = CacLibrary.Store(ExcacFile.FromSave(currentSave.DecryptedData, slot, character));
                MessageBox.Show($"{character.Name} was stored in the {character.RaceName} library.\n\n{path}", "CaC Library", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Could not store the CaC:\n\n{ex.Message}", "CaC Library - Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void StoreAllInLibrary()
        {
            if (currentSave == null) return;
            try
            {
                StoreCurrentCharacterControls();
                List<XV2Character> characters = currentSave.Characters.Where(character => !character.IsEmpty).ToList();
                if (characters.Count == 0)
                {
                    MessageBox.Show("This save has no occupied CaC slots to store.", "CaC Library", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                foreach (XV2Character character in characters)
                    CacLibrary.Store(ExcacFile.FromSave(currentSave.DecryptedData, character.Slot - 1, character));
                MessageBox.Show($"Stored {characters.Count} CaC(s) in their race libraries.\n\n{CacLibrary.RootPath}", "CaC Library", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Could not store all CaCs:\n\n{ex.Message}", "CaC Library - Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ImportFromLibrary()
        {
            if (currentSave == null) return;
            int slot = SelectedManagedSlot();
            if (slot < 0)
            {
                MessageBox.Show("Select the destination CaC slot first.", "CaC Library", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            CacLibrary.EnsureFolders();
            string folder = cmbCacLibraryRace.SelectedIndex > 0
                ? CacLibrary.GetRaceFolder(cmbCacLibraryRace.SelectedIndex - 1)
                : CacLibrary.RootPath;
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Title = $"Import a library CaC into slot {slot + 1}",
                Filter = "Exported CaCs (*.excac)|*.excac",
                InitialDirectory = folder,
                CheckFileExists = true
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                ExcacFile import = ExcacFile.Load(dialog.FileName);
                XV2Character target = currentSave.Characters[slot];
                string targetText = target.IsEmpty ? $"empty slot {slot + 1}" : $"{target.Name} in slot {slot + 1}";
                if (MessageBox.Show($"Import {import.Name} over {targetText}?\n\nThis remains unsaved until you create an edited save.", "Import Library CaC", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;
                import.ImportInto(currentSave.DecryptedData, slot);
                RefreshLoadedSave(slot, preserveDirty: true);
                MarkUnsaved();
            }
            catch (Exception ex) { MessageBox.Show($"Could not import the CaC:\n\n{ex.Message}", "CaC Library - Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void OpenCacLibrary()
        {
            using CacLibraryBrowser browser = new CacLibraryBrowser();
            if (browser.ShowDialog(this) != DialogResult.OK || browser.SelectedPath == null) return;
            if (currentSave == null) { MessageBox.Show("Load a save before importing a library CaC.", "CaC Library", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            int slot = SelectedManagedSlot();
            if (slot < 0) { MessageBox.Show("Select the destination slot in CaC Management first.", "CaC Library", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            try
            {
                ExcacFile import = ExcacFile.Load(browser.SelectedPath);
                XV2Character target = currentSave.Characters[slot];
                string targetText = target.IsEmpty ? $"empty slot {slot + 1}" : $"{target.Name} in slot {slot + 1}";
                if (MessageBox.Show($"Import {import.Name} over {targetText}?\n\nThis remains unsaved until you create an edited save.", "Import Library CaC", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;
                import.ImportInto(currentSave.DecryptedData, slot); RefreshLoadedSave(slot, preserveDirty: true); MarkUnsaved();
            }
            catch (Exception ex) { MessageBox.Show($"Could not import the CaC:\n\n{ex.Message}", "CaC Library - Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void CopyVerifiedCacBlocks(int source, int target)
        {
            byte[] data = currentSave!.DecryptedData;
            Array.Copy(data, ExcacFile.BaseOffset + source * ExcacFile.BaseLength, data, ExcacFile.BaseOffset + target * ExcacFile.BaseLength, ExcacFile.BaseLength);
            Array.Copy(data, ExcacFile.DlcOffset + source * ExcacFile.DlcLength, data, ExcacFile.DlcOffset + target * ExcacFile.DlcLength, ExcacFile.DlcLength);
        }

        private void SwapVerifiedCacBlocks(int first, int second)
        {
            byte[] data = currentSave!.DecryptedData;
            SwapBlock(data, ExcacFile.BaseOffset + first * ExcacFile.BaseLength, ExcacFile.BaseOffset + second * ExcacFile.BaseLength, ExcacFile.BaseLength);
            SwapBlock(data, ExcacFile.DlcOffset + first * ExcacFile.DlcLength, ExcacFile.DlcOffset + second * ExcacFile.DlcLength, ExcacFile.DlcLength);
        }

        private static void SwapBlock(byte[] data, int first, int second, int length)
        {
            byte[] temporary = new byte[length];
            Array.Copy(data, first, temporary, 0, length);
            Array.Copy(data, second, data, first, length);
            Array.Copy(temporary, 0, data, second, length);
        }

        private sealed record CacSlotChoice(int Slot, string Label)
        {
            public override string ToString() => Label;
        }
    }
}
