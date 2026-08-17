using System.Text.Json;

namespace XV2SaveEditor;

public sealed class PresetLibraryFile
{
    public const int CurrentVersion = 1;
    public int Version { get; set; } = CurrentVersion;
    public string Name { get; set; } = "Preset";
    public string SourceCharacter { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public XV2Preset Preset { get; set; } = new();

    public static string LibraryPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "XV2 Save Editor", "Preset Library");

    public static string Store(XV2Preset preset, string name, string sourceCharacter)
    {
        Directory.CreateDirectory(LibraryPath);
        string safe = Sanitize(name);
        string path = Path.Combine(LibraryPath, safe + ".xv2preset");
        for (int number = 2; File.Exists(path); number++) path = Path.Combine(LibraryPath, $"{safe} ({number}).xv2preset");
        PresetLibraryFile file = new() { Name = name, SourceCharacter = sourceCharacter, Preset = Clone(preset) };
        File.WriteAllText(path, JsonSerializer.Serialize(file, new JsonSerializerOptions { WriteIndented = true }));
        return path;
    }

    public static PresetLibraryFile Load(string path)
    {
        PresetLibraryFile file = JsonSerializer.Deserialize<PresetLibraryFile>(File.ReadAllText(path)) ?? throw new InvalidDataException("The preset file is empty.");
        if (file.Version != CurrentVersion || file.Preset == null) throw new InvalidDataException("Unsupported preset library file.");
        return file;
    }

    private static XV2Preset Clone(XV2Preset source) => new()
    {
        Index = source.Index, Top = source.Top, Bottom = source.Bottom, Gloves = source.Gloves, Shoes = source.Shoes, Accessory = source.Accessory,
        SuperSoul = source.SuperSoul, QQBang = source.QQBang, TopColor1 = source.TopColor1, TopColor2 = source.TopColor2, TopColor3 = source.TopColor3, TopColor4 = source.TopColor4,
        BottomColor1 = source.BottomColor1, BottomColor2 = source.BottomColor2, BottomColor3 = source.BottomColor3, BottomColor4 = source.BottomColor4,
        GlovesColor1 = source.GlovesColor1, GlovesColor2 = source.GlovesColor2, GlovesColor3 = source.GlovesColor3, GlovesColor4 = source.GlovesColor4,
        ShoesColor1 = source.ShoesColor1, ShoesColor2 = source.ShoesColor2, ShoesColor3 = source.ShoesColor3, ShoesColor4 = source.ShoesColor4,
        SuperSkill1 = source.SuperSkill1, SuperSkill2 = source.SuperSkill2, SuperSkill3 = source.SuperSkill3, SuperSkill4 = source.SuperSkill4,
        UltimateSkill1 = source.UltimateSkill1, UltimateSkill2 = source.UltimateSkill2, EvasiveSkill = source.EvasiveSkill, BlastSkill = source.BlastSkill, AwokenSkill = source.AwokenSkill
    };

    private static string Sanitize(string value)
    {
        string result = string.Concat(value.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch)).Trim();
        return string.IsNullOrWhiteSpace(result) ? "Preset" : result;
    }
}
