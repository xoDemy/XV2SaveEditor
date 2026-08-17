using System;

namespace XV2SaveEditor
{
    public static class AppearanceReader
    {
        public const int CharacterSectionOffset = 124976;
        public const int CharacterStride = 50888;

        public static XV2Appearance ReadAppearance(
            byte[] data,
            int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 8)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slotIndex)
                );
            }

            int offset =
                CharacterSectionOffset +
                (CharacterStride * slotIndex);

            return new XV2Appearance
            {
                BodyShape =
                    BitConverter.ToInt32(data, offset + 28),

                SkinColor1 =
                    BitConverter.ToUInt16(data, offset + 36),

                SkinColor2 =
                    BitConverter.ToUInt16(data, offset + 38),

                SkinColor3 =
                    BitConverter.ToUInt16(data, offset + 40),

                SkinColor4 =
                    BitConverter.ToUInt16(data, offset + 42),

                HairColor =
                    BitConverter.ToUInt16(data, offset + 44),

                EyeColor =
                    BitConverter.ToUInt16(data, offset + 46),

                MakeupColor1 =
                    BitConverter.ToUInt16(data, offset + 48),

                MakeupColor2 =
                    BitConverter.ToUInt16(data, offset + 50),

                MakeupColor3 =
                    BitConverter.ToUInt16(data, offset + 52),

                FaceBase =
                    BitConverter.ToInt32(data, offset + 132),

                FaceForehead =
                    BitConverter.ToInt32(data, offset + 136),

                Eyes =
                    BitConverter.ToInt32(data, offset + 140),

                Nose =
                    BitConverter.ToInt32(data, offset + 144),

                Ears =
                    BitConverter.ToInt32(data, offset + 148),

                Hair =
                    BitConverter.ToInt32(data, offset + 152)
            };
        }
    }
}