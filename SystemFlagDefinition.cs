using System.Collections.Generic;

namespace XV2SaveEditor
{
    public sealed class SystemFlagDefinition
    {
        public int Index { get; init; }
        public string Name { get; init; } = "";
        public string Type { get; init; } = "";
        public List<string> Conditions1 { get; init; } = new List<string>();
        public List<string> Conditions2 { get; init; } = new List<string>();
        public bool ChangeIfSet { get; init; }
        public bool IsConditionDriven => Type != "Other" && (Conditions1.Count > 0 || Conditions2.Count > 0);
        public override string ToString() => $"[{Index}] {Name}";
    }
}
