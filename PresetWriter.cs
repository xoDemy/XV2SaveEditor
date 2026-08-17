using System;
using System.Collections.Generic;

namespace XV2SaveEditor
{
    public static class PresetWriter
    {
        public const int VerifiedEmptyQQBang = 0x00AAAAAA;

        public const int PresetSectionOffset =
            125184;

        public const int CharacterStride =
            50888;

        public const int PresetSize =
            96;

        public const int PresetCount =
            8;

        public static void ApplyVerifiedEmptyLoadout(XV2Preset preset)
        {
            ArgumentNullException.ThrowIfNull(preset);
            preset.Top = -1;
            preset.Bottom = -1;
            preset.Gloves = -1;
            preset.Shoes = -1;
            preset.Accessory = -1;
            preset.SuperSoul = -1;
            preset.QQBang = VerifiedEmptyQQBang;
            preset.SuperSkill1 = -1;
            preset.SuperSkill2 = -1;
            preset.SuperSkill3 = -1;
            preset.SuperSkill4 = -1;
            preset.UltimateSkill1 = -1;
            preset.UltimateSkill2 = -1;
            preset.EvasiveSkill = -1;
            preset.BlastSkill = -1;
            preset.AwokenSkill = -1;
            // Real empty presets retain their colour fields, so preserve them.
        }


        // =========================================================
        // WRITE ALL 8 PRESETS FOR ONE CHARACTER
        // =========================================================

        public static void WritePresets(
            byte[] data,
            int characterSlotIndex,
            List<XV2Preset> presets)
        {
            if (
                characterSlotIndex < 0 ||
                characterSlotIndex >= 8)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(characterSlotIndex)
                );
            }


            if (presets == null)
            {
                throw new ArgumentNullException(
                    nameof(presets)
                );
            }


            if (presets.Count != PresetCount)
            {
                throw new InvalidOperationException(
                    $"Expected exactly {PresetCount} presets, " +
                    $"but found {presets.Count}."
                );
            }


            int characterOffset =
                PresetSectionOffset +
                (CharacterStride * characterSlotIndex);


            if (
                characterOffset +
                (PresetSize * PresetCount) >
                data.Length)
            {
                throw new InvalidOperationException(
                    "Preset data would be written outside the save."
                );
            }


            for (
                int presetIndex = 0;
                presetIndex < PresetCount;
                presetIndex++)
            {
                WritePreset(
                    data,
                    characterSlotIndex,
                    presetIndex,
                    presets[presetIndex]
                );
            }
        }


        // =========================================================
        // WRITE ONE PRESET
        // =========================================================

        public static void WritePreset(
            byte[] data,
            int characterSlotIndex,
            int presetIndex,
            XV2Preset preset)
        {
            if (
                characterSlotIndex < 0 ||
                characterSlotIndex >= 8)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(characterSlotIndex)
                );
            }


            if (
                presetIndex < 0 ||
                presetIndex >= PresetCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(presetIndex)
                );
            }


            if (preset == null)
            {
                throw new ArgumentNullException(
                    nameof(preset)
                );
            }


            int offset =
                PresetSectionOffset +
                (CharacterStride * characterSlotIndex) +
                (PresetSize * presetIndex);


            if (
                offset + PresetSize >
                data.Length)
            {
                throw new InvalidOperationException(
                    "Preset would be written outside the save."
                );
            }


            // =====================================================
            // EQUIPMENT
            // =====================================================

            WriteInt32(
                data,
                offset + 0,
                preset.Top
            );


            WriteInt32(
                data,
                offset + 4,
                preset.Bottom
            );


            WriteInt32(
                data,
                offset + 8,
                preset.Gloves
            );


            WriteInt32(
                data,
                offset + 12,
                preset.Shoes
            );


            WriteInt32(
                data,
                offset + 16,
                preset.Accessory
            );


            WriteInt32(
                data,
                offset + 20,
                preset.SuperSoul
            );


            // Raw packed QQ Bang
            WriteInt32(
                data,
                offset + 24,
                preset.QQBang
            );


            // =====================================================
            // TOP COLORS
            // =====================================================

            WriteUInt16(
                data,
                offset + 28,
                preset.TopColor1
            );


            WriteUInt16(
                data,
                offset + 30,
                preset.TopColor2
            );


            WriteUInt16(
                data,
                offset + 32,
                preset.TopColor3
            );


            WriteUInt16(
                data,
                offset + 34,
                preset.TopColor4
            );


            // =====================================================
            // BOTTOM COLORS
            // =====================================================

            WriteUInt16(
                data,
                offset + 36,
                preset.BottomColor1
            );


            WriteUInt16(
                data,
                offset + 38,
                preset.BottomColor2
            );


            WriteUInt16(
                data,
                offset + 40,
                preset.BottomColor3
            );


            WriteUInt16(
                data,
                offset + 42,
                preset.BottomColor4
            );


            // =====================================================
            // GLOVES COLORS
            // =====================================================

            WriteUInt16(
                data,
                offset + 44,
                preset.GlovesColor1
            );


            WriteUInt16(
                data,
                offset + 46,
                preset.GlovesColor2
            );


            WriteUInt16(
                data,
                offset + 48,
                preset.GlovesColor3
            );


            WriteUInt16(
                data,
                offset + 50,
                preset.GlovesColor4
            );


            // =====================================================
            // SHOES COLORS
            // =====================================================

            WriteUInt16(
                data,
                offset + 52,
                preset.ShoesColor1
            );


            WriteUInt16(
                data,
                offset + 54,
                preset.ShoesColor2
            );


            WriteUInt16(
                data,
                offset + 56,
                preset.ShoesColor3
            );


            WriteUInt16(
                data,
                offset + 58,
                preset.ShoesColor4
            );


            // =====================================================
            // SUPER SKILLS
            // =====================================================

            WriteInt32(
                data,
                offset + 60,
                preset.SuperSkill1
            );


            WriteInt32(
                data,
                offset + 64,
                preset.SuperSkill2
            );


            WriteInt32(
                data,
                offset + 68,
                preset.SuperSkill3
            );


            WriteInt32(
                data,
                offset + 72,
                preset.SuperSkill4
            );


            // =====================================================
            // ULTIMATES
            // =====================================================

            WriteInt32(
                data,
                offset + 76,
                preset.UltimateSkill1
            );


            WriteInt32(
                data,
                offset + 80,
                preset.UltimateSkill2
            );


            // =====================================================
            // EVASIVE / BLAST / AWOKEN
            // =====================================================

            WriteInt32(
                data,
                offset + 84,
                preset.EvasiveSkill
            );


            WriteInt32(
                data,
                offset + 88,
                preset.BlastSkill
            );


            WriteInt32(
                data,
                offset + 92,
                preset.AwokenSkill
            );
        }


        // =========================================================
        // HELPERS
        // =========================================================

        private static void WriteInt32(
            byte[] data,
            int offset,
            int value)
        {
            byte[] bytes =
                BitConverter.GetBytes(
                    value
                );


            Array.Copy(
                bytes,
                0,
                data,
                offset,
                4
            );
        }


        private static void WriteUInt16(
            byte[] data,
            int offset,
            ushort value)
        {
            byte[] bytes =
                BitConverter.GetBytes(
                    value
                );


            Array.Copy(
                bytes,
                0,
                data,
                offset,
                2
            );
        }
    }
}
