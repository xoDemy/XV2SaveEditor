using System;

namespace XV2SaveEditor
{
    public static class QQBangWriter
    {
        public static XV2QQBang AddNeutral(byte[] data)
        {
            QQBangReader.ValidateSection(data);
            for (int slot = 0; slot < QQBangReader.EntryCount; slot++)
            {
                int offset = QQBangReader.SectionOffset + slot * QQBangReader.EntrySize;
                if (data[offset + 4] != 0xFF) continue;

                // Six neutral stats are stored as six nibbles with value 5.
                data[offset] = 0x55;
                data[offset + 1] = 0x55;
                data[offset + 2] = 0x55;
                data[offset + 3] = 0;
                data[offset + 4] = 9;
                data[offset + 5] = 1;
                data[offset + 6] = 0;
                data[offset + 7] = 0;

                foreach (XV2QQBang item in QQBangReader.Read(data))
                    if (item.SlotIndex == slot) return item;
            }

            throw new InvalidOperationException("There are no empty QQ Bang slots.");
        }

        public static void WriteVerifiedFields(byte[] data, XV2QQBang item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            ValidateSlot(data, item.SlotIndex);
            int offset = QQBangReader.SectionOffset + item.SlotIndex * QQBangReader.EntrySize;

            // Deliberately write only the six verified signed stats and quantity.
            // Byte +6 remains untouched because its full meaning is not proven.
            uint packedStats = (uint)(data[offset + 3] << 24);
            sbyte[] stats = { item.Health, item.Ki, item.Stamina, item.BasicAttack, item.StrikeSupers, item.KiBlastSupers };
            for (int i = 0; i < stats.Length; i++)
            {
                if (stats[i] < -5 || stats[i] > 5)
                    throw new ArgumentOutOfRangeException(nameof(item), "QQ Bang stats must be between -5 and +5.");
                packedStats |= (uint)(stats[i] + 5) << (i * 4);
            }
            byte[] packedBytes = BitConverter.GetBytes(packedStats);
            Buffer.BlockCopy(packedBytes, 0, data, offset, 3);
            data[offset + 5] = item.Quantity;
        }

        public static void RestoreOriginal(byte[] data, XV2QQBang item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.OriginalBytes.Length != QQBangReader.EntrySize)
                throw new InvalidOperationException("The original QQ Bang record is unavailable.");
            ValidateSlot(data, item.SlotIndex);
            int offset = QQBangReader.SectionOffset + item.SlotIndex * QQBangReader.EntrySize;
            Buffer.BlockCopy(item.OriginalBytes, 0, data, offset, QQBangReader.EntrySize);
        }

        private static void ValidateSlot(byte[] data, int slot)
        {
            QQBangReader.ValidateSection(data);
            if (slot < 0 || slot >= QQBangReader.EntryCount)
                throw new ArgumentOutOfRangeException(nameof(slot));
        }
    }
}
