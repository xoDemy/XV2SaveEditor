namespace XV2SaveEditor
{
    public class XV2InventoryItem
    {
        // =========================================================
        // LOCATION
        // =========================================================

        // Physical slot inside the 512-entry inventory block.
        public int SlotIndex { get; set; }


        // =========================================================
        // RAW SAVE DATA
        // =========================================================

        public int ID { get; set; }

        public byte Type { get; set; }

        public byte Quantity { get; set; }

        public byte I_06 { get; set; }

        public byte I_07 { get; set; }


        // =========================================================
        // HELPERS
        // =========================================================

        public bool IsEmpty
        {
            get
            {
                return
                    ID == -1 ||
                    Type == 255;
            }
        }


        public override string ToString()
        {
            return
                $"ID {ID} - Qty {Quantity}";
        }
    }
}