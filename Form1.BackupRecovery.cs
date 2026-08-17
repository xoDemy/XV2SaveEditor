using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public partial class Form1
    {
        private TabPage tabBackupRecovery = null!;
        private ListView lstBackups = null!;
        private Label lblBackupSummary = null!;

        private void ConfigureBackupRecovery()
        {
            tabBackupRecovery = new TabPage("Backup Recovery");
            Panel header = new Panel { Dock = DockStyle.Top, Height = 82, Padding = new Padding(14) };
            Label title = new Label { Text = "BACKUP & RECOVERY CENTER", AutoSize = true, Location = new System.Drawing.Point(14, 10), Font = new System.Drawing.Font("Segoe UI Semibold", 11F), ForeColor = ModernTheme.Cyan };
            lblBackupSummary = new Label { Text = "Load a save to view its backups.", AutoSize = true, Location = new System.Drawing.Point(15, 41) };
            header.Controls.AddRange(new Control[] { title, lblBackupSummary });

            lstBackups = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false };
            lstBackups.Columns.Add("Created", 175);
            lstBackups.Columns.Add("Location", 150);
            lstBackups.Columns.Add("File", 390);
            lstBackups.Columns.Add("Size", 110);

            Panel actions = new Panel { Dock = DockStyle.Bottom, Height = 118, Padding = new Padding(14) };
            Button refresh = MakeBackupButton("Refresh", 14, (_, _) => RefreshBackupRecovery());
            Button create = MakeBackupButton("Create backup", 164, (_, _) => CreateManualBackup());
            Button restore = MakeBackupButton("Restore selected", 324, (_, _) => RestoreSelectedBackup());
            Button folder = MakeBackupButton("Open folder", 494, (_, _) => OpenBackupFolder());
            Button compare = MakeBackupButton("Compare", 14, (_, _) => CompareSelectedBackup()); compare.Top = 62;
            Button restoreCacs = MakeBackupButton("Restore CaCs", 164, (_, _) => RestoreCacsFromBackup()); restoreCacs.Top = 62;
            actions.Controls.AddRange(new Control[] { refresh, create, restore, folder, compare, restoreCacs });

            tabBackupRecovery.Controls.Add(lstBackups);
            tabBackupRecovery.Controls.Add(actions);
            tabBackupRecovery.Controls.Add(header);
            tabMain.TabPages.Add(tabBackupRecovery);
        }

        private static Button MakeBackupButton(string text, int x, EventHandler handler)
        {
            Button button = new Button { Text = text, Location = new System.Drawing.Point(x, 14), Size = new System.Drawing.Size(145, 34) };
            button.Click += handler;
            return button;
        }

        private void RefreshBackupRecovery()
        {
            if (lstBackups == null) return;
            lstBackups.Items.Clear();
            if (currentSave == null)
            {
                lblBackupSummary.Text = "Load a save to view its backups.";
                return;
            }
            string source = currentSave.FilePath;
            string name = Path.GetFileNameWithoutExtension(source);
            string extension = Path.GetExtension(source);
            string dedicated = SaveFile.GetBackupDirectory(source, currentSave.Platform);
            List<BackupEntry> entries = new();
            if (Directory.Exists(dedicated))
                entries.AddRange(Directory.GetFiles(dedicated, $"{name}_BACKUP_*{extension}").Select(path => new BackupEntry(path, "Documents / organized", false)));
            // Discover both older layouts without moving or deleting user files.
            foreach (string legacyDirectory in SaveFile.GetLegacyBackupDirectories(source).Distinct(StringComparer.OrdinalIgnoreCase))
                if (Directory.Exists(legacyDirectory))
                    entries.AddRange(Directory.GetFiles(legacyDirectory, $"{name}_BACKUP_*{extension}").Select(path => new BackupEntry(path, "Legacy location", true)));
            entries = entries.DistinctBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase).ToList();
            foreach (BackupEntry entry in entries.OrderByDescending(entry => entry.Created))
            {
                ListViewItem row = new ListViewItem(entry.Created.ToString("yyyy-MM-dd  HH:mm:ss")) { Tag = entry };
                row.SubItems.Add(entry.Location);
                row.SubItems.Add(Path.GetFileName(entry.Path));
                row.SubItems.Add($"{entry.Size / 1024.0:N1} KB");
                if (entry.Legacy) row.ForeColor = ModernTheme.Muted;
                lstBackups.Items.Add(row);
            }
            lblBackupSummary.Text = $"{entries.Count} backup(s)  •  New backups: {dedicated}";
            if (lstBackups.Items.Count > 0) lstBackups.Items[0].Selected = true;
        }

        private void CreateManualBackup()
        {
            if (currentSave == null) return;
            try
            {
                string path = currentSave.CreateBackup();
                RefreshBackupRecovery();
                MessageBox.Show($"Backup created in XV2SaveEditor Backups:\n\n{Path.GetFileName(path)}", "Create Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Could not create the backup:\n\n{ex.Message}", "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void RestoreSelectedBackup()
        {
            if (currentSave == null || lstBackups.SelectedItems.Count == 0 || lstBackups.SelectedItems[0].Tag is not BackupEntry entry) return;
            if (hasUnsavedChanges && MessageBox.Show("Restoring will discard the current unsaved changes. Continue?", "Restore Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            if (MessageBox.Show($"Restore this backup over the active source save?\n\n{Path.GetFileName(entry.Path)}\n\nA safety backup of the current source will be created first.", "Restore Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                string source = currentSave.FilePath;
                SaveFile.CreateBackupForPath(source);
                File.Copy(entry.Path, source, true);
                currentSave = new SaveFile(source);
                enforceInfiniteDragonBalls = false;
                RefreshLoadedSave(0, preserveDirty: false);
                hasUnsavedChanges = false;
                ResetChangeHistory();
                ResetAndRunDiagnostics();
                UpdateWindowTitle();
                RefreshBackupRecovery();
                MessageBox.Show("Backup restored successfully.", "Restore Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Could not restore the backup:\n\n{ex.Message}", "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void OpenBackupFolder()
        {
            if (currentSave == null) return;
            try
            {
                string folder = SaveFile.GetBackupDirectory(currentSave.FilePath, currentSave.Platform);
                Directory.CreateDirectory(folder);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = folder, UseShellExecute = true });
            }
            catch (Exception ex) { MessageBox.Show($"Could not open the backup folder:\n\n{ex.Message}", "Backup Folder", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private BackupEntry? SelectedBackup() => lstBackups.SelectedItems.Count == 1 ? lstBackups.SelectedItems[0].Tag as BackupEntry : null;

        private void CompareSelectedBackup()
        {
            BackupEntry? entry = SelectedBackup(); if (currentSave == null || entry == null) return;
            try
            {
                SaveFile backup = new(entry.Path); int bytes = currentSave.DecryptedData.Zip(backup.DecryptedData).Count(pair => pair.First != pair.Second);
                List<string> cacs = new();
                for (int slot=0;slot<8;slot++)
                {
                    int start=ExcacFile.BaseOffset+slot*ExcacFile.BaseLength; int baseDiff=0; for(int i=0;i<ExcacFile.BaseLength;i++)if(currentSave.DecryptedData[start+i]!=backup.DecryptedData[start+i])baseDiff++;
                    int dlc=ExcacFile.DlcOffset+slot*ExcacFile.DlcLength; int dlcDiff=0;for(int i=0;i<ExcacFile.DlcLength;i++)if(currentSave.DecryptedData[dlc+i]!=backup.DecryptedData[dlc+i])dlcDiff++;
                    if(baseDiff+dlcDiff>0)cacs.Add($"Slot {slot+1}: {baseDiff+dlcDiff:N0} changed bytes");
                }
                string currency=$"Zeni: {backup.Zeni:N0} → {currentSave.Zeni:N0}\nTP Medals: {backup.TPMedals:N0} → {currentSave.TPMedals:N0}";
                MessageBox.Show($"Backup comparison\n\nTotal changed decrypted bytes: {bytes:N0}\n\n{currency}\n\nCaC blocks:\n{(cacs.Count==0?"No differences":string.Join("\n",cacs))}","Backup Comparison",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            catch(Exception ex){MessageBox.Show($"Could not compare the backup:\n\n{ex.Message}","Backup Comparison",MessageBoxButtons.OK,MessageBoxIcon.Error);}
        }

        private void RestoreCacsFromBackup()
        {
            BackupEntry? entry=SelectedBackup();if(currentSave==null||entry==null)return;
            try
            {
                SaveFile backup=new(entry.Path);using CacTargetPicker picker=new(backup.Characters,Enumerable.Empty<int>(),"backup CaC restore");if(picker.ShowDialog(this)!=DialogResult.OK)return;
                if(MessageBox.Show($"Restore {picker.SelectedSlots.Count} selected CaC block(s) from this backup?\n\nCurrent versions remain recoverable through the automatic safety backup.","Selective Restore",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;
                currentSave.CreateBackup();foreach(int slot in picker.SelectedSlots){XV2Character source=backup.Characters[slot];ExcacFile.FromSave(backup.DecryptedData,slot,source).ImportInto(currentSave.DecryptedData,slot);}
                RefreshLoadedSave(picker.SelectedSlots[0],true);MarkUnsaved();RefreshBackupRecovery();
            }
            catch(Exception ex){MessageBox.Show($"Could not restore the selected CaCs:\n\n{ex.Message}","Selective Restore",MessageBoxButtons.OK,MessageBoxIcon.Error);}
        }

        private sealed class BackupEntry
        {
            public string Path { get; }
            public string Location { get; }
            public bool Legacy { get; }
            public DateTime Created => File.GetLastWriteTime(Path);
            public long Size => new FileInfo(Path).Length;
            public BackupEntry(string path, string location, bool legacy) { Path = path; Location = location; Legacy = legacy; }
        }
    }
}
