namespace XV2SaveEditor
{
    public enum NamedValueKind
    {
        // =========================================================
        // EQUIPMENT
        // =========================================================

        Top,
        Bottom,
        Gloves,
        Shoes,
        Accessory,
        SuperSoul,


        // =========================================================
        // INVENTORY ITEMS
        // =========================================================

        MixItem,
        ImportantItem,
        Capsule,


        // =========================================================
        // SKILLS
        // =========================================================

        SuperSkill,
        UltimateSkill,
        EvasiveSkill,
        AwokenSkill
    }


    public class NamedSaveValue
    {
        // =========================================================
        // SAVE / DATABASE IDS
        // =========================================================

        // Value actually stored in the XV2 save.
        public int SaveId { get; set; }


        // ID used by the IDB file.
        public int DatabaseId { get; set; }


        // =========================================================
        // DISPLAY DATA
        // =========================================================

        public string Name { get; set; } = "";


        public NamedValueKind Kind { get; set; }


        public int? RaceMask { get; set; }


        // =========================================================
        // DISPLAY NAME
        // =========================================================

        public string DisplayName
        {
            get
            {
                return Name;
            }
        }


        public override string ToString()
        {
            return DisplayName;
        }
    }
}
