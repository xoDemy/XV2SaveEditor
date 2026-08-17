using System.Drawing;

namespace XV2SaveEditor;

public partial class Form1
{
    private ToolTip? editorHelpToolTip;
    private const string SteamIdUrl = "https://steamid.io/";
    private const string ExampleSteamSaveFolder = @"C:\Program Files (x86)\Steam\userdata\<YOUR_STEAM_USER_ID>\323470\remote\DBXV21";

    private const string EditorHelpText =
        "CTRL + Z - Undo\r\n" +
        "CTRL + Y - Redo\r\n\r\n" +
        "Drag SDATA File onto Editor, and then edit file as you please. Then, press \"Save\" button to save file onto PC. Once done, please re-encrypt using a Discord bot (like HTOS) for PlayStation. For XBOX saves, rename to original save name. If difficulty, please refer to these videos (will make videos in a bit for it).\r\n\r\n" +
        "For Steam saves, drag DBXV2.sav file onto editor and edit to your liking. Once completed, save and drag file onto original file location, or copy this path:\r\n" +
        "C:\\Program Files (x86)\\Steam\\userdata\\<YOUR_STEAM_USER_ID>\\323470\\remote\\DBXV21\r\n\r\n" +
        "If needed to resign to a different Steam account, open https://steamid.io/ and find your Steam ID, usually located here. Copy the end of the numbers to find your Steam ID, then click \"Link Steam ID\".\r\n\r\n" +
        "CONTACT\r\n" +
        "Discord: demyliciouss\r\n" +
        "Discord: gliscors\r\n" +
        "https://discord.com/invite/desurui\r\n" +
        "https://discord.gg/rrpvUequwX\r\n\r\n" +
        "SPECIAL THANKS TO\r\n" +
        "https://gitlab.com/BawsDeep - for the XV2PStoPC Tool <3\r\n" +
        "https://github.com/mineminemine - for the Xbox save decrypter <3\r\n" +
        "https://github.com/gabrieluto - Extensive research on Xenoverse 2 PS4 saves and detailed spreadsheet of offsets.";

    private void ConfigureEditorHelp(Control header)
    {
        Button help = new()
        {
            Text = "?",
            AccessibleName = "Editor help and tooltips",
            Location = new Point(850, 13),
            Size = new Size(36, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Font = new Font("Segoe UI Semibold", 11F),
            Cursor = Cursors.Help
        };

        editorHelpToolTip = new ToolTip
        {
            AutoPopDelay = 30000,
            InitialDelay = 350,
            ReshowDelay = 100,
            ShowAlways = true,
            ToolTipTitle = "XV2 Save Editor Help"
        };
        editorHelpToolTip.SetToolTip(help, EditorHelpText);
        help.Click += (_, _) => ShowEditorHelp();
        header.Controls.Add(help);
        ModernTheme.StyleButton(help, false);
    }

    private void ShowEditorHelp()
    {
        using Form dialog = CreateToolDialog("XV2 Save Editor Help", 690, 555);
        RichTextBox instructions = new()
        {
            Text = EditorHelpText,
            Location = new Point(18, 18),
            Size = new Size(646, 376),
            ReadOnly = true,
            DetectUrls = true,
            BackColor = ModernTheme.Surface,
            ForeColor = ModernTheme.Text,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10F)
        };
        instructions.LinkClicked += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.LinkText))
                OpenHelpTarget(e.LinkText);
        };

        LinkLabel steamIdLink = new()
        {
            Text = "Open SteamID.io",
            Location = new Point(18, 410),
            AutoSize = true,
            LinkColor = ModernTheme.Cyan,
            ActiveLinkColor = ModernTheme.Purple,
            VisitedLinkColor = ModernTheme.Cyan
        };
        steamIdLink.LinkClicked += (_, _) => OpenHelpTarget(SteamIdUrl);

        LinkLabel folderLink = new()
        {
            Text = "Open example Steam save folder",
            Location = new Point(18, 441),
            AutoSize = true,
            LinkColor = ModernTheme.Cyan,
            ActiveLinkColor = ModernTheme.Purple,
            VisitedLinkColor = ModernTheme.Cyan
        };
        folderLink.LinkClicked += (_, _) => OpenHelpTarget(ExampleSteamSaveFolder);

        Button settings = ToolButton("Settings", 18, 470, (_, _) => ShowEditorSettings());
        Button about = ToolButton("About", 153, 470, (_, _) => ShowAboutEditor());
        Button close = ToolButton("Close", 539, 470, (_, _) => dialog.Close());
        ModernTheme.StyleButton(close, true);
        dialog.Controls.AddRange(new Control[] { instructions, steamIdLink, folderLink, settings, about, close });
        ModernTheme.Apply(dialog);
        steamIdLink.LinkColor = folderLink.LinkColor = ModernTheme.Cyan;
        dialog.ShowDialog(this);
    }

    private void OpenHelpTarget(string target)
    {
        try
        {
            if (target.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(target) { UseShellExecute = true });
                return;
            }

            if (!Directory.Exists(target))
            {
                MessageBox.Show(
                    "That example folder does not exist on this PC. Your Steam account-number folder may be different, so open Steam\\userdata and select your own account folder.",
                    "Steam Folder Not Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", $"\"{target}\"") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open the link:\n\n{ex.Message}", "Open Link", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
