using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public partial class Form1
    {
        private const int MiraOutfitId = 106;
        private const int CamoBikiniId = 274;
        private readonly Dictionary<int, byte[]> invisibilitySnapshots = new Dictionary<int, byte[]>();

        private void ConfigureInvisibilityTools()
        {
            grpSpecialAppearance.Size = new Size(300, 320);
            btnResetAppearance.Location = new Point(15, 275);
            btnCopyAppearance.Location = new Point(155, 275);
            Button giantVisuals = new Button { Text = "Giant Namekian / OP Visuals", Location = new Point(15, 190), Size = new Size(270, 34) };
            Button invisibility = new Button { Text = "Invisibility Tools", Location = new Point(15, 230), Size = new Size(270, 34) };
            invisibility.Click += (_, _) => ShowInvisibilityDialog();
            giantVisuals.Click += (_, _) => ApplyGiantNamekianVisuals();
            grpSpecialAppearance.Controls.AddRange(new Control[] { giantVisuals, invisibility });
        }

        private void ApplyGiantNamekianVisuals()
        {
            if (currentSave == null || cmbCharacters.SelectedItem is not XV2Character character) return;
            if (MessageBox.Show($"Apply the verified Giant Namekian height/weight value (12) to {character.Name}?\n\nThis changes only Body Shape.",
                "Giant Namekian / OP Visuals", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            StoreCurrentCharacterControls();
            int slot = character.Slot - 1;
            XV2Appearance appearance = AppearanceReader.ReadAppearance(currentSave.DecryptedData, slot);
            appearance.BodyShape = 12;
            AppearanceWriter.WriteAppearance(currentSave.DecryptedData, slot, appearance.BodyShape,
                appearance.SkinColor1, appearance.SkinColor2, appearance.SkinColor3, appearance.SkinColor4,
                appearance.HairColor, appearance.EyeColor, appearance.MakeupColor1, appearance.MakeupColor2, appearance.MakeupColor3,
                appearance.FaceBase, appearance.FaceForehead, appearance.Eyes, appearance.Nose, appearance.Ears, appearance.Hair);
            RefreshLoadedSave(slot, preserveDirty: true);
            MarkUnsaved();
        }

        private void ShowInvisibilityDialog()
        {
            if (currentSave == null || cmbCharacters.SelectedItem is not XV2Character character) return;
            using Form dialog = new Form
            {
                Text = "CaC Invisibility Tools",
                ClientSize = new Size(470, 430),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = ModernTheme.Window,
                ForeColor = ModernTheme.Text,
                Font = new Font("Segoe UI", 9.5F)
            };
            Label description = new Label
            {
                Text = $"{character.Name} • {character.RaceName}\nChoose which parts should use an invisible/empty component. Clothing is applied to all eight presets.",
                Location = new Point(18, 16), Size = new Size(430, 48), ForeColor = ModernTheme.Muted
            };
            CheckedListBox parts = new CheckedListBox
            {
                Location = new Point(18, 72), Size = new Size(434, 245), CheckOnClick = true,
                BackColor = ModernTheme.Surface, ForeColor = ModernTheme.Text, BorderStyle = BorderStyle.FixedSingle
            };
            parts.Items.AddRange(new object[]
            {
                "Top outfit", "Bottom outfit", "Gloves", "Shoes",
                "Face base", "Forehead", "Eyes", "Nose", "Ears", "Hair"
            });
            Button selectAll = new Button { Text = "Select all", Location = new Point(18, 330), Size = new Size(100, 32) };
            Button clear = new Button { Text = "Clear", Location = new Point(128, 330), Size = new Size(90, 32) };
            Button revert = new Button { Text = "Revert", Location = new Point(228, 330), Size = new Size(90, 32) };
            Button apply = new Button { Text = "Apply", Location = new Point(328, 330), Size = new Size(124, 32), DialogResult = DialogResult.OK };
            Label outfit = new Label
            {
                Text = IsFemaleRace(character.Race) ? "Female preset: Mira's Clothes (ID 106)" : "Male preset: Camo Bikini (ID 274)",
                Location = new Point(18, 380), AutoSize = true, ForeColor = ModernTheme.Cyan
            };
            selectAll.Click += (_, _) => { for (int i = 0; i < parts.Items.Count; i++) parts.SetItemChecked(i, true); };
            clear.Click += (_, _) => { for (int i = 0; i < parts.Items.Count; i++) parts.SetItemChecked(i, false); };
            revert.Click += (_, _) => { RevertInvisibility(character.Slot - 1); dialog.Close(); };
            dialog.Controls.AddRange(new Control[] { description, parts, selectAll, clear, revert, apply, outfit });
            ModernTheme.Apply(dialog);
            ModernTheme.StyleButton(apply, true);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            HashSet<int> selected = parts.CheckedIndices.Cast<int>().ToHashSet();
            if (selected.Count == 0) return;
            ApplyInvisibility(character.Slot - 1, selected);
        }

        private void ApplyInvisibility(int slot, HashSet<int> selected)
        {
            if (currentSave == null) return;
            StoreCurrentCharacterControls();
            if (!invisibilitySnapshots.ContainsKey(slot))
                invisibilitySnapshots[slot] = currentSave.DecryptedData.AsSpan(ExcacFile.BaseOffset + slot * ExcacFile.BaseLength, ExcacFile.BaseLength).ToArray();

            XV2Character character = currentSave.Characters[slot];
            int outfitId = IsFemaleRace(character.Race) ? MiraOutfitId : CamoBikiniId;
            foreach (XV2Preset preset in character.Presets)
            {
                if (selected.Contains(0)) preset.Top = outfitId;
                if (selected.Contains(1)) preset.Bottom = outfitId;
                if (selected.Contains(2)) preset.Gloves = outfitId;
                if (selected.Contains(3)) preset.Shoes = outfitId;
            }
            if (selected.Any(index => index <= 3)) PresetWriter.WritePresets(currentSave.DecryptedData, slot, character.Presets);

            XV2Appearance appearance = character.Appearance ?? AppearanceReader.ReadAppearance(currentSave.DecryptedData, slot);
            if (selected.Contains(4)) appearance.FaceBase = -1;
            if (selected.Contains(5)) appearance.FaceForehead = -1;
            if (selected.Contains(6)) appearance.Eyes = -1;
            if (selected.Contains(7)) appearance.Nose = -1;
            if (selected.Contains(8)) appearance.Ears = -1;
            if (selected.Contains(9)) appearance.Hair = -1;
            AppearanceWriter.WriteAppearance(currentSave.DecryptedData, slot, appearance.BodyShape,
                appearance.SkinColor1, appearance.SkinColor2, appearance.SkinColor3, appearance.SkinColor4,
                appearance.HairColor, appearance.EyeColor, appearance.MakeupColor1, appearance.MakeupColor2, appearance.MakeupColor3,
                appearance.FaceBase, appearance.FaceForehead, appearance.Eyes, appearance.Nose, appearance.Ears, appearance.Hair);
            RefreshLoadedSave(slot, preserveDirty: true);
            MarkUnsaved();
        }

        private void RevertInvisibility(int slot)
        {
            if (currentSave == null || !invisibilitySnapshots.TryGetValue(slot, out byte[]? snapshot))
            {
                MessageBox.Show("No invisibility snapshot exists for this CaC in the current session.", "Revert Invisibility", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            snapshot.CopyTo(currentSave.DecryptedData, ExcacFile.BaseOffset + slot * ExcacFile.BaseLength);
            invisibilitySnapshots.Remove(slot);
            RefreshLoadedSave(slot, preserveDirty: true);
            MarkUnsaved();
        }

        private static bool IsFemaleRace(int race) => race is 1 or 3 or 7;
    }
}
