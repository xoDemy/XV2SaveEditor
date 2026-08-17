namespace XV2SaveEditor
{
    public class InventoryDisplayItem
    {
        // =========================================================
        // RAW INVENTORY ITEM
        // =========================================================

        public XV2InventoryItem Item
        {
            get;
            set;
        } =
            new XV2InventoryItem();


        // =========================================================
        // DISPLAY NAME
        // =========================================================

        public string Name
        {
            get;
            set;
        } =
            "";


        // =========================================================
        // CONVENIENCE PROPERTIES
        // =========================================================

        public int ID
        {
            get
            {
                return Item.ID;
            }
        }


        public byte Quantity
        {
            get
            {
                return Item.Quantity;
            }
        }


        public int SlotIndex
        {
            get
            {
                return Item.SlotIndex;
            }
        }


        public byte Type
        {
            get
            {
                return Item.Type;
            }
        }


        // =========================================================
        // DISPLAY TEXT
        // =========================================================

        public string DisplayName
        {
            get
            {
                return
                    $"{Name} — Qty {Item.Quantity}";
            }
        }


        public override string ToString()
        {
            return DisplayName;
        }
    }
}
