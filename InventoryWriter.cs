using System;

namespace XV2SaveEditor
{
    public static class InventoryWriter
    {
        // =========================================================
        // WRITE ONE ITEM QUANTITY
        // =========================================================

        public static void WriteQuantity(
            byte[] data,
            int sectionOffset,
            int slotIndex,
            byte quantity)
        {
            if (data == null)
            {
                throw new ArgumentNullException(
                    nameof(data)
                );
            }


            if (
                slotIndex < 0 ||
                slotIndex >=
                InventoryReader.EntriesPerSection)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slotIndex),
                    $"Inventory slot must be between 0 and " +
                    $"{InventoryReader.EntriesPerSection - 1}."
                );
            }


            int entryOffset =
                sectionOffset +
                (
                    slotIndex *
                    InventoryReader.EntrySize
                );


            ValidateEntryOffset(
                data,
                entryOffset
            );


            // =====================================================
            // IMPORTANT:
            //
            // Inventory entry:
            //
            // +0  Int32 ID
            // +4  Byte Type
            // +5  Byte Quantity
            // +6  Byte I_06
            // +7  Byte I_07
            //
            // We ONLY change +5.
            // =====================================================

            data[
                entryOffset + 5
            ] =
                quantity;
        }


        // =========================================================
        // WRITE QUANTITY USING INVENTORY ITEM
        // =========================================================

        public static void WriteQuantity(
            byte[] data,
            int sectionOffset,
            XV2InventoryItem item,
            byte quantity)
        {
            if (item == null)
            {
                throw new ArgumentNullException(
                    nameof(item)
                );
            }


            WriteQuantity(
                data,
                sectionOffset,
                item.SlotIndex,
                quantity
            );


            // Keep the in-memory object synchronized too.
            item.Quantity =
                quantity;
        }


        // =========================================================
        // MAX ALL EXISTING ITEMS IN CATEGORY
        // =========================================================

        public static void SetAllQuantities(
            byte[] data,
            int sectionOffset,
            System.Collections.Generic.IEnumerable<XV2InventoryItem> items,
            byte quantity)
        {
            if (items == null)
            {
                throw new ArgumentNullException(
                    nameof(items)
                );
            }


            foreach (
                XV2InventoryItem item
                in items)
            {
                WriteQuantity(
                    data,
                    sectionOffset,
                    item,
                    quantity
                );
            }
        }

        // Verified clothing insertion format used by the authoritative editor:
        // ID + EquipmentType + quantity; I_06 and I_07 remain zero.
        public static int AddMissingClothing(
            byte[] data,
            int sectionOffset,
            byte clothingType,
            System.Collections.Generic.IEnumerable<int> catalogueIds,
            byte quantity = 1)
        {
            if (clothingType > 3) throw new ArgumentOutOfRangeException(nameof(clothingType));
            System.Collections.Generic.HashSet<int> existing = new System.Collections.Generic.HashSet<int>();
            System.Collections.Generic.Queue<int> emptySlots = new System.Collections.Generic.Queue<int>();
            for (int slot = 0; slot < InventoryReader.EntriesPerSection; slot++)
            {
                int offset = sectionOffset + slot * InventoryReader.EntrySize;
                ValidateEntryOffset(data, offset);
                int id = BitConverter.ToInt32(data, offset);
                byte type = data[offset + 4];
                if (id == -1 || type == byte.MaxValue) emptySlots.Enqueue(slot); else existing.Add(id);
            }

            int added = 0;
            foreach (int id in catalogueIds)
            {
                if (id < 0 || existing.Contains(id)) continue;
                if (emptySlots.Count == 0) break;
                int offset = sectionOffset + emptySlots.Dequeue() * InventoryReader.EntrySize;
                Array.Copy(BitConverter.GetBytes(id), 0, data, offset, 4);
                data[offset + 4] = clothingType;
                data[offset + 5] = quantity;
                data[offset + 6] = 0;
                data[offset + 7] = 0;
                existing.Add(id);
                added++;
            }
            return added;
        }

        public static int AddMissingItems(
            byte[] data, int sectionOffset, byte itemType,
            System.Collections.Generic.IEnumerable<int> catalogueIds, byte quantity)
        {
            if (itemType > 8) throw new ArgumentOutOfRangeException(nameof(itemType));
            System.Collections.Generic.HashSet<int> existing = new System.Collections.Generic.HashSet<int>();
            System.Collections.Generic.Queue<int> emptySlots = new System.Collections.Generic.Queue<int>();
            for (int slot = 0; slot < InventoryReader.EntriesPerSection; slot++)
            {
                int offset = sectionOffset + slot * InventoryReader.EntrySize;
                ValidateEntryOffset(data, offset);
                int id = BitConverter.ToInt32(data, offset);
                byte type = data[offset + 4];
                if (id == -1 || type == byte.MaxValue) emptySlots.Enqueue(slot); else existing.Add(id);
            }
            int added = 0;
            foreach (int id in catalogueIds)
            {
                if (id < 0 || existing.Contains(id)) continue;
                if (emptySlots.Count == 0) break;
                int offset = sectionOffset + emptySlots.Dequeue() * InventoryReader.EntrySize;
                Array.Copy(BitConverter.GetBytes(id), 0, data, offset, 4);
                data[offset + 4] = itemType;
                data[offset + 5] = quantity;
                data[offset + 6] = 0;
                data[offset + 7] = 0;
                existing.Add(id);
                added++;
            }
            return added;
        }

        // Dragon Balls use a verified special Important Item marker. Their
        // second Int32 is 0x0000QQ06 (marker 0x06, quantity in the next byte),
        // rather than the ordinary Important Item type value 0x07.
        public static int SetDragonBalls(byte[] data, byte quantity = 125)
        {
            int sectionOffset = InventoryReader.ImportantItemsOffset;
            int existingCount = 0;

            for (int slot = 0; slot < InventoryReader.EntriesPerSection; slot++)
            {
                int offset = sectionOffset + slot * InventoryReader.EntrySize;
                ValidateEntryOffset(data, offset);
                int id = BitConverter.ToInt32(data, offset);
                if (id >= 1 && id <= 7) existingCount++;

                // The verified web editor reserves slots 0-6 for the seven
                // balls and removes duplicate ball records from later slots.
                if (slot >= 7 && id >= 1 && id <= 7)
                {
                    Array.Copy(BitConverter.GetBytes(-1), 0, data, offset, 4);
                    Array.Clear(data, offset + 4, 4);
                }
            }

            for (int ball = 1; ball <= 7; ball++)
            {
                int offset = sectionOffset + (ball - 1) * InventoryReader.EntrySize;
                Array.Copy(BitConverter.GetBytes(ball), 0, data, offset, 4);
                data[offset + 4] = 0x06;
                data[offset + 5] = quantity;
                data[offset + 6] = 0;
                data[offset + 7] = 0;
            }

            return Math.Max(0, 7 - Math.Min(7, existingCount));
        }


        // =========================================================
        // VALIDATE ENTRY
        // =========================================================

        private static void ValidateEntryOffset(
            byte[] data,
            int entryOffset)
        {
            if (
                entryOffset < 0 ||
                entryOffset +
                InventoryReader.EntrySize >
                data.Length)
            {
                throw new InvalidOperationException(
                    "The selected inventory entry is outside " +
                    "the decrypted save data.\n\n" +

                    $"Entry Offset: {entryOffset}\n" +
                    $"Entry Size: {InventoryReader.EntrySize}\n" +
                    $"Save Size: {data.Length}"
                );
            }
        }
    }
}
