using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace XV2SaveEditor
{
    public sealed class ExcacFile
    {
        public const int BaseOffset = 95280;
        public const int BaseLength = 50888;
        public const int DlcOffset = 519816;
        public const int DlcLength = 25392;
        public const int CompactLength = 21192;
        public const int CompactOffsetWithinBase = BaseLength - CompactLength;
        private static readonly string[] RaceNames = { "HUM", "HUF", "SYM", "SYF", "NMC", "FRI", "MAM", "MAF" };

        public string Name { get; init; } = "";
        public int Level { get; init; }
        public int Race { get; init; }
        public byte[] Base { get; init; } = Array.Empty<byte>();
        public byte[] Dlc { get; init; } = Array.Empty<byte>();
        public byte[] CompactData { get; init; } = Array.Empty<byte>();

        public static ExcacFile FromSave(byte[] data, int slot, XV2Character character)
        {
            ValidateSlotAndData(data, slot);
            return new ExcacFile
            {
                Name = character.Name,
                Level = character.Level,
                Race = character.Race,
                Base = data.AsSpan(BaseOffset + BaseLength * slot, BaseLength).ToArray(),
                Dlc = data.AsSpan(DlcOffset + DlcLength * slot, DlcLength).ToArray()
            };
        }

        public void Save(string path)
        {
            string race = Race >= 0 && Race < RaceNames.Length ? RaceNames[Race] : Race.ToString(CultureInfo.InvariantCulture);
            var document = new XDocument(
                new XComment("Listed Name, Race and Level values are for UI purposes only. Editing them here won't affect the actual character."),
                new XElement("CharacterExport",
                    new XAttribute("Name", Name), new XAttribute("Level", Level), new XAttribute("Race", race),
                    new XElement("Base", new XAttribute("bytes", string.Join(", ", Base))),
                    new XElement("DLC", new XAttribute("bytes", string.Join(", ", Dlc)))));
            document.Save(path);
        }

        public static ExcacFile Load(string path)
        {
            XElement root = XDocument.Load(path).Root ?? throw new InvalidDataException("The .excac file has no root element.");
            int race = ParseRace((string?)root.Attribute("Race"));
            XElement? baseElement = FindBlock(root, "Base");
            XElement? compactElement = FindBlock(root, "Data");
            byte[] baseBytes = baseElement == null ? Array.Empty<byte>() : ParseBytes(baseElement, "Base");
            byte[] compactBytes = baseElement == null && compactElement != null ? ParseCompactData(compactElement) : Array.Empty<byte>();
            byte[] dlcBytes = baseElement == null ? Array.Empty<byte>() : ParseBytes(FindBlock(root, "DLC"), "DLC", allowMissing: true);
            if (baseBytes.Length == 0 && compactBytes.Length == 0)
                throw new InvalidDataException("The .excac file contains neither a Base block nor a supported compact Data block.");
            if (baseBytes.Length != 0 && baseBytes.Length != BaseLength) throw new InvalidDataException($"Invalid .excac base block ({baseBytes.Length:N0} bytes; expected {BaseLength:N0}).");
            if (compactBytes.Length != 0 && compactBytes.Length != CompactLength) throw new InvalidDataException($"Invalid compact .excac Data block ({compactBytes.Length:N0} bytes; expected {CompactLength:N0}).");
            if (dlcBytes.Length != 0 && dlcBytes.Length != DlcLength) throw new InvalidDataException($"Invalid .excac DLC block ({dlcBytes.Length:N0} bytes; expected {DlcLength:N0}).");
            return new ExcacFile
            {
                Name = (string?)root.Attribute("Name") ?? "Imported CaC",
                Level = (int?)root.Attribute("Level") ?? 0,
                Race = race,
                Base = baseBytes,
                Dlc = baseBytes.Length == 0 ? Array.Empty<byte>() : dlcBytes.Length == 0 ? new byte[DlcLength] : dlcBytes,
                CompactData = compactBytes
            };
        }

        public void ImportInto(byte[] data, int slot)
        {
            ValidateSlotAndData(data, slot);
            if (CompactData.Length == CompactLength)
            {
                // Compact exports are the exact 21,192-byte character tail of
                // the verified 50,888-byte Base block. Preserve the target's
                // quest/progression prefix and its DLC block.
                CompactData.CopyTo(data, BaseOffset + BaseLength * slot + CompactOffsetWithinBase);
                return;
            }
            Base.CopyTo(data, BaseOffset + BaseLength * slot);
            Dlc.CopyTo(data, DlcOffset + DlcLength * slot);
        }

        private static XElement? FindBlock(XElement root, string blockName)
        {
            // Older/community .excac exporters are not entirely consistent about
            // element casing, XML namespaces, or wrapping the data blocks. Match
            // by local name so Unicode character metadata cannot affect discovery.
            return root.DescendantsAndSelf().FirstOrDefault(element =>
                string.Equals(element.Name.LocalName, blockName, StringComparison.OrdinalIgnoreCase));
        }

        private static byte[] ParseBytes(XElement? element, string blockName, bool allowMissing = false)
        {
            if (element == null)
            {
                if (allowMissing) return Array.Empty<byte>();
                throw new InvalidDataException($"The .excac file is missing its {blockName} block.");
            }
            XAttribute? bytesAttribute = element.Attributes().FirstOrDefault(attribute =>
                string.Equals(attribute.Name.LocalName, "bytes", StringComparison.OrdinalIgnoreCase));
            string value = bytesAttribute?.Value ?? element.Value;
            if (string.IsNullOrWhiteSpace(value)) return Array.Empty<byte>();
            return value.Split(new[] { ',', ' ', '\r', '\n', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => byte.Parse(part, CultureInfo.InvariantCulture)).ToArray();
        }

        private static byte[] ParseCompactData(XElement element)
        {
            string value = element.Value.Trim();
            if (value.Length == 0) return Array.Empty<byte>();
            try { return Convert.FromBase64String(value); }
            catch (FormatException ex) { throw new InvalidDataException("The compact .excac Data block is not valid Base64.", ex); }
        }

        private static int ParseRace(string? value)
        {
            int named = Array.FindIndex(RaceNames, name => string.Equals(name, value, StringComparison.OrdinalIgnoreCase));
            if (named >= 0) return named;
            return int.TryParse(value, out int numeric) ? numeric : -1;
        }

        private static void ValidateSlotAndData(byte[] data, int slot)
        {
            if (slot < 0 || slot >= 8) throw new ArgumentOutOfRangeException(nameof(slot));
            if (data.Length < DlcOffset + DlcLength * (slot + 1)) throw new InvalidDataException("This save does not contain the verified DLC CaC block.");
        }
    }
}
