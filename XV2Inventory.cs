using System.Collections.Generic;

namespace XV2SaveEditor
{
    public class XV2Inventory
    {
        // =========================================================
        // CLOTHING
        // =========================================================

        public List<XV2InventoryItem> Tops
        {
            get;
            set;
        } =
            new List<XV2InventoryItem>();


        public List<XV2InventoryItem> Bottoms
        {
            get;
            set;
        } =
            new List<XV2InventoryItem>();


        public List<XV2InventoryItem> Gloves
        {
            get;
            set;
        } =
            new List<XV2InventoryItem>();


        public List<XV2InventoryItem> Shoes
        {
            get;
            set;
        } =
            new List<XV2InventoryItem>();


        // =========================================================
        // OTHER EQUIPMENT
        // =========================================================

        public List<XV2InventoryItem> Accessories
        {
            get;
            set;
        } =
            new List<XV2InventoryItem>();


        public List<XV2InventoryItem> SuperSouls
        {
            get;
            set;
        } =
            new List<XV2InventoryItem>();


        // =========================================================
        // ITEMS
        // =========================================================

        public List<XV2InventoryItem> MixItems
        {
            get;
            set;
        } =
            new List<XV2InventoryItem>();


        public List<XV2InventoryItem> ImportantItems
        {
            get;
            set;
        } =
            new List<XV2InventoryItem>();


        public List<XV2InventoryItem> Capsules
        {
            get;
            set;
        } =
            new List<XV2InventoryItem>();


        // =========================================================
        // COUNTS
        // =========================================================

        public int TotalNormalItems
        {
            get
            {
                return
                    Tops.Count +
                    Bottoms.Count +
                    Gloves.Count +
                    Shoes.Count +
                    Accessories.Count +
                    SuperSouls.Count +
                    MixItems.Count +
                    ImportantItems.Count +
                    Capsules.Count;
            }
        }
    }
}