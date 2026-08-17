using System.Diagnostics;

namespace XV2SaveEditor;

public static class CacLibrary
{
    private static readonly string[] FolderNames =
    {
        "Earthling Male", "Earthling Female", "Saiyan Male", "Saiyan Female",
        "Namekian", "Frieza Race", "Majin Male", "Majin Female"
    };

    public static string RootPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "XV2 Save Editor", "CaC Library");

    public static IReadOnlyList<string> Races => FolderNames;

    public sealed record Entry(string Path, string Name, int Race, string RaceName, int Level, DateTime Modified);

    public static List<Entry> ReadEntries()
    {
        EnsureFolders();
        List<Entry> entries = new();
        foreach (string path in Directory.EnumerateFiles(RootPath, "*.excac", SearchOption.AllDirectories))
        {
            try
            {
                ExcacFile character = ExcacFile.Load(path);
                string race = character.Race >= 0 && character.Race < FolderNames.Length ? FolderNames[character.Race] : "Unknown Race";
                entries.Add(new Entry(path, character.Name, character.Race, race, character.Level, File.GetLastWriteTime(path)));
            }
            catch { }
        }
        return entries;
    }

    public static string GetRaceFolder(int race)
    {
        string name = race >= 0 && race < FolderNames.Length ? FolderNames[race] : "Unknown Race";
        string path = Path.Combine(RootPath, name);
        Directory.CreateDirectory(path);
        return path;
    }

    public static void EnsureFolders()
    {
        Directory.CreateDirectory(RootPath);
        foreach (string name in FolderNames) Directory.CreateDirectory(Path.Combine(RootPath, name));
    }

    public static string Store(ExcacFile character)
    {
        string folder = GetRaceFolder(character.Race);
        string name = Sanitize(string.IsNullOrWhiteSpace(character.Name) ? "Unnamed CaC" : character.Name);
        string path = Path.Combine(folder, name + ".excac");
        for (int number = 2; File.Exists(path); number++)
            path = Path.Combine(folder, $"{name} ({number}).excac");
        character.Save(path);
        return path;
    }

    public static void OpenFolder(string path)
    {
        EnsureFolders();
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
    }

    private static string Sanitize(string value)
    {
        string result = string.Concat(value.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch)).Trim();
        return string.IsNullOrWhiteSpace(result) ? "Unnamed CaC" : result;
    }
}
