using System;
using System.Collections.Generic;
using System.Text;

namespace XV2SaveEditor
{
    public static class CharacterReader
    {
        public const int CharacterSectionOffset =
            124976;

        public const int CharacterStride =
            50888;

        public const int CharacterCount =
            8;


        public static List<XV2Character> ReadCharacters(
            byte[] data)
        {
            List<XV2Character> characters =
                new List<XV2Character>();


            for (
                int slot = 0;
                slot < CharacterCount;
                slot++)
            {
                int offset =
                    CharacterSectionOffset +
                    (CharacterStride * slot);


                if (
                    offset + 208 >
                    data.Length)
                {
                    throw new InvalidOperationException(
                        $"Character slot {slot + 1} is outside the save data."
                    );
                }


                XV2Character character =
                    new XV2Character
                    {
                        Slot =
                            slot + 1,


                        // =========================================
                        // BASIC DATA
                        // =========================================

                        Race =
                            BitConverter.ToInt32(
                                data,
                                offset + 20
                            ),


                        Voice =
                            BitConverter.ToInt32(
                                data,
                                offset + 24
                            ),


                        Name =
                            ReadName(
                                data,
                                offset + 68,
                                64
                            ),


                        // =========================================
                        // LEVEL / XP
                        // =========================================

                        Level =
                            BitConverter.ToInt32(
                                data,
                                offset + 172
                            ),


                        Experience =
                            BitConverter.ToInt32(
                                data,
                                offset + 176
                            ),


                        AttributePoints =
                            BitConverter.ToInt32(
                                data,
                                offset + 180
                            ),


                        // =========================================
                        // STATS
                        // =========================================

                        Health =
                            BitConverter.ToInt32(
                                data,
                                offset + 184
                            ),


                        Ki =
                            BitConverter.ToInt32(
                                data,
                                offset + 188
                            ),


                        BasicAttack =
                            BitConverter.ToInt32(
                                data,
                                offset + 192
                            ),


                        StrikeSupers =
                            BitConverter.ToInt32(
                                data,
                                offset + 196
                            ),


                        KiBlastSupers =
                            BitConverter.ToInt32(
                                data,
                                offset + 200
                            ),


                        Stamina =
                            BitConverter.ToInt32(
                                data,
                                offset + 204
                            )
                    };


                // =============================================
                // APPEARANCE
                // =============================================

                character.Appearance =
                    AppearanceReader.ReadAppearance(
                        data,
                        slot
                    );


                // =============================================
                // PRESETS
                // =============================================

                character.Presets =
                    PresetReader.ReadPresets(
                        data,
                        slot
                    );


                characters.Add(
                    character
                );
            }


            return characters;
        }


        // =========================================================
        // CHARACTER NAME
        // =========================================================

        private static string ReadName(
            byte[] data,
            int offset,
            int maxLength)
        {
            int length =
                0;


            while (
                length < maxLength &&
                data[offset + length] != 0)
            {
                length++;
            }


            return Encoding.UTF8
                .GetString(
                    data,
                    offset,
                    length
                )
                .Trim();
        }
    }
}