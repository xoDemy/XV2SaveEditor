using System.Collections.Generic;

namespace XV2SaveEditor
{
    public class XV2Character
    {
        public int Slot { get; set; }

        public int Race { get; set; }

        public int Voice { get; set; }

        public string Name { get; set; } = "";

        public int Level { get; set; }

        public int Experience { get; set; }

        public int AttributePoints { get; set; }

        public int Health { get; set; }

        public int Ki { get; set; }

        public int Stamina { get; set; }

        public int BasicAttack { get; set; }

        public int StrikeSupers { get; set; }

        public int KiBlastSupers { get; set; }


        // =========================================================
        // APPEARANCE
        // =========================================================

        public XV2Appearance? Appearance { get; set; }


        // =========================================================
        // PRESETS
        // =========================================================

        public List<XV2Preset> Presets { get; set; }
            = new List<XV2Preset>();


        // =========================================================
        // EMPTY SLOT CHECK
        // =========================================================

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrWhiteSpace(
                    Name
                );
            }
        }


        // =========================================================
        // RACE DISPLAY NAME
        // =========================================================

        public string RaceName
        {
            get
            {
                return Race switch
                {
                    0 => "Human Male",
                    1 => "Human Female",
                    2 => "Saiyan Male",
                    3 => "Saiyan Female",
                    4 => "Namekian",
                    5 => "Frieza Race",
                    6 => "Majin Male",
                    7 => "Majin Female",

                    _ => $"Unknown ({Race})"
                };
            }
        }


        // =========================================================
        // CHARACTER DROPDOWN DISPLAY
        // =========================================================

        public override string ToString()
        {
            if (IsEmpty)
            {
                return $"Slot {Slot} - Not Used";
            }

            return $"Slot {Slot} - {Name}";
        }
    }
}
