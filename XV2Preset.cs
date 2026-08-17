namespace XV2SaveEditor
{
    public class XV2Preset
    {
        public int Index { get; set; }

        // =========================================================
        // EQUIPMENT
        // =========================================================

        public int Top { get; set; }

        public int Bottom { get; set; }

        public int Gloves { get; set; }

        public int Shoes { get; set; }

        public int Accessory { get; set; }

        public int SuperSoul { get; set; }

        // Raw packed QQ Bang value for now.
        // We will decode the actual stat structure later.
        public int QQBang { get; set; }


        // =========================================================
        // TOP COLORS
        // =========================================================

        public ushort TopColor1 { get; set; }

        public ushort TopColor2 { get; set; }

        public ushort TopColor3 { get; set; }

        public ushort TopColor4 { get; set; }


        // =========================================================
        // BOTTOM COLORS
        // =========================================================

        public ushort BottomColor1 { get; set; }

        public ushort BottomColor2 { get; set; }

        public ushort BottomColor3 { get; set; }

        public ushort BottomColor4 { get; set; }


        // =========================================================
        // GLOVES COLORS
        // =========================================================

        public ushort GlovesColor1 { get; set; }

        public ushort GlovesColor2 { get; set; }

        public ushort GlovesColor3 { get; set; }

        public ushort GlovesColor4 { get; set; }


        // =========================================================
        // SHOES COLORS
        // =========================================================

        public ushort ShoesColor1 { get; set; }

        public ushort ShoesColor2 { get; set; }

        public ushort ShoesColor3 { get; set; }

        public ushort ShoesColor4 { get; set; }


        // =========================================================
        // SKILLS
        // =========================================================

        public int SuperSkill1 { get; set; }

        public int SuperSkill2 { get; set; }

        public int SuperSkill3 { get; set; }

        public int SuperSkill4 { get; set; }

        public int UltimateSkill1 { get; set; }

        public int UltimateSkill2 { get; set; }

        public int EvasiveSkill { get; set; }

        public int BlastSkill { get; set; }

        public int AwokenSkill { get; set; }


        // =========================================================
        // DISPLAY
        // =========================================================

        public string DisplayName
        {
            get
            {
                if (Index == 0)
                {
                    return "Main";
                }

                return $"Preset {Index}";
            }
        }


        public override string ToString()
        {
            return DisplayName;
        }
    }
}