using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace XV2SaveEditor
{
    public sealed class TokipediaRequirement
    {
        public int ID { get; init; }
        public List<string> BranchingPaths { get; init; } = new List<string>();
        public List<string> AlternatePaths { get; init; } = new List<string>();
    }

    public sealed class ProgressReferenceDatabase
    {
        public List<SystemFlagDefinition> SystemFlags { get; } = new List<SystemFlagDefinition>();
        public List<TokipediaRequirement> Tokipedia { get; } = new List<TokipediaRequirement>();

        public void Load()
        {
            SystemFlags.Clear(); Tokipedia.Clear();
            XDocument sys = LoadEmbedded("SysFlags.xml");
            foreach (XElement item in sys.Root!.Elements("SysFlag"))
                SystemFlags.Add(new SystemFlagDefinition
                {
                    Index = (int)item.Attribute("Index")!, Name = (string?)item.Attribute("Name") ?? "",
                    Type = (string?)item.Attribute("Type") ?? "Other",
                    Conditions1 = Split((string?)item.Attribute("Conditions1")), Conditions2 = Split((string?)item.Attribute("Conditions2")),
                    ChangeIfSet = bool.TryParse((string?)item.Attribute("ChangeIfSet"), out bool value) && value
                });
            XDocument toki = LoadEmbedded("Tokipedia.xml");
            foreach (XElement item in toki.Root!.Elements("TokipediaEntry"))
                Tokipedia.Add(new TokipediaRequirement
                {
                    ID = (int)item.Attribute("ID")!,
                    BranchingPaths = Split((string?)item.Element("BranchingPaths")?.Attribute("Flags")),
                    AlternatePaths = Split((string?)item.Element("AlternatePaths")?.Attribute("Flags"))
                });
        }

        private static List<string> Split(string? value) => string.IsNullOrWhiteSpace(value) || value == "None"
            ? new List<string>() : value.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();

        private static XDocument LoadEmbedded(string file)
        {
            Assembly assembly = typeof(ProgressReferenceDatabase).Assembly;
            string resource = assembly.GetManifestResourceNames().Single(x => x.EndsWith("Data.Progress." + file, StringComparison.OrdinalIgnoreCase));
            using Stream stream = assembly.GetManifestResourceStream(resource) ?? throw new FileNotFoundException(file);
            return XDocument.Load(stream);
        }
    }
}
