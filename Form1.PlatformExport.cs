using System.Drawing;

namespace XV2SaveEditor;

public partial class Form1
{
    private Button? btnPlatformExport;
    private Button? btnLinkSteamId;

    private void ConfigurePlatformExport()
    {
        btnPlatformExport = new() { Text = "Export to Platform", Location = new Point(270, 30), Size = new Size(155, 32) };
        btnLinkSteamId = new() { Text = "Link Steam ID", Location = new Point(435, 30), Size = new Size(140, 32) };
        btnPlatformExport.Click += (_, _) => ShowPlatformExport();
        btnLinkSteamId.Click += (_, _) => ShowSteamLinkDialog();
        grpGeneral.Controls.AddRange(new Control[] { btnPlatformExport, btnLinkSteamId });
        grpGeneral.Resize += (_, _) => LayoutSaveControlBar();
    }

    private void LayoutSaveControlBar()
    {
        if (btnPlatformExport == null || btnLinkSteamId == null || grpGeneral.Width <= 0)
            return;

        bool compact = grpGeneral.ClientSize.Width < 1100;
        int x = compact ? 14 : 20;
        int gap = compact ? 7 : 10;

        Place(btnOpenSave, compact ? 98 : 110);
        Place(btnSave, compact ? 108 : 120);
        Place(btnPlatformExport, compact ? 128 : 155);
        Place(btnLinkSteamId, compact ? 112 : 140);

        x += compact ? 10 : 15;
        Place(lblZeniTitle, compact ? 38 : 50, 29, 27);
        Place(nudZeni, compact ? 88 : 130, 25, 30);
        Place(btnMaxZeni, compact ? 42 : 52, 28, 29);

        x += compact ? 8 : 8;
        Place(lblTPMedalsTitle, compact ? 62 : 82, 29, 27);
        Place(nudTPMedals, compact ? 88 : 120, 25, 30);
        Place(btnMaxTP, compact ? 42 : 48, 28, 29);

        void Place(Control control, int width, int height = 32, int top = 30)
        {
            control.Location = new Point(x, top);
            control.Size = new Size(width, height);
            x += width + gap;
        }
    }

    private void ShowSteamLinkDialog()
    {
        if (currentSave == null) return;
        using Form dialog = CreateToolDialog("Link Save to Steam ID", 510, 265);
        dialog.Controls.Add(new Label { Text = "Enter a SteamID64 or the shorter numeric account ID.\nYou can also copy ownership from a known-working PC save.", Location = new Point(18, 16), Size = new Size(465, 44), ForeColor = ModernTheme.Muted });
        TextBox id = new() { Location = new Point(18, 72), Width = 465, Text = SteamOwnership.ReadSteamId64(currentSave.DecryptedData).ToString() };
        Button donor = ToolButton("Use donor PC save", 18, 116, (_, _) =>
        {
            using OpenFileDialog open = new() { Filter = "PC XV2 Save (*.sav)|*.sav", CheckFileExists = true };
            if (open.ShowDialog(dialog) != DialogResult.OK) return;
            SaveFile donorSave = new(open.FileName);
            if (donorSave.Platform != SavePlatform.PC) throw new InvalidDataException("The donor must be a PC / Steam save.");
            id.Text = SteamOwnership.ReadSteamId64(donorSave.DecryptedData).ToString();
        });
        Button apply = ToolButton("Link Steam ID", 343, 182, (_, _) =>
        {
            if (!ulong.TryParse(id.Text.Trim(), out ulong value)) { MessageBox.Show("Enter a numeric SteamID64 or account ID."); return; }
            ulong normalized = SteamOwnership.Normalize(value);
            SteamOwnership.WriteSteamId64(currentSave.DecryptedData, normalized);
            MarkUnsaved();
            MessageBox.Show($"Save linked to SteamID64 {normalized}.\n\nThis change will be included the next time you save or export to PC.", "Steam ID Linked", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dialog.DialogResult = DialogResult.OK;
        });
        ModernTheme.StyleButton(apply, true);
        dialog.Controls.AddRange(new Control[] { id, donor, apply });
        ModernTheme.Apply(dialog);
        dialog.ShowDialog(this);
    }

    private void ShowPlatformExport()
    {
        if (currentSave == null) return;
        StoreCurrentCharacterControls();
        using Form dialog = CreateToolDialog("Export Save to Platform", 510, 305);
        dialog.Controls.Add(new Label { Text = $"Source: {PlatformSaveAdapter.DisplayName(currentSave.Platform)}\nChoose the output container. The loaded save is not changed.", Location = new Point(18, 16), Size = new Size(465, 44), ForeColor = ModernTheme.Muted });
        ComboBox target = new() { Location = new Point(18, 72), Width = 465, DropDownStyle = ComboBoxStyle.DropDownList };
        target.Items.AddRange(new object[] { "PC / Steam (.sav)", "Xbox (.bin)", "PlayStation encrypted (.DAT)", "PlayStation decrypted (.DAT)" });
        target.SelectedIndex = 0;
        Label warning = new() { Text = "Console exports may still require profile resigning. PC exports must be linked to the intended Steam account.", Location = new Point(18, 116), Size = new Size(465, 44), ForeColor = ModernTheme.Cyan };
        Button export = ToolButton("Choose output & export", 283, 222, (_, _) =>
        {
            SavePlatform platform = target.SelectedIndex switch { 0 => SavePlatform.PC, 1 => SavePlatform.Xbox, 2 => SavePlatform.PlayStationEncrypted, _ => SavePlatform.PlayStation };
            if (platform is SavePlatform.PlayStation or SavePlatform.PlayStationEncrypted &&
                currentSave.Platform is not SavePlatform.PlayStation and not SavePlatform.PlayStationEncrypted)
            {
                MessageBox.Show("PC/Xbox → PlayStation requires a donor PS save so its platform-specific header and key material can be preserved. This unsafe direction is not enabled yet.", "PlayStation Donor Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (platform == SavePlatform.PC && currentSave.Platform != SavePlatform.PC &&
                MessageBox.Show($"This PC export is currently linked to SteamID64 {SteamOwnership.ReadSteamId64(currentSave.DecryptedData)}.\n\nContinue with that owner? Choose No to use Link Steam ID first.", "Confirm Steam Owner", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            string extension = platform == SavePlatform.PC ? "sav" : platform == SavePlatform.Xbox ? "bin" : "DAT";
            using SaveFileDialog saveDialog = new() { Filter = $"{target.SelectedItem}|*.{extension}|All Files (*.*)|*.*", FileName = $"Converted_{Path.GetFileNameWithoutExtension(currentSave.FilePath)}.{extension}" };
            if (saveDialog.ShowDialog(dialog) != DialogResult.OK) return;
            if (File.Exists(saveDialog.FileName)) SaveFile.CreateBackupForPath(saveDialog.FileName);
            byte[] encoded = PlatformSaveAdapter.Encode(platform, currentSave.DecryptedData);
            File.WriteAllBytes(saveDialog.FileName, encoded);
            PlatformSaveData verified = PlatformSaveAdapter.Load(saveDialog.FileName);
            if (verified.Platform != platform || !CanonicalContentMatches(currentSave.DecryptedData, verified.DecryptedData, platform))
                throw new InvalidDataException("Converted output failed internal round-trip validation.");
            MessageBox.Show($"Conversion verified successfully.\n\n{PlatformSaveAdapter.DisplayName(currentSave.Platform)} → {PlatformSaveAdapter.DisplayName(platform)}\n{saveDialog.FileName}\n\nConsole profile resigning may still be required.", "Platform Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dialog.DialogResult = DialogResult.OK;
        });
        ModernTheme.StyleButton(export, true);
        dialog.Controls.AddRange(new Control[] { target, warning, export });
        ModernTheme.Apply(dialog);
        dialog.ShowDialog(this);
    }

    private static bool CanonicalContentMatches(byte[] source, byte[] converted, SavePlatform target)
    {
        if (source.Length != converted.Length) return false;
        for (int i = 0; i < source.Length; i++)
        {
            bool platformMetadata = target == SavePlatform.Xbox && (i is >= 0x08 and < 0x18 || i >= source.Length - 16);
            if (!platformMetadata && source[i] != converted[i]) return false;
        }
        return true;
    }
}
