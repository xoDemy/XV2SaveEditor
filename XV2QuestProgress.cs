namespace XV2SaveEditor
{
    public enum XV2QuestCategory { TimePatrol, Parallel, TimeRift, Mentor, Expert, Raid, ElderKai, FriezaSiege }

    public sealed class XV2QuestProgress
    {
        public int CharacterSlot { get; init; }
        public XV2QuestCategory Category { get; init; }
        public int RecordOffset { get; init; }
        public int Type { get; init; }
        public int ID { get; init; }
        public int State { get; set; }
        public int Rank { get; set; }
        public int WinCondition { get; set; }
        public int Score { get; set; }
        public bool IsCleared => State == 3;
        public string DisplayName => $"{Category} {ID:D3} | {(IsCleared ? "Cleared" : State == 0 ? "Locked" : State == 2 ? "New" : "Not complete")} | Rank {RankName} | Score {Score:N0}";
        public string RankName => Rank switch { 0 => "—", 1 => "D", 2 => "C", 3 => "B", 4 => "A", 5 => "S", 6 => "Z", 7 => "Super", _ => Rank.ToString() };
        public override string ToString() => DisplayName;
    }
}
