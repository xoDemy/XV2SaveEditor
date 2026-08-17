using System;
using System.Collections.Generic;

namespace XV2SaveEditor
{
    public static class QQBangReader
    {
        public const int SectionOffset = 0xA410;
        public const int EntrySize = 8;
        public const int EntryCount = 512;
        public const int SectionSize = EntrySize * EntryCount;

        public static List<XV2QQBang> Read(byte[] data)
        {
            ValidateSection(data);
            List<XV2QQBang> result = new List<XV2QQBang>();

            for (int slot = 0; slot < EntryCount; slot++)
            {
                int offset = SectionOffset + (slot * EntrySize);
                byte type = data[offset + 4];
                byte quantity = data[offset + 5];

                // QQ Bang stats are packed into six nibbles in bytes +0..+2.
                // Ownership is identified by the normal inventory type field.
                if (type == 0xFF || type != 9 || quantity == 0 || quantity == 0xFF)
                {
                    continue;
                }

                byte[] original = new byte[EntrySize];
                Buffer.BlockCopy(data, offset, original, 0, EntrySize);
                uint packedStats = BitConverter.ToUInt32(data, offset);

                result.Add(new XV2QQBang
                {
                    SlotIndex = slot,
                    Health = DecodeStat(packedStats, 0),
                    Ki = DecodeStat(packedStats, 1),
                    Stamina = DecodeStat(packedStats, 2),
                    BasicAttack = DecodeStat(packedStats, 3),
                    StrikeSupers = DecodeStat(packedStats, 4),
                    KiBlastSupers = DecodeStat(packedStats, 5),
                    Metadata = data[offset + 6],
                    I_07 = data[offset + 7],
                    Quantity = quantity,
                    OriginalBytes = original
                });
            }

            return result;
        }

        public static void ValidateSection(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (SectionOffset + SectionSize > data.Length)
            {
                throw new InvalidOperationException(
                    $"The QQ Bang section is outside the decrypted save data.\n\n" +
                    $"Offset: 0x{SectionOffset:X}\nRequired size: {SectionSize}\nSave size: {data.Length}");
            }
        }

        private static sbyte DecodeStat(uint packedStats, int index) =>
            (sbyte)(((packedStats >> (index * 4)) & 0xF) - 5);
    }
}
