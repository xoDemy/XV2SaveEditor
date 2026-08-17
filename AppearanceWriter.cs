using System;

namespace XV2SaveEditor
{
    public static class AppearanceWriter
    {
        public const int CharacterSectionOffset = 124976;
        public const int CharacterStride = 50888;

        public static void WriteAppearance(
            byte[] data,
            int slotIndex,
            int bodyShape,
            ushort skinColor1,
            ushort skinColor2,
            ushort skinColor3,
            ushort skinColor4,
            ushort hairColor,
            ushort eyeColor,
            ushort makeupColor1,
            ushort makeupColor2,
            ushort makeupColor3,
            int faceBase,
            int forehead,
            int eyes,
            int nose,
            int ears,
            int hair)
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

            WriteInt32(
                data,
                offset + 28,
                bodyShape
            );

            WriteUInt16(
                data,
                offset + 36,
                skinColor1
            );

            WriteUInt16(
                data,
                offset + 38,
                skinColor2
            );

            WriteUInt16(
                data,
                offset + 40,
                skinColor3
            );

            WriteUInt16(
                data,
                offset + 42,
                skinColor4
            );

            WriteUInt16(
                data,
                offset + 44,
                hairColor
            );

            WriteUInt16(
                data,
                offset + 46,
                eyeColor
            );

            WriteUInt16(
                data,
                offset + 48,
                makeupColor1
            );

            WriteUInt16(
                data,
                offset + 50,
                makeupColor2
            );

            WriteUInt16(
                data,
                offset + 52,
                makeupColor3
            );

            WriteInt32(
                data,
                offset + 132,
                faceBase
            );

            WriteInt32(
                data,
                offset + 136,
                forehead
            );

            WriteInt32(
                data,
                offset + 140,
                eyes
            );

            WriteInt32(
                data,
                offset + 144,
                nose
            );

            WriteInt32(
                data,
                offset + 148,
                ears
            );

            WriteInt32(
                data,
                offset + 152,
                hair
            );
        }

        private static void WriteInt32(
            byte[] data,
            int offset,
            int value)
        {
            byte[] bytes =
                BitConverter.GetBytes(value);

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
                BitConverter.GetBytes(value);

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