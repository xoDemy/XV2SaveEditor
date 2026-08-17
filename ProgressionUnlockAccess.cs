using System;
using System.Collections.Generic;

namespace XV2SaveEditor
{
    public sealed class MentorGauge
    {
        public int Index { get; init; }
        public string Name { get; init; } = "";
        public ushort Friendship { get; set; }
        public ushort DualUltimate { get; set; }
        public override string ToString() => $"{Name} — Friendship {Friendship}/100, Dual {DualUltimate}/100";
    }

    public sealed class CollectionUnlock
    {
        public int ID { get; init; }
        public string Name { get; init; } = "";
        public bool Owned { get; set; }
        public override string ToString() => Name;
    }

    public sealed class PartnerStatEntry
    {
        public int Index { get; init; }
        public string Name { get; init; } = "";
        public int Offset { get; init; }
        public ushort StatType { get; set; }
        public bool Materialized { get; init; }
        public override string ToString() => Materialized ? Name : $"{Name} — Not initialized in-game";
    }

    public sealed record PartnerStatPreset(ushort ID, string Name)
    {
        public override string ToString() => Name;
    }

    public static class ProgressionUnlockAccess
    {
        public const int MentorOffset = 125956;
        public const int MentorStride = 50888;
        public const int MentorEntrySize = 8;
        public const int MentorCount = 33;
        public const int MascotOffset = 204;
        public const int MascotCount = 56; // Verified current catalogue; storage reserves 64 bits.
        public const int ArtworkOffset = 506228;
        public const int ArtworkCount = 1024; // High DLC IDs are outside the verified editable range.
        private const int PartnerRegularOffset = 520096, PartnerRegularCount = 47, PartnerRegularSize = 92;
        private const int PartnerDlcOffset = 524432, PartnerDlcCount = 10, PartnerDlcSize = 44;
        private const int PartnerStride = 25392;
        private const int PartnerFestivalOffset = 757220, PartnerFestivalCount = 54, PartnerFestivalSize = 44, PartnerFestivalStride = 19588;

        private static readonly Dictionary<int, string> PartnerNames = new()
        {
            [0]="Krillin",[1]="Tien",[2]="Yamcha",[3]="Piccolo",[4]="Raditz",[5]="Gohan (Kid)",[6]="Nappa",[7]="Vegeta",
            [8]="Zarbon",[9]="Dodoria",[10]="Captain Ginyu",[11]="Frieza (1st Form)",[12]="Android 18",[13]="Cell (Perfect)",
            [14]="Lord Slug",[15]="Majin Buu",[16]="Hercule",[17]="Gohan (Adult)",[18]="Gotenks",[19]="Turles",[20]="Broly",
            [21]="Beerus",[22]="Pan",[23]="Jaco",[24]="Goku",[25]="Whis",[26]="Cooler",[27]="Android 16",[28]="Gohan (Future)",
            [29]="Bardock",[30]="Hit",[31]="Bojack",[32]="Zamasu",[45]="Videl",[46]="Fu",[47]="Goku (Super Saiyan 4)",
            [48]="Vegeta (Super Saiyan 4)",[49]="Trunks",[50]="SSGSS Vegito",[51]="SSGSS Gogeta",[52]="Tapion",
            [53]="Rosé Goku Black",[54]="Android 17 (DB Super)",[55]="Janemba",[56]="Broly (Full Power Super Saiyan)",
            [57]="Goku (GT)",[58]="Omega Shenron",[59]="Majin Buu (Gohan Absorbed)",[60]="Jiren",[61]="Kefla (Super Saiyan)",
            [64]="Goku (Super Saiyan God)",[65]="Gogeta (Super Saiyan 4)",[66]="Vegeta (Super Saiyan God)",
            [67]="Super Baby 2",[68]="Goku Black"
        };

        public static readonly IReadOnlyList<PartnerStatPreset> PartnerStatPresets = new List<PartnerStatPreset>
        {
            new(0,"Default (None)"), new(24,"Balanced A"), new(49,"Balanced B"),
            new(1,"Combo Type A"),new(25,"Combo Type A+"),new(2,"Combo Type B"),new(26,"Combo Type B+"),new(27,"Combo Type C+"),
            new(15,"Endurance A"),new(40,"Endurance A+"),new(16,"Endurance B"),new(41,"Endurance B+"),new(17,"Endurance C"),new(42,"Endurance C+"),
            new(6,"Health A"),new(31,"Health A+"),new(7,"Health B"),new(32,"Health B+"),new(8,"Health C"),new(33,"Health C+"),
            new(21,"Ki A"),new(46,"Ki A+"),new(22,"Ki B"),new(47,"Ki B+"),new(23,"Ki C"),new(48,"Ki C+"),
            new(3,"Ki Blast Super A"),new(28,"Ki Blast Super A+"),new(4,"Ki Blast Super B"),new(29,"Ki Blast Super B+"),new(5,"Ki Blast Super C"),new(30,"Ki Blast Super C+"),
            new(12,"Speed A"),new(37,"Speed A+"),new(13,"Speed B"),new(38,"Speed B+"),new(14,"Speed C"),new(39,"Speed C+"),
            new(18,"Strike Super A"),new(43,"Strike Super A+"),new(19,"Strike Super B"),new(44,"Strike Super B+"),new(20,"Strike Super C"),new(45,"Strike Super C+"),
            new(9,"Technical A"),new(34,"Technical A+"),new(10,"Technical B"),new(35,"Technical B+"),new(11,"Technical C"),new(36,"Technical C+"),
            new(800,"Broken Stats (Modded)")
        };

        private static readonly string[] MentorNames =
        {
            "Krillin", "Tien", "Yamcha", "Piccolo", "Raditz", "Kid Gohan", "Nappa", "Vegeta",
            "Zarbon", "Dodoria", "Captain Ginyu", "Frieza", "Android 18", "Cell", "Lord Slug",
            "Majin Buu", "Hercule", "Gohan & Videl", "Gotenks", "Turles", "Broly", "Beerus",
            "Pan", "Jaco", "Goku", "Whis", "Cooler", "Android 16", "Future Gohan", "Bardock",
            "Hit", "Bojack", "Zamasu"
        };

        public static List<MentorGauge> ReadMentors(byte[] data, int characterSlot)
        {
            List<MentorGauge> result = new();
            int start = MentorOffset + characterSlot * MentorStride;
            Validate(data, start, MentorCount * MentorEntrySize);
            for (int i = 0; i < MentorCount; i++)
            {
                int offset = start + i * MentorEntrySize;
                result.Add(new MentorGauge { Index = i, Name = MentorNames[i],
                    Friendship = BitConverter.ToUInt16(data, offset), DualUltimate = BitConverter.ToUInt16(data, offset + 2) });
            }
            return result;
        }

        public static void WriteMentor(byte[] data, int characterSlot, MentorGauge gauge)
        {
            int offset = MentorOffset + characterSlot * MentorStride + gauge.Index * MentorEntrySize;
            Validate(data, offset, MentorEntrySize);
            Array.Copy(BitConverter.GetBytes((ushort)Math.Min(100, (int)gauge.Friendship)), 0, data, offset, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Math.Min(100, (int)gauge.DualUltimate)), 0, data, offset + 2, 2);
            // Preserve the verified flags Int32 at +4.
        }

        public static List<CollectionUnlock> ReadCollection(byte[] data, bool artwork)
        {
            int offset = artwork ? ArtworkOffset : MascotOffset;
            int count = artwork ? ArtworkCount : MascotCount;
            Validate(data, offset, (count + 7) / 8);
            List<CollectionUnlock> result = new();
            for (int id = 0; id < count; id++)
                result.Add(new CollectionUnlock { ID = id,
                    Name = artwork ? $"Artwork {id + 1}" : $"Mascot {id + 1}",
                    Owned = (data[offset + id / 8] & 1 << id % 8) != 0 });
            return result;
        }

        public static void WriteCollection(byte[] data, bool artwork, CollectionUnlock item, bool owned)
        {
            int offset = (artwork ? ArtworkOffset : MascotOffset) + item.ID / 8;
            Validate(data, offset, 1);
            byte mask = (byte)(1 << item.ID % 8);
            data[offset] = owned ? (byte)(data[offset] | mask) : (byte)(data[offset] & ~mask);
            item.Owned = owned;
        }

        public static List<PartnerStatEntry> ReadPartnerStats(byte[] data, int characterSlot)
        {
            List<PartnerStatEntry> result = new();
            ReadPartnerBlock(data, result, characterSlot, PartnerRegularOffset, PartnerStride, 0, PartnerRegularCount, PartnerRegularSize);
            ReadPartnerBlock(data, result, characterSlot, PartnerDlcOffset, PartnerStride, 47, PartnerDlcCount, PartnerDlcSize);
            ReadPartnerBlock(data, result, characterSlot, PartnerFestivalOffset, PartnerFestivalStride, 57, PartnerFestivalCount, PartnerFestivalSize);
            return result;
        }

        private static void ReadPartnerBlock(byte[] data, List<PartnerStatEntry> result, int slot, int baseOffset, int stride, int firstIndex, int count, int size)
        {
            int start = baseOffset + slot * stride; Validate(data, start, count * size);
            for (int position = 0; position < count; position++)
            {
                int offset = start + position * size; ushort partnerId = BitConverter.ToUInt16(data, offset);
                int index = firstIndex + position;
                if (!PartnerNames.TryGetValue(index, out string? name)) continue;
                result.Add(new PartnerStatEntry { Index = index, Offset = offset,
                    Name = name,
                    StatType = BitConverter.ToUInt16(data, offset + 2), Materialized = partnerId is not 0 and not ushort.MaxValue });
            }
        }

        public static void WritePartnerStat(byte[] data, PartnerStatEntry entry, ushort statType)
        {
            Validate(data, entry.Offset, 4);
            ushort partnerId = BitConverter.ToUInt16(data, entry.Offset);
            if (partnerId is 0 or ushort.MaxValue) throw new InvalidOperationException("This partner record is not materialized.");
            Array.Copy(BitConverter.GetBytes(statType), 0, data, entry.Offset + 2, 2); entry.StatType = statType;
        }

        private static void Validate(byte[] data, int offset, int length)
        {
            if (offset < 0 || offset + length > data.Length)
                throw new InvalidOperationException("Verified progression block is outside this save version.");
        }
    }
}
