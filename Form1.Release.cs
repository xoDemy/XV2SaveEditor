using System.Diagnostics;
using System.Reflection;

namespace XV2SaveEditor;

public partial class Form1
{
    private void ApplyApplicationIcon()
    {
        using Stream? stream = typeof(Form1).Assembly.GetManifestResourceStream("XV2SaveEditor.AppIcon.ico");
        if (stream == null) return;
        using System.Drawing.Icon embedded = new(stream);
        Icon = (System.Drawing.Icon)embedded.Clone();
    }

    private void Form1ReleaseShown(object? sender, EventArgs e)
    {
        EditorPreferences preferences = EditorPreferences.Load();
        if (!preferences.HasSeenWelcome)
        {
            ShowWelcomeGuide();
            preferences = EditorPreferences.Load();
            preferences.HasSeenWelcome = true;
            preferences.Save();
        }
        TryAutoOpenLastSave();
    }

    private static void RecordOpenedSave(string path)
    {
        EditorPreferences preferences = EditorPreferences.Load();
        string fullPath = Path.GetFullPath(path);
        preferences.LastSavePath = fullPath;
        preferences.RecentSavePaths.RemoveAll(item => string.Equals(item, fullPath, StringComparison.OrdinalIgnoreCase));
        preferences.RecentSavePaths.Insert(0, fullPath);
        if (preferences.RecentSavePaths.Count > 10)
            preferences.RecentSavePaths.RemoveRange(10, preferences.RecentSavePaths.Count - 10);
        preferences.Save();
    }

    private void ShowWelcomeGuide()
    {
        using Form dialog = CreateToolDialog("Welcome to XV2 Save Editor", 650, 460);
        Label title = new() { Text = "WELCOME / SAFE EDITING CHECKLIST", Location = new Point(22, 20), AutoSize = true, Font = new Font("Segoe UI Semibold", 14F), ForeColor = ModernTheme.Cyan };
        Label body = new()
        {
            Text = "1. Drag a PC, Xbox, or supported PlayStation save onto the editor.\n\n" +
                   "2. Automatic backups are created before writes. Use Backup Recovery if needed.\n\n" +
                   "3. PC exports must be linked to the intended Steam ID.\n\n" +
                   "4. PlayStation saves may require re-encryption/resigning after editing.\n\n" +
                   "5. Review the change summary and save-health warnings before writing.\n\n" +
                   "Open the ? button at any time for platform instructions and contact links.",
            Location = new Point(24, 68), Size = new Size(590, 270), ForeColor = ModernTheme.Text, Font = new Font("Segoe UI", 10F)
        };
        CheckBox autoOpen = new() { Text = "Automatically reopen my last save", Location = new Point(24, 350), AutoSize = true, Checked = EditorPreferences.Load().AutoOpenLastSave };
        Button continueButton = ToolButton("Continue", 492, 375, (_, _) => dialog.Close());
        continueButton.Click += (_, _) => { EditorPreferences preferences = EditorPreferences.Load(); preferences.AutoOpenLastSave = autoOpen.Checked; preferences.Save(); };
        ModernTheme.StyleButton(continueButton, true);
        dialog.Controls.AddRange(new Control[] { title, body, autoOpen, continueButton });
        ModernTheme.Apply(dialog);
        dialog.ShowDialog(this);
    }

    private void ShowEditorSettings()
    {
        EditorPreferences preferences = EditorPreferences.Load();
        using Form dialog = CreateToolDialog("Editor Settings", 560, 310);
        CheckBox autoOpen = new() { Text = "Automatically reopen the last successfully opened save", Location = new Point(22, 28), AutoSize = true, Checked = preferences.AutoOpenLastSave };
        Label backup = new() { Text = "Backups are organized by platform and source save in Documents\\XV2 Save Editor Backups.", Location = new Point(22, 70), Size = new Size(510, 44), ForeColor = ModernTheme.Muted };
        Button clearRecent = ToolButton("Clear recent saves", 22, 130, (_, _) => { preferences.RecentSavePaths.Clear(); preferences.Save(); MessageBox.Show("Recent-save history cleared.", "Settings"); });
        Button welcome = ToolButton("Show welcome guide", 182, 130, (_, _) => ShowWelcomeGuide());
        Button save = ToolButton("Save settings", 405, 218, (_, _) => { preferences.AutoOpenLastSave = autoOpen.Checked; preferences.Save(); dialog.Close(); });
        ModernTheme.StyleButton(save, true);
        dialog.Controls.AddRange(new Control[] { autoOpen, backup, clearRecent, welcome, save });
        ModernTheme.Apply(dialog);
        dialog.ShowDialog(this);
    }

    private void ShowRecentSaves()
    {
        EditorPreferences preferences = EditorPreferences.Load();
        preferences.RecentSavePaths.RemoveAll(path => !File.Exists(path));
        preferences.Save();
        using Form dialog = CreateToolDialog("Recent Saves", 720, 430);
        ListBox list = new() { Location = new Point(18, 18), Size = new Size(676, 315) };
        foreach (string path in preferences.RecentSavePaths) list.Items.Add(path);
        if (list.Items.Count > 0) list.SelectedIndex = 0;
        Button open = ToolButton("Open selected", 559, 346, (_, _) => { if (list.SelectedItem is string path) { dialog.Close(); OpenSavePath(path); } });
        Button folder = ToolButton("Open folder", 18, 346, (_, _) => { if (list.SelectedItem is string path) Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true }); });
        ModernTheme.StyleButton(open, true);
        dialog.Controls.AddRange(new Control[] { list, folder, open });
        ModernTheme.Apply(dialog);
        dialog.ShowDialog(this);
    }

    private void ShowAboutEditor()
    {
        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? Application.ProductVersion;
        using Form dialog = CreateToolDialog("About XV2 Save Editor", 570, 390);
        Label title = new() { Text = "XV2 SAVE EDITOR", Location = new Point(24, 22), AutoSize = true, Font = new Font("Segoe UI Semibold", 18F), ForeColor = ModernTheme.Cyan };
        Label details = new()
        {
            Text = $"Release candidate {version}\n64-bit Windows / self-contained release\n\n" +
                   "Made with love by: Demyliciouss\nWith help from: Gliscors\n\n" +
                   "Supported containers: PC / Steam, Xbox, and verified PlayStation formats.\n" +
                   "All bulk edits and repairs are limited to verified save structures.",
            Location = new Point(26, 75), Size = new Size(510, 190), ForeColor = ModernTheme.Text
        };
        LinkLabel discord = new() { Text = "Discord support: discord.gg/desurui", Location = new Point(26, 280), AutoSize = true, LinkColor = ModernTheme.Cyan };
        discord.LinkClicked += (_, _) => OpenHelpTarget("https://discord.com/invite/desurui");
        Button close = ToolButton("Close", 415, 305, (_, _) => dialog.Close());
        ModernTheme.StyleButton(close, true);
        dialog.Controls.AddRange(new Control[] { title, details, discord, close });
        ModernTheme.Apply(dialog);
        discord.LinkColor = ModernTheme.Cyan;
        dialog.ShowDialog(this);
    }
}
