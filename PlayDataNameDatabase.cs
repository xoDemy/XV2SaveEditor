using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace XV2SaveEditor
{
    public sealed record PlayDataNamedValue(int Id, string Name)
    {
        public override string ToString() => Name;
    }

    public static class PlayDataNameDatabase
    {
        private sealed class CharacterEntry
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        private static IReadOnlyList<PlayDataNamedValue>? characters;

        public static IReadOnlyList<PlayDataNamedValue> Characters => characters ??= LoadCharacters();

        public static IReadOnlyList<PlayDataNamedValue> TrainingClasses { get; } = new[]
        {
            new PlayDataNamedValue(0, "Beginner"), new PlayDataNamedValue(1, "Intermediate"),
            new PlayDataNamedValue(2, "Advanced"), new PlayDataNamedValue(3, "Kai"),
            new PlayDataNamedValue(4, "God"), new PlayDataNamedValue(5, "Super")
        };

        public static IReadOnlyList<PlayDataNamedValue> Mentors { get; } = new[]
        {
            new PlayDataNamedValue(255, "None"), new PlayDataNamedValue(0, "Krillin"),
            new PlayDataNamedValue(1, "Tien"), new PlayDataNamedValue(2, "Yamcha"),
            new PlayDataNamedValue(3, "Piccolo"), new PlayDataNamedValue(4, "Raditz"),
            new PlayDataNamedValue(5, "Gohan (Kid)"), new PlayDataNamedValue(6, "Nappa"),
            new PlayDataNamedValue(7, "Vegeta"), new PlayDataNamedValue(8, "Zarbon"),
            new PlayDataNamedValue(9, "Dodoria"), new PlayDataNamedValue(10, "Captain Ginyu"),
            new PlayDataNamedValue(11, "Frieza (1st Form)"), new PlayDataNamedValue(12, "Android 18"),
            new PlayDataNamedValue(13, "Cell (Perfect)"), new PlayDataNamedValue(14, "Lord Slug"),
            new PlayDataNamedValue(15, "Majin Buu"), new PlayDataNamedValue(16, "Hercule"),
            new PlayDataNamedValue(17, "Gohan and Videl"), new PlayDataNamedValue(18, "Gotenks"),
            new PlayDataNamedValue(19, "Turles"), new PlayDataNamedValue(20, "Broly"),
            new PlayDataNamedValue(21, "God of Destruction Beerus"), new PlayDataNamedValue(22, "Pan"),
            new PlayDataNamedValue(23, "Jaco"), new PlayDataNamedValue(24, "Goku"),
            new PlayDataNamedValue(25, "Whis"), new PlayDataNamedValue(26, "Cooler (Final Form)"),
            new PlayDataNamedValue(27, "Android 16"), new PlayDataNamedValue(28, "Gohan (Future)"),
            new PlayDataNamedValue(29, "Bardock"), new PlayDataNamedValue(30, "Hit"),
            new PlayDataNamedValue(31, "Bojack"), new PlayDataNamedValue(32, "Zamasu")
        };

        public static IReadOnlyList<PlayDataNamedValue> Skills(Xv2NameDatabase database, int relativeOffset)
        {
            IEnumerable<NamedSaveValue> source = relativeOffset switch
            {
                140 => database.GetValues(NamedValueKind.SuperSkill),
                144 => database.GetValues(NamedValueKind.UltimateSkill),
                148 => database.GetValues(NamedValueKind.EvasiveSkill),
                152 => database.GetValues(NamedValueKind.SuperSkill)
                    .Concat(database.GetValues(NamedValueKind.UltimateSkill))
                    .Concat(database.GetValues(NamedValueKind.EvasiveSkill))
                    .Concat(database.GetValues(NamedValueKind.AwokenSkill)),
                _ => Enumerable.Empty<NamedSaveValue>()
            };

            return source
                .GroupBy(value => value.SaveId)
                .Select(group => new PlayDataNamedValue(group.Key,
                    string.Join(" / ", group.Select(value => value.Name).Distinct())))
                .OrderBy(value => value.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(value => value.Id)
                .ToList();
        }

        private static IReadOnlyList<PlayDataNamedValue> LoadCharacters()
        {
            Assembly assembly = typeof(PlayDataNameDatabase).Assembly;
            string? resource = assembly.GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith("Data.PlayData.characters.json", StringComparison.OrdinalIgnoreCase));
            if (resource == null) return Array.Empty<PlayDataNamedValue>();
            using Stream stream = assembly.GetManifestResourceStream(resource)!;
            List<CharacterEntry> entries = JsonSerializer.Deserialize<List<CharacterEntry>>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            return entries.Where(entry => !string.IsNullOrWhiteSpace(entry.Name))
                .Select(entry => new PlayDataNamedValue(entry.Id, entry.Name.Trim()))
                .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(entry => entry.Id)
                .ToList();
        }
    }
}
