using System;
using System.Collections.Generic;

namespace XV2SaveEditor
{
    public static class PresetReader
    {
        public const int PresetSectionOffset =
            125184;

        public const int CharacterStride =
            50888;

        public const int PresetSize =
            96;

        public const int PresetCount =
            8;


        public static List<XV2Preset> ReadPresets(
            byte[] data,
            int characterSlotIndex)
        {
            if (
                characterSlotIndex < 0 ||
                characterSlotIndex >= 8)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(characterSlotIndex)
                );
            }


            List<XV2Preset> presets =
                new List<XV2Preset>();


            int characterPresetOffset =
                PresetSectionOffset +
                (CharacterStride * characterSlotIndex);


            for (
                int presetIndex = 0;
                presetIndex < PresetCount;
                presetIndex++)
            {
                int offset =
                    characterPresetOffset +
                    (PresetSize * presetIndex);


                if (
                    offset + PresetSize >
                    data.Length)
                {
                    throw new InvalidOperationException(
                        $"Preset {presetIndex} for character slot " +
                        $"{characterSlotIndex + 1} is outside the save data."
                    );
                }


                XV2Preset preset =
                    new XV2Preset
                    {
                        Index =
                            presetIndex,


                        // =========================================
                        // EQUIPMENT
                        // =========================================

                        Top =
                            BitConverter.ToInt32(
                                data,
                                offset + 0
                            ),

                        Bottom =
                            BitConverter.ToInt32(
                                data,
                                offset + 4
                            ),

                        Gloves =
                            BitConverter.ToInt32(
                                data,
                                offset + 8
                            ),

                        Shoes =
                            BitConverter.ToInt32(
                                data,
                                offset + 12
                            ),

                        Accessory =
                            BitConverter.ToInt32(
                                data,
                                offset + 16
                            ),

                        SuperSoul =
                            BitConverter.ToInt32(
                                data,
                                offset + 20
                            ),

                        QQBang =
                            BitConverter.ToInt32(
                                data,
                                offset + 24
                            ),


                        // =========================================
                        // TOP COLORS
                        // =========================================

                        TopColor1 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 28
                            ),

                        TopColor2 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 30
                            ),

                        TopColor3 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 32
                            ),

                        TopColor4 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 34
                            ),


                        // =========================================
                        // BOTTOM COLORS
                        // =========================================

                        BottomColor1 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 36
                            ),

                        BottomColor2 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 38
                            ),

                        BottomColor3 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 40
                            ),

                        BottomColor4 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 42
                            ),


                        // =========================================
                        // GLOVES COLORS
                        // =========================================

                        GlovesColor1 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 44
                            ),

                        GlovesColor2 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 46
                            ),

                        GlovesColor3 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 48
                            ),

                        GlovesColor4 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 50
                            ),


                        // =========================================
                        // SHOES COLORS
                        // =========================================

                        ShoesColor1 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 52
                            ),

                        ShoesColor2 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 54
                            ),

                        ShoesColor3 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 56
                            ),

                        ShoesColor4 =
                            BitConverter.ToUInt16(
                                data,
                                offset + 58
                            ),


                        // =========================================
                        // SKILLS
                        // =========================================

                        SuperSkill1 =
                            BitConverter.ToInt32(
                                data,
                                offset + 60
                            ),

                        SuperSkill2 =
                            BitConverter.ToInt32(
                                data,
                                offset + 64
                            ),

                        SuperSkill3 =
                            BitConverter.ToInt32(
                                data,
                                offset + 68
                            ),

                        SuperSkill4 =
                            BitConverter.ToInt32(
                                data,
                                offset + 72
                            ),

                        UltimateSkill1 =
                            BitConverter.ToInt32(
                                data,
                                offset + 76
                            ),

                        UltimateSkill2 =
                            BitConverter.ToInt32(
                                data,
                                offset + 80
                            ),

                        EvasiveSkill =
                            BitConverter.ToInt32(
                                data,
                                offset + 84
                            ),

                        BlastSkill =
                            BitConverter.ToInt32(
                                data,
                                offset + 88
                            ),

                        AwokenSkill =
                            BitConverter.ToInt32(
                                data,
                                offset + 92
                            )
                    };


                presets.Add(
                    preset
                );
            }


            return presets;
        }
    }
}