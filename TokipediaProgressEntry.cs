using System;
using System.Collections.Generic;

namespace XV2SaveEditor
{
    public sealed class TokipediaProgressEntry
    {
        public int ID { get; init; }
        public int Offset { get; init; }
        public ulong Flags { get; set; }
        public List<string> BranchingPaths { get; init; } = new List<string>();
        public List<string> AlternatePaths { get; init; } = new List<string>();
        public override string ToString() => $"Entry {ID}: {CountCompleted()} / {BranchingPaths.Count + AlternatePaths.Count} referenced routes";
        public int CountCompleted()
        {
            int count = 0;
            foreach (string path in BranchingPaths) if ((Flags & TokipediaFlagMap.Get(path)) != 0) count++;
            foreach (string path in AlternatePaths) if ((Flags & TokipediaFlagMap.Get(path)) != 0) count++;
            return count;
        }
    }

    public static class TokipediaFlagMap
    {
        private static readonly string[] Names = { "Krillin","Tien","Yamcha","Piccolo","Raditz","KidGohan","Nappa","Vegeta","Zarbon","Dodoria","Ginyu","Frieza","Android18","Cell","LordSlug","MajinBuu","Hercule","AdultGohan","Gotenks","Turles","Broly","Beerus","Pan","Jaco","Goku","Whis","Cooler","Android16","FutureGohan","Bardock","Hit","Bojack","Zamasu" };
        public static ulong Get(string name) => name switch
        {
            "Videl" => 0x200000000000UL, "Fuu" => 0x400000000000UL,
            _ => Array.FindIndex(Names, x => x.Equals(name, StringComparison.OrdinalIgnoreCase)) is int index && index >= 0 ? 1UL << index : 0
        };
    }
}
