using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public partial class Form1
    {
        private TabPage tabMaxOut = null!;

        private void ConfigureMaxOutWizard()
        {
            tabMaxOut = new TabPage("Max-Out Wizard");
            Panel card = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28) };
            Label title = new Label { Text = "VERIFIED MAX-OUT WIZARD", AutoSize = true, Location = new System.Drawing.Point(28, 25), Font = new System.Drawing.Font("Segoe UI Semibold", 16F), ForeColor = ModernTheme.Cyan };
            Label description = new Label { Text = "Build a safe completion package for one CaC or every occupied CaC.\nOnly structures already verified by this editor are changed; undocumented fields remain excluded.", Location = new System.Drawing.Point(30, 68), Size = new System.Drawing.Size(800, 52), ForeColor = ModernTheme.Muted };
            Button launch = new Button { Text = "Open Max-Out Wizard", Location = new System.Drawing.Point(30, 145), Size = new System.Drawing.Size(230, 42) };
            launch.Click += (_, _) => ShowMaxOutWizard();
            Label safety = new Label { Text = "A timestamped backup is created automatically before the package is applied.", Location = new System.Drawing.Point(30, 205), AutoSize = true };
            card.Controls.AddRange(new Control[] { title, description, launch, safety });
            tabMaxOut.Controls.Add(card);
            tabMain.TabPages.Add(tabMaxOut);
        }

        private void ShowMaxOutWizard()
        {
            if (currentSave == null) return;
            using Form dialog = CreateToolDialog("Verified Max-Out Wizard", 650, 610);
            ComboBox preset = new ComboBox { Location = new System.Drawing.Point(20, 48), Size = new System.Drawing.Size(280, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            preset.Items.AddRange(new object[] { "Safe Max", "Completionist", "Everything Verified", "Custom" }); preset.SelectedIndex = 0;
            List<int> selectedTargets = currentSave.Characters.Where(x => !x.IsEmpty).Select(x => x.Slot - 1).ToList();
            Button target = new Button { Location = new System.Drawing.Point(320, 46), Size = new System.Drawing.Size(310, 30), Text = TargetSummary(selectedTargets) };
            target.Click += (_, _) => { using CacTargetPicker picker = new(currentSave.Characters, selectedTargets, "max-out package"); if (picker.ShowDialog(dialog) == DialogResult.OK) { selectedTargets = picker.SelectedSlots; target.Text = TargetSummary(selectedTargets); } };
            dialog.Controls.Add(new Label { Text = "Package", Location = new System.Drawing.Point(20, 22), AutoSize = true });
            dialog.Controls.Add(new Label { Text = "Target", Location = new System.Drawing.Point(320, 22), AutoSize = true });
            CheckedListBox options = new CheckedListBox { Location = new System.Drawing.Point(20, 92), Size = new System.Drawing.Size(610, 390), CheckOnClick = true };
            options.Items.AddRange(new object[]
            {
                "Level 199 + correct XP + 600 attribute points", "Verified Guru / Whis level-cap flags",
                "Max Zeni and TP Medals", "Unlock all verified skills and Awokens",
                "Infinite Dragon Balls (125)", "Customisation Keys 1–20",
                "Clear all existing quests with Z rank", "Max all existing quest scores",
                "Max mentor friendship and Dual Ultimate", "Complete all existing Tokipedia paths",
                "Unlock verified artwork and mascots",
                "Give all catalogued clothes (125 each)",
                "Unlock all verified Festival mentor presets",
                "Give one perfect all +5 QQ Bang (99 quantity)",
                "Give all catalogued Accessories (125 each)",
                "Give all catalogued Super Souls (125 each)",
                "Give all catalogued Mix Items (125 each)",
                "Give all catalogued Capsules (125 each)"
            });
            void ApplyPreset()
            {
                bool[] values = preset.SelectedIndex switch
                {
                    0 => new[] { true, true, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
                    1 => Enumerable.Repeat(true, options.Items.Count).ToArray(),
                    2 => Enumerable.Repeat(true, options.Items.Count).ToArray(),
                    _ => options.CheckedIndices.Cast<int>().Select(_ => false).ToArray()
                };
                if (preset.SelectedIndex < 3) for (int i = 0; i < options.Items.Count; i++) options.SetItemChecked(i, values[i]);
            }
            preset.SelectedIndexChanged += (_, _) => ApplyPreset(); ApplyPreset();
            Button review = ToolButton("Review & Apply", 490, 540, (_, _) =>
            {
                List<int> selected = options.CheckedIndices.Cast<int>().ToList();
                if (selected.Count == 0) return;
                List<int> slots = selectedTargets.ToList();
                string summary = $"Apply {selected.Count} verified section(s) to {slots.Count} CaC(s)?\n\n" + string.Join("\n", selected.Select(i => "• " + options.Items[i])) + "\n\nUndocumented flags will remain unchanged.";
                if (MessageBox.Show(summary, "Review Max-Out Package", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK) return;
                ApplyMaxOutPackage(selected, slots); dialog.DialogResult = DialogResult.OK;
            });
            ModernTheme.StyleButton(review, true);
            dialog.Controls.AddRange(new Control[] { preset, target, options, review }); ModernTheme.Apply(dialog); dialog.ShowDialog(this);
        }

        private string TargetSummary(IReadOnlyList<int> slots)
        {
            if (currentSave == null || slots.Count == 0) return "Choose CaCs";
            int occupied = currentSave.Characters.Count(x => !x.IsEmpty);
            if (slots.Count == occupied) return $"All occupied CaCs ({slots.Count})";
            return slots.Count <= 3 ? string.Join(", ", slots.Select(x => $"Slot {x + 1}")) : $"{slots.Count} selected CaCs";
        }

        private void ApplyMaxOutPackage(List<int> options, List<int> slots)
        {
            if (currentSave == null) return;
            string backup = currentSave.CreateBackup();
            byte[] data = currentSave.DecryptedData;
            if (options.Contains(0)) foreach (int slot in slots) { WriteCharacterInt(slot, 172, 199); WriteCharacterInt(slot, 176, LevelExperience.ExperienceForLevel(199)); WriteCharacterInt(slot, 180, LevelExperience.AttributePointsForLevel(199)); }
            if (options.Contains(1)) LevelCapFlagValidator.Apply(data, slots);
            if (options.Contains(2)) { currentSave.Zeni = 999999999; currentSave.TPMedals = 999999999; }
            if (options.Contains(3)) foreach (XV2SkillOwnership skill in skillOwnership) { skill.Owned = true; SkillOwnershipWriter.WriteOwned(data, skill); }
            if (options.Contains(4)) { InventoryWriter.SetDragonBalls(data, 125); enforceInfiniteDragonBalls = true; }
            if (options.Contains(5))
            {
                PartnerKeyAccess.GiveAll(data);
                PartnerCustomizationInitializer.Initialize(data, slots);
                PartnerCustomizationInitializer.UnlockAllOptions(data);
            }
            if (options.Contains(6) || options.Contains(7)) foreach (int slot in slots) foreach (XV2QuestProgress quest in QuestProgressReader.Read(data, slot)) { if (options.Contains(6)) { quest.State = 3; quest.Rank = 6; } if (options.Contains(7)) quest.Score = 999999999; QuestProgressWriter.Write(data, quest); }
            if (options.Contains(8)) foreach (int slot in slots) foreach (MentorGauge gauge in ProgressionUnlockAccess.ReadMentors(data, slot)) { gauge.Friendship = 100; gauge.DualUltimate = 100; ProgressionUnlockAccess.WriteMentor(data, slot, gauge); }
            if (options.Contains(9)) foreach (int slot in slots) foreach (TokipediaProgressEntry entry in TokipediaProgressAccess.Read(data, slot, progressReferences.Tokipedia)) { foreach (string path in entry.BranchingPaths.Concat(entry.AlternatePaths)) entry.Flags |= TokipediaFlagMap.Get(path); TokipediaProgressAccess.Write(data, entry); }
            if (options.Contains(10)) foreach (bool artwork in new[] { true, false }) foreach (CollectionUnlock item in ProgressionUnlockAccess.ReadCollection(data, artwork)) ProgressionUnlockAccess.WriteCollection(data, artwork, item, true);
            if (options.Contains(11)) GiveAllClothesForMaxOut(data, 125);
            if (options.Contains(12))
            {
                FestivalPresetAccess.Unlock(data, slots);
                PartnerCustomizationInitializer.UnlockAllOptions(data);
            }
            if (options.Contains(13)) GivePerfectQQBangForMaxOut(data);
            if (options.Contains(14)) GiveAllItemsForMaxOut(data, NamedValueKind.Accessory, InventoryReader.AccessoriesOffset, 4, 125);
            if (options.Contains(15)) GiveAllItemsForMaxOut(data, NamedValueKind.SuperSoul, InventoryReader.SuperSoulsOffset, 5, 125);
            if (options.Contains(16)) GiveAllItemsForMaxOut(data, NamedValueKind.MixItem, InventoryReader.MixItemsOffset, 6, 125);
            if (options.Contains(17)) GiveAllItemsForMaxOut(data, NamedValueKind.Capsule, InventoryReader.CapsulesOffset, 8, 125);
            RefreshLoadedSave(slots[0], preserveDirty: true); MarkUnsaved();
            MessageBox.Show($"Verified max-out package applied. Review the save before writing it.\n\nSafety backup: {System.IO.Path.GetFileName(backup)}", "Max-Out Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void GiveAllClothesForMaxOut(byte[] data, byte quantity)
        {
            NamedValueKind[] kinds = { NamedValueKind.Top, NamedValueKind.Bottom, NamedValueKind.Gloves, NamedValueKind.Shoes };
            int[] offsets = { InventoryReader.TopsOffset, InventoryReader.BottomsOffset, InventoryReader.GlovesOffset, InventoryReader.ShoesOffset };
            for (int type = 0; type < kinds.Length; type++)
            {
                List<int> ids = nameDatabase.GetValues(kinds[type]).Select(value => value.SaveId).Distinct().ToList();
                InventoryWriter.AddMissingClothing(data, offsets[type], (byte)type, ids, quantity);
                XV2Inventory inventory = InventoryReader.Read(data);
                IEnumerable<XV2InventoryItem> items = type switch
                {
                    0 => inventory.Tops,
                    1 => inventory.Bottoms,
                    2 => inventory.Gloves,
                    _ => inventory.Shoes
                };
                InventoryWriter.SetAllQuantities(data, offsets[type], items, quantity);
            }
        }

        private static void GivePerfectQQBangForMaxOut(byte[] data)
        {
            XV2QQBang? perfect = QQBangReader.Read(data).FirstOrDefault(item =>
                item.Health == 5 && item.Ki == 5 && item.Stamina == 5 && item.BasicAttack == 5 &&
                item.StrikeSupers == 5 && item.KiBlastSupers == 5);
            perfect ??= QQBangWriter.AddNeutral(data);
            perfect.Health = perfect.Ki = perfect.Stamina = perfect.BasicAttack =
                perfect.StrikeSupers = perfect.KiBlastSupers = 5;
            perfect.Quantity = 99;
            QQBangWriter.WriteVerifiedFields(data, perfect);
        }

        private void GiveAllItemsForMaxOut(byte[] data, NamedValueKind kind, int sectionOffset, byte itemType, byte quantity)
        {
            List<int> ids = nameDatabase.GetValues(kind).Select(value => value.SaveId).Distinct().ToList();
            InventoryWriter.AddMissingItems(data, sectionOffset, itemType, ids, quantity);
            XV2Inventory inventory = InventoryReader.Read(data);
            IEnumerable<XV2InventoryItem> items = kind switch
            {
                NamedValueKind.Accessory => inventory.Accessories,
                NamedValueKind.SuperSoul => inventory.SuperSouls,
                NamedValueKind.MixItem => inventory.MixItems,
                NamedValueKind.Capsule => inventory.Capsules,
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
            InventoryWriter.SetAllQuantities(data, sectionOffset, items, quantity);
        }
    }
}
