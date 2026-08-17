using System;

namespace XV2SaveEditor
{
    public static class SystemFlagAccess
    {
        public const int BaseOffset = 95280;
        public const int CharacterStride = 50888;
        public const int ByteCount = 1024;
        public static bool Read(byte[] data, int characterSlot, int index)
        {
            Validate(data, characterSlot, index);
            int offset = BaseOffset + CharacterStride * characterSlot + index / 8;
            return (data[offset] & (1 << index % 8)) != 0;
        }
        public static void Write(byte[] data, int characterSlot, int index, bool value)
        {
            Validate(data, characterSlot, index);
            int offset = BaseOffset + CharacterStride * characterSlot + index / 8;
            byte mask = (byte)(1 << index % 8);
            data[offset] = value ? (byte)(data[offset] | mask) : (byte)(data[offset] & ~mask);
        }
        private static void Validate(byte[] data, int slot, int index)
        {
            if (slot < 0 || slot >= 8) throw new ArgumentOutOfRangeException(nameof(slot));
            if (index < 0 || index >= ByteCount * 8) throw new ArgumentOutOfRangeException(nameof(index));
            if (BaseOffset + CharacterStride * slot + ByteCount > data.Length) throw new ArgumentException("System flag section is outside this save.");
        }
    }
}
