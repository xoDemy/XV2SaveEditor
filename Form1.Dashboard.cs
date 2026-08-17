using System.Diagnostics;

namespace XV2SaveEditor;

public partial class Form1
{
    private TabPage tabDashboard = null!;
    private Label lblDashboardFile = null!, lblDashboardPlatform = null!, lblDashboardOwner = null!, lblDashboardCacs = null!, lblDashboardMoney = null!, lblDashboardHealth = null!, lblDashboardBackup = null!;
    private ComboBox cmbSaveProfiles = null!;

    private void ConfigureSaveDashboard()
    {
        tabDashboard = new TabPage("Dashboard") { Padding = new Padding(24) };
        Label title = new() { Text = "SAVE COMMAND CENTER", Location = new System.Drawing.Point(26, 22), AutoSize = true, Font = new System.Drawing.Font("Segoe UI Semibold", 16F), ForeColor = ModernTheme.Cyan };
        TableLayoutPanel cards = new() { Location = new System.Drawing.Point(26, 72), Size = new System.Drawing.Size(1060, 280), ColumnCount = 2, RowCount = 4, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i=0;i<4;i++) cards.RowStyles.Add(new RowStyle(SizeType.Percent,25));
        lblDashboardFile = DashboardCard(cards,"ACTIVE FILE",0,0); lblDashboardPlatform=DashboardCard(cards,"PLATFORM",1,0); lblDashboardOwner=DashboardCard(cards,"OWNERSHIP",0,1);
        lblDashboardCacs=DashboardCard(cards,"CHARACTERS",1,1); lblDashboardMoney=DashboardCard(cards,"CURRENCIES",0,2); lblDashboardHealth=DashboardCard(cards,"SAVE HEALTH",1,2);
        lblDashboardBackup=DashboardCard(cards,"LATEST BACKUP",0,3);
        Panel profiles = new() { Location = new System.Drawing.Point(26, 378), Size = new System.Drawing.Size(1060, 86), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        profiles.Controls.Add(new Label { Text="SAVE PROFILES", Location=new System.Drawing.Point(0,0), AutoSize=true, ForeColor=ModernTheme.Cyan });
        cmbSaveProfiles = new ComboBox { Location=new System.Drawing.Point(0,28), Size=new System.Drawing.Size(360,27), DropDownStyle=ComboBoxStyle.DropDownList };
        Button open=DashboardButton("Open profile",375,26,(_,_)=>OpenSelectedProfile()); Button add=DashboardButton("Pin current",510,26,(_,_)=>PinCurrentProfile()); Button remove=DashboardButton("Remove",645,26,(_,_)=>RemoveSelectedProfile()); Button recent=DashboardButton("Recent saves",780,26,(_,_)=>ShowRecentSaves());
        profiles.Controls.AddRange(new Control[]{cmbSaveProfiles,open,add,remove,recent});
        Panel quick = new() { Location=new System.Drawing.Point(26,490), Size=new System.Drawing.Size(1060,70), Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right };
        quick.Controls.Add(new Label { Text="QUICK ACTIONS", Location=new System.Drawing.Point(0,0), AutoSize=true, ForeColor=ModernTheme.Cyan });
        quick.Controls.AddRange(new Control[]{DashboardButton("Diagnostics",0,28,(_,_)=>OpenModule(tabDiagnostics)),DashboardButton("Backups",145,28,(_,_)=>OpenModule(tabBackupRecovery)),DashboardButton("Max-Out Wizard",290,28,(_,_)=>OpenModule(tabMaxOut)),DashboardButton("Refresh",455,28,(_,_)=>RefreshDashboard())});
        tabDashboard.Controls.AddRange(new Control[]{title,cards,profiles,quick}); tabMain.TabPages.Add(tabDashboard); RefreshProfiles(); RefreshDashboard();
    }

    private Label DashboardCard(TableLayoutPanel table,string heading,int column,int row)
    {
        Panel card=new(){Dock=DockStyle.Fill,Margin=new Padding(5),Padding=new Padding(14),BackColor=ModernTheme.Surface}; Label h=new(){Text=heading,AutoSize=true,ForeColor=ModernTheme.Cyan,Font=new System.Drawing.Font("Segoe UI Semibold",8F)}; Label v=new(){Text="No save loaded",Location=new System.Drawing.Point(14,34),AutoSize=true,MaximumSize=new System.Drawing.Size(470,38)}; card.Controls.AddRange(new Control[]{h,v}); table.Controls.Add(card,column,row); return v;
    }
    private static Button DashboardButton(string text,int x,int y,EventHandler action){Button b=new(){Text=text,Location=new System.Drawing.Point(x,y),Size=new System.Drawing.Size(125,34)};b.Click+=action;return b;}
    private void OpenModule(TabPage page){tabMain.SelectedTab=page;UpdateModernNavigation();}

    private void RefreshDashboard()
    {
        if(lblDashboardFile==null)return;
        if(currentSave==null){foreach(Label label in new[]{lblDashboardFile,lblDashboardPlatform,lblDashboardOwner,lblDashboardCacs,lblDashboardMoney,lblDashboardHealth,lblDashboardBackup})label.Text="No save loaded";return;}
        lblDashboardFile.Text=Path.GetFileName(currentSave.FilePath); lblDashboardPlatform.Text=PlatformSaveAdapter.DisplayName(currentSave.Platform);
        lblDashboardOwner.Text=currentSave.Platform==SavePlatform.PC?$"SteamID64 {SteamOwnership.ReadSteamId64(currentSave.DecryptedData)}":"Steam link not required";
        List<XV2Character> cacs=currentSave.Characters.Where(x=>!x.IsEmpty).ToList();lblDashboardCacs.Text=$"{cacs.Count} occupied · {8-cacs.Count} empty · Max Lv. {(cacs.Count==0?0:cacs.Max(x=>x.Level))}";
        lblDashboardMoney.Text=$"Zeni {currentSave.Zeni:N0} · TP {currentSave.TPMedals:N0}"; List<PreSaveIssue> issues=PreSaveValidator.Inspect(currentSave);lblDashboardHealth.Text=issues.Count==0?"No verified issues":$"{issues.Count(x=>x.Severity=="Error")} errors · {issues.Count(x=>x.Severity!="Error")} warnings";
        string folder=SaveFile.GetBackupDirectory(currentSave.FilePath,currentSave.Platform);string name=Path.GetFileNameWithoutExtension(currentSave.FilePath);string extension=Path.GetExtension(currentSave.FilePath);string? latest=Directory.Exists(folder)?Directory.GetFiles(folder,$"{name}_BACKUP_*{extension}").OrderByDescending(File.GetLastWriteTime).FirstOrDefault():null;lblDashboardBackup.Text=latest==null?"No backup found":$"{Path.GetFileName(latest)} · {File.GetLastWriteTime(latest):g}";
    }

    private void RefreshProfiles(){if(cmbSaveProfiles==null)return;cmbSaveProfiles.Items.Clear();foreach(SaveProfile p in EditorPreferences.Load().SaveProfiles)cmbSaveProfiles.Items.Add(p);cmbSaveProfiles.DisplayMember=nameof(SaveProfile.Name);if(cmbSaveProfiles.Items.Count>0)cmbSaveProfiles.SelectedIndex=0;}
    private void PinCurrentProfile(){if(currentSave==null)return;string name=Path.GetFileNameWithoutExtension(currentSave.FilePath);EditorPreferences prefs=EditorPreferences.Load();if(prefs.SaveProfiles.Any(x=>string.Equals(x.Path,currentSave.FilePath,StringComparison.OrdinalIgnoreCase)))return;prefs.SaveProfiles.Add(new(name,currentSave.FilePath));prefs.LastSavePath=currentSave.FilePath;prefs.Save();RefreshProfiles();}
    private void RemoveSelectedProfile(){if(cmbSaveProfiles.SelectedItem is not SaveProfile profile)return;EditorPreferences prefs=EditorPreferences.Load();prefs.SaveProfiles.RemoveAll(x=>string.Equals(x.Path,profile.Path,StringComparison.OrdinalIgnoreCase));prefs.Save();RefreshProfiles();}
    private void OpenSelectedProfile(){if(cmbSaveProfiles.SelectedItem is not SaveProfile profile)return;if(!File.Exists(profile.Path)){MessageBox.Show("That profile file no longer exists.","Save Profiles",MessageBoxButtons.OK,MessageBoxIcon.Information);return;}OpenSavePath(profile.Path);}

    private void ConfigureDragAndDrop()
    {
        AllowDrop=true;DragEnter+=(_,e)=>{if(e.Data?.GetDataPresent(DataFormats.FileDrop)==true)e.Effect=DragDropEffects.Copy;};
        DragDrop+=(_,e)=>{string[] files=(string[]?)e.Data?.GetData(DataFormats.FileDrop)??Array.Empty<string>();if(files.Length==0)return;string ext=Path.GetExtension(files[0]).ToLowerInvariant();if(ext==".excac"){ImportDroppedCacs(files.Where(x=>Path.GetExtension(x).Equals(".excac",StringComparison.OrdinalIgnoreCase)).ToArray());return;}if(ext==".xv2preset"){ImportDroppedPreset(files[0]);return;}OpenSavePath(files[0]);};
    }
    private void OpenSavePath(string path){try{currentSave=new SaveFile(path);RecordOpenedSave(path);RefreshLoadedSave(0,false);ResetChangeHistory();RefreshDashboard();}catch(Exception ex){MessageBox.Show($"Could not open the dropped save:\n\n{ex.Message}","Open Save",MessageBoxButtons.OK,MessageBoxIcon.Error);}}
    private void ImportDroppedCacs(string[] paths){if(currentSave==null){MessageBox.Show("Load a save before dropping CaCs.","Drag and Drop",MessageBoxButtons.OK,MessageBoxIcon.Information);return;}List<ExcacFile> imports=paths.Take(8).Select(ExcacFile.Load).ToList();using CacTargetPicker picker=new(currentSave.Characters,currentSave.Characters.Where(x=>x.IsEmpty).Take(imports.Count).Select(x=>x.Slot-1),"dropped CaC import");if(picker.ShowDialog(this)!=DialogResult.OK||picker.SelectedSlots.Count!=imports.Count){MessageBox.Show($"Select exactly {imports.Count} slots.","Drag and Drop",MessageBoxButtons.OK,MessageBoxIcon.Information);return;}for(int i=0;i<imports.Count;i++)imports[i].ImportInto(currentSave.DecryptedData,picker.SelectedSlots[i]);RefreshLoadedSave(picker.SelectedSlots[0],true);MarkUnsaved();}
    private void ImportDroppedPreset(string path){if(cmbPresets.SelectedItem is not XV2Preset destination){MessageBox.Show("Load a save and select a preset first.","Drag and Drop",MessageBoxButtons.OK,MessageBoxIcon.Information);return;}PresetLibraryFile file=PresetLibraryFile.Load(path);CopyEntirePreset(file.Preset,destination);LoadSelectedPreset();MarkUnsaved();}
}
