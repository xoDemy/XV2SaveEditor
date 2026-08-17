namespace XV2SaveEditor;

public partial class Form1
{
    private Button btnStorePresetLibrary = null!, btnImportPresetLibrary = null!;

    private void ConfigurePresetLibrary()
    {
        btnStorePresetLibrary = new Button { Text = "Store preset" };
        btnImportPresetLibrary = new Button { Text = "Import preset" };
        btnStorePresetLibrary.Click += (_, _) => StoreCurrentPresetInLibrary();
        btnImportPresetLibrary.Click += (_, _) => ImportPresetFromLibrary();
        grpPresetTools.Controls.AddRange(new Control[] { btnStorePresetLibrary, btnImportPresetLibrary });
    }

    private void StoreCurrentPresetInLibrary()
    {
        XV2Preset? preset = GetCurrentPreset(); if (preset == null) return;
        string character = (cmbPresetCharacters.SelectedItem as XV2Character)?.Name ?? "Unknown CaC";
        string suggested = $"{character} - {preset.DisplayName}";
        string? name = PromptPresetName(suggested); if (string.IsNullOrWhiteSpace(name)) return;
        try { string path = PresetLibraryFile.Store(preset, name.Trim(), character); MessageBox.Show($"Preset stored.\n\n{path}", "Preset Library", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        catch (Exception ex) { MessageBox.Show($"Could not store the preset:\n\n{ex.Message}", "Preset Library - Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ImportPresetFromLibrary()
    {
        if (cmbPresets.SelectedItem is not XV2Preset destination) return;
        Directory.CreateDirectory(PresetLibraryFile.LibraryPath);
        using OpenFileDialog dialog = new() { Title = "Choose a preset", Filter = "XV2 Presets (*.xv2preset)|*.xv2preset", InitialDirectory = PresetLibraryFile.LibraryPath, CheckFileExists = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            PresetLibraryFile file = PresetLibraryFile.Load(dialog.FileName);
            PresetImportSelection? selection = SelectPresetComponents(file); if (selection == null) return;
            XV2Preset source = file.Preset;
            if (selection.Clothing) { destination.Top = source.Top; destination.Bottom = source.Bottom; destination.Gloves = source.Gloves; destination.Shoes = source.Shoes; destination.Accessory = source.Accessory; }
            if (selection.Colors) CopyPresetColors(source, destination);
            if (selection.Skills) CopySkillFields(source, destination);
            if (selection.SuperSoul) destination.SuperSoul = source.SuperSoul;
            if (selection.QQBang) destination.QQBang = source.QQBang;
            LoadSelectedPreset(); MarkUnsaved();
        }
        catch (Exception ex) { MessageBox.Show($"Could not import the preset:\n\n{ex.Message}", "Preset Library - Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private static void CopyPresetColors(XV2Preset s, XV2Preset d)
    {
        d.TopColor1=s.TopColor1; d.TopColor2=s.TopColor2; d.TopColor3=s.TopColor3; d.TopColor4=s.TopColor4;
        d.BottomColor1=s.BottomColor1; d.BottomColor2=s.BottomColor2; d.BottomColor3=s.BottomColor3; d.BottomColor4=s.BottomColor4;
        d.GlovesColor1=s.GlovesColor1; d.GlovesColor2=s.GlovesColor2; d.GlovesColor3=s.GlovesColor3; d.GlovesColor4=s.GlovesColor4;
        d.ShoesColor1=s.ShoesColor1; d.ShoesColor2=s.ShoesColor2; d.ShoesColor3=s.ShoesColor3; d.ShoesColor4=s.ShoesColor4;
    }

    private PresetImportSelection? SelectPresetComponents(PresetLibraryFile file)
    {
        using Form form = new() { Text = "Import Preset Components", StartPosition = FormStartPosition.CenterParent, ClientSize = new System.Drawing.Size(450, 340), FormBorderStyle = FormBorderStyle.FixedDialog, MinimizeBox = false, MaximizeBox = false };
        Label info = new() { Text = $"{file.Name}\nSource: {file.SourceCharacter}\n\nChoose what to apply to the currently selected preset:", Location = new System.Drawing.Point(18, 16), Size = new System.Drawing.Size(410, 74) };
        CheckedListBox options = new() { Location = new System.Drawing.Point(18, 96), Size = new System.Drawing.Size(414, 160), CheckOnClick = true };
        options.Items.AddRange(new object[] { "Clothing and accessory", "Clothing colours", "Skills and Awoken", "Super Soul", "QQ Bang" }); for (int i=0;i<options.Items.Count;i++) options.SetItemChecked(i,true);
        Button okay = new() { Text = "Import selected", Location = new System.Drawing.Point(258, 286), Size = new System.Drawing.Size(112, 34), DialogResult = DialogResult.OK };
        Button cancel = new() { Text = "Cancel", Location = new System.Drawing.Point(378, 286), Size = new System.Drawing.Size(70, 34), DialogResult = DialogResult.Cancel };
        form.Controls.AddRange(new Control[] { info, options, okay, cancel }); form.AcceptButton=okay; form.CancelButton=cancel; ModernTheme.Apply(form);
        if (form.ShowDialog(this) != DialogResult.OK) return null;
        return new(options.GetItemChecked(0), options.GetItemChecked(1), options.GetItemChecked(2), options.GetItemChecked(3), options.GetItemChecked(4));
    }

    private string? PromptPresetName(string suggested)
    {
        using Form form = new() { Text="Store Preset", StartPosition=FormStartPosition.CenterParent, ClientSize=new System.Drawing.Size(430,135), FormBorderStyle=FormBorderStyle.FixedDialog, MinimizeBox=false, MaximizeBox=false };
        TextBox value = new() { Text=suggested, Location=new System.Drawing.Point(14,36), Size=new System.Drawing.Size(402,27) };
        Button okay=new() { Text="Store", Location=new System.Drawing.Point(254,84), Size=new System.Drawing.Size(76,32), DialogResult=DialogResult.OK }; Button cancel=new() { Text="Cancel", Location=new System.Drawing.Point(340,84), Size=new System.Drawing.Size(76,32), DialogResult=DialogResult.Cancel };
        form.Controls.Add(new Label { Text="Preset name", Location=new System.Drawing.Point(14,13), AutoSize=true }); form.Controls.AddRange(new Control[]{value,okay,cancel}); form.AcceptButton=okay; form.CancelButton=cancel; ModernTheme.Apply(form); value.SelectAll();
        return form.ShowDialog(this)==DialogResult.OK ? value.Text : null;
    }

    private sealed record PresetImportSelection(bool Clothing, bool Colors, bool Skills, bool SuperSoul, bool QQBang);
}
