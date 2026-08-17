using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public partial class Form1
    {
        private Button selectiveAppearanceTransfer = null!;

        private void ConfigureAdvancedTransferTools()
        {
            Button batch = new Button { Text = "Batch Loadout Tools", Location = new Point(850, 27), Size = new Size(180, 32), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            batch.Click += (_, _) => ShowBatchLoadoutTools();
            grpPresetSelection.Controls.Add(batch);

            selectiveAppearanceTransfer = new Button { Text = "Selective Appearance Transfer", Location = new Point(300, 180), Size = new Size(300, 32), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            selectiveAppearanceTransfer.Click += (_, _) => ShowAppearanceTransferTools();
            grpAppearance.Controls.Add(selectiveAppearanceTransfer);
        }

        private void ShowBatchLoadoutTools()
        {
            if (currentSave == null || cmbPresetCharacters.SelectedItem is not XV2Character character || cmbPresets.SelectedItem is not XV2Preset source) return;
            StorePresetControlsInSelectedPreset();
            using Form dialog = CreateToolDialog("Batch Loadout Tools", 540, 545);
            dialog.Controls.Add(new Label { Text = $"Source: {character.Name} • {source.DisplayName}\nSelect fields and destination presets.", Location = new Point(18, 16), Size = new Size(490, 44), ForeColor = ModernTheme.Muted });
            CheckedListBox fields = new CheckedListBox { Location = new Point(18, 68), Size = new Size(240, 330), CheckOnClick = true };
            fields.Items.AddRange(new object[] { "Top", "Bottom", "Gloves", "Shoes", "Accessory", "Clothing colours", "Super Soul", "QQ Bang", "Super skills", "Ultimate skills", "Evasive", "Awoken" });
            CheckedListBox targets = new CheckedListBox { Location = new Point(280, 68), Size = new Size(240, 330), CheckOnClick = true };
            foreach (XV2Preset preset in character.Presets) targets.Items.Add(preset, preset.Index != source.Index);
            Button allFields = ToolButton("All fields", 18, 414, (_, _) => CheckAll(fields, true));
            Button clear = ToolButton("Clear", 148, 414, (_, _) => CheckAll(fields, false));
            Button allTargets = ToolButton("All targets", 280, 414, (_, _) => CheckAll(targets, true));
            Button apply = ToolButton("Apply batch", 390, 472, (_, _) => { ApplyBatchLoadout(character, source, fields, targets); dialog.DialogResult = DialogResult.OK; });
            ModernTheme.StyleButton(apply, true);
            dialog.Controls.AddRange(new Control[] { fields, targets, allFields, clear, allTargets, apply });
            ModernTheme.Apply(dialog);
            dialog.ShowDialog(this);
        }

        private void ApplyBatchLoadout(XV2Character character, XV2Preset source, CheckedListBox fields, CheckedListBox targets)
        {
            HashSet<int> selectedFields = fields.CheckedIndices.Cast<int>().ToHashSet();
            List<XV2Preset> selectedTargets = targets.CheckedItems.Cast<XV2Preset>().ToList();
            if (selectedFields.Count == 0 || selectedTargets.Count == 0) return;
            if (MessageBox.Show($"Apply {selectedFields.Count} field group(s) from {source.DisplayName} to {selectedTargets.Count} preset(s)?", "Batch Loadout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            foreach (XV2Preset target in selectedTargets)
            {
                if (selectedFields.Contains(0)) target.Top = source.Top;
                if (selectedFields.Contains(1)) target.Bottom = source.Bottom;
                if (selectedFields.Contains(2)) target.Gloves = source.Gloves;
                if (selectedFields.Contains(3)) target.Shoes = source.Shoes;
                if (selectedFields.Contains(4)) target.Accessory = source.Accessory;
                if (selectedFields.Contains(5)) CopyClothingColours(source, target);
                if (selectedFields.Contains(6)) target.SuperSoul = source.SuperSoul;
                if (selectedFields.Contains(7)) target.QQBang = source.QQBang;
                if (selectedFields.Contains(8)) { target.SuperSkill1 = source.SuperSkill1; target.SuperSkill2 = source.SuperSkill2; target.SuperSkill3 = source.SuperSkill3; target.SuperSkill4 = source.SuperSkill4; }
                if (selectedFields.Contains(9)) { target.UltimateSkill1 = source.UltimateSkill1; target.UltimateSkill2 = source.UltimateSkill2; }
                if (selectedFields.Contains(10)) target.EvasiveSkill = source.EvasiveSkill;
                if (selectedFields.Contains(11)) target.AwokenSkill = source.AwokenSkill;
            }
            PresetWriter.WritePresets(currentSave!.DecryptedData, character.Slot - 1, character.Presets);
            LoadSelectedPreset();
            MarkUnsaved();
        }

        private static void CopyClothingColours(XV2Preset source, XV2Preset target)
        {
            target.TopColor1 = source.TopColor1; target.TopColor2 = source.TopColor2; target.TopColor3 = source.TopColor3; target.TopColor4 = source.TopColor4;
            target.BottomColor1 = source.BottomColor1; target.BottomColor2 = source.BottomColor2; target.BottomColor3 = source.BottomColor3; target.BottomColor4 = source.BottomColor4;
            target.GlovesColor1 = source.GlovesColor1; target.GlovesColor2 = source.GlovesColor2; target.GlovesColor3 = source.GlovesColor3; target.GlovesColor4 = source.GlovesColor4;
            target.ShoesColor1 = source.ShoesColor1; target.ShoesColor2 = source.ShoesColor2; target.ShoesColor3 = source.ShoesColor3; target.ShoesColor4 = source.ShoesColor4;
        }

        private void ShowAppearanceTransferTools()
        {
            if (currentSave == null || cmbCharacters.SelectedItem is not XV2Character source) return;
            StoreCurrentCharacterControls();
            XV2Appearance sourceAppearance = AppearanceReader.ReadAppearance(currentSave.DecryptedData, source.Slot - 1);
            using Form dialog = CreateToolDialog("Selective Appearance Transfer", 520, 510);
            dialog.Controls.Add(new Label { Text = $"Copy selected appearance parts from {source.Name} to another CaC.", Location = new Point(18, 16), AutoSize = true, ForeColor = ModernTheme.Muted });
            ComboBox target = new ComboBox { Location = new Point(18, 48), Size = new Size(484, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (XV2Character character in currentSave.Characters.Where(character => !character.IsEmpty && character.Slot != source.Slot)) target.Items.Add(character);
            if (target.Items.Count > 0) target.SelectedIndex = 0;
            CheckedListBox parts = new CheckedListBox { Location = new Point(18, 90), Size = new Size(484, 300), CheckOnClick = true };
            parts.Items.AddRange(new object[] { "Body shape", "Skin colours", "Hair colour", "Eye colour", "Makeup colours", "Face base", "Forehead", "Eyes", "Nose", "Ears", "Hair" });
            Button selectAll = ToolButton("Select all", 18, 410, (_, _) => CheckAll(parts, true));
            Button apply = ToolButton("Transfer", 372, 450, (_, _) =>
            {
                if (target.SelectedItem is XV2Character destination && TransferAppearance(source, destination, sourceAppearance, parts)) dialog.DialogResult = DialogResult.OK;
            });
            ModernTheme.StyleButton(apply, true);
            dialog.Controls.AddRange(new Control[] { target, parts, selectAll, apply });
            ModernTheme.Apply(dialog);
            dialog.ShowDialog(this);
        }

        private bool TransferAppearance(XV2Character source, XV2Character destination, XV2Appearance from, CheckedListBox parts)
        {
            HashSet<int> selected = parts.CheckedIndices.Cast<int>().ToHashSet();
            if (selected.Count == 0) return false;
            if (source.Race != destination.Race && MessageBox.Show($"{source.Name} and {destination.Name} use different races. Some appearance IDs may not exist for the destination race. Continue with the selected parts?", "Race Compatibility", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return false;
            XV2Appearance to = AppearanceReader.ReadAppearance(currentSave!.DecryptedData, destination.Slot - 1);
            if (selected.Contains(0)) to.BodyShape = from.BodyShape;
            if (selected.Contains(1)) { to.SkinColor1 = from.SkinColor1; to.SkinColor2 = from.SkinColor2; to.SkinColor3 = from.SkinColor3; to.SkinColor4 = from.SkinColor4; }
            if (selected.Contains(2)) to.HairColor = from.HairColor;
            if (selected.Contains(3)) to.EyeColor = from.EyeColor;
            if (selected.Contains(4)) { to.MakeupColor1 = from.MakeupColor1; to.MakeupColor2 = from.MakeupColor2; to.MakeupColor3 = from.MakeupColor3; }
            if (selected.Contains(5)) to.FaceBase = from.FaceBase;
            if (selected.Contains(6)) to.FaceForehead = from.FaceForehead;
            if (selected.Contains(7)) to.Eyes = from.Eyes;
            if (selected.Contains(8)) to.Nose = from.Nose;
            if (selected.Contains(9)) to.Ears = from.Ears;
            if (selected.Contains(10)) to.Hair = from.Hair;
            AppearanceWriter.WriteAppearance(currentSave.DecryptedData, destination.Slot - 1, to.BodyShape, to.SkinColor1, to.SkinColor2, to.SkinColor3, to.SkinColor4, to.HairColor, to.EyeColor, to.MakeupColor1, to.MakeupColor2, to.MakeupColor3, to.FaceBase, to.FaceForehead, to.Eyes, to.Nose, to.Ears, to.Hair);
            RefreshLoadedSave(destination.Slot - 1, preserveDirty: true);
            MarkUnsaved();
            return true;
        }

        private static Form CreateToolDialog(string title, int width, int height) => new Form { Text = title, ClientSize = new Size(width, height), FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, StartPosition = FormStartPosition.CenterParent, BackColor = ModernTheme.Window, ForeColor = ModernTheme.Text, Font = new Font("Segoe UI", 9.5F) };
        private static Button ToolButton(string text, int x, int y, EventHandler handler) { Button button = new Button { Text = text, Location = new Point(x, y), Size = new Size(120, 32) }; button.Click += handler; return button; }
        private static void CheckAll(CheckedListBox list, bool value) { for (int i = 0; i < list.Items.Count; i++) list.SetItemChecked(i, value); }
    }
}
