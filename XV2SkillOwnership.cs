namespace XV2SaveEditor
{
    public sealed class XV2SkillOwnership
    {
        public int SlotIndex { get; init; }
        public bool Owned { get; set; }
        public ushort ID1 { get; init; }
        public ushort Type { get; init; }
        public ushort ID2 { get; init; }
        public NamedValueKind Category { get; init; }
        public string Name { get; init; } = "";

        public string DisplayName => Name;
        public override string ToString() => DisplayName;
    }
}
