using System;
using System.IO;
using System.Text.Json;

namespace XV2SaveEditor
{
    public sealed class EditorPreferences
    {
        public string? LastSavePath { get; set; }
        public List<SaveProfile> SaveProfiles { get; set; } = new();
        public List<string> RecentSavePaths { get; set; } = new();
        public bool AutoOpenLastSave { get; set; } = true;
        public bool HasSeenWelcome { get; set; }

        private static string PreferencesPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "XV2SaveEditor", "preferences.json");

        public static EditorPreferences Load()
        {
            try
            {
                return File.Exists(PreferencesPath)
                    ? JsonSerializer.Deserialize<EditorPreferences>(File.ReadAllText(PreferencesPath)) ?? new()
                    : new();
            }
            catch { return new(); }
        }

        public void Save()
        {
            string? directory = Path.GetDirectoryName(PreferencesPath);
            if (directory != null) Directory.CreateDirectory(directory);
            File.WriteAllText(PreferencesPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    public sealed record SaveProfile(string Name, string Path);
}
