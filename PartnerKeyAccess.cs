using System;
using System.Linq;

namespace XV2SaveEditor
{
    public static class PartnerKeyAccess
    {
        public static readonly int[] KeyItemIds = { 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 31, 32, 33, 34, 35 };
        private const int FirstFlagsOffset = 506772;
        private const int SecondFlagsOffset = 724484;

        public static int GiveAll(byte[] data)
        {
            int added = InventoryWriter.AddMissingItems(data, InventoryReader.ImportantItemsOffset, 7, KeyItemIds, 1);
            foreach (XV2InventoryItem item in InventoryReader.Read(data).ImportantItems.Where(x => KeyItemIds.Contains(x.ID)))
                InventoryWriter.WriteQuantity(data, InventoryReader.ImportantItemsOffset, item, 1);

            uint first = BitConverter.ToUInt32(data, FirstFlagsOffset) | 0x3FFu;
            Array.Copy(BitConverter.GetBytes(first), 0, data, FirstFlagsOffset, 4);
            // LazyBones' verified Give Partner Keys operation sets this entire
            // second 32-bit partner-key field for keys 11-20.
            Array.Copy(BitConverter.GetBytes(uint.MaxValue), 0, data, SecondFlagsOffset, 4);
            return added;
        }
    }
}
