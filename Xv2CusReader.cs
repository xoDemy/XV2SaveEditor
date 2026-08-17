using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XV2SaveEditor
{
    public enum Xv2CusSkillKind
    {
        Super,
        Ultimate,
        Evasive,
        Blast,
        Awoken
    }


    public class Xv2CusSkill
    {
        public string ShortCode { get; set; } = "";

        // Save-facing ID stored in presets.
        public ushort ID1 { get; set; }

        // Category-local ID used by the MSG tables.
        public ushort ID2 { get; set; }

        public Xv2CusSkillKind Kind { get; set; }


        public override string ToString()
        {
            return $"{ShortCode} | ID1 {ID1} | ID2 {ID2}";
        }
    }


    public class Xv2CusFile
    {
        public List<Xv2CusSkill> Skills { get; set; }
            = new List<Xv2CusSkill>();


        public Xv2CusSkill? FindById1(
            Xv2CusSkillKind kind,
            int id1)
        {
            foreach (
                Xv2CusSkill skill
                in Skills)
            {
                if (
                    skill.Kind == kind &&
                    skill.ID1 == id1)
                {
                    return skill;
                }
            }

            return null;
        }
    }


    public static class Xv2CusReader
    {
        private const int HeaderSize =
            0x48;

        private const int ModernRecordSize =
            92;


        public static Xv2CusFile Load(
            string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "CUS file was not found.",
                    path
                );
            }


            return Load(
                File.ReadAllBytes(path)
            );
        }


        public static Xv2CusFile Load(
            byte[] data)
        {
            if (data.Length < HeaderSize)
            {
                throw new InvalidDataException(
                    "CUS file is too small."
                );
            }


            string signature =
                Encoding.ASCII.GetString(
                    data,
                    0,
                    4
                );


            if (signature != "#CUS")
            {
                throw new InvalidDataException(
                    "Invalid CUS signature."
                );
            }


            Xv2CusFile file =
                new Xv2CusFile();


            // =====================================================
            // COUNTS
            // =====================================================

            int superCount =
                BitConverter.ToInt32(
                    data,
                    0x10
                );


            int ultimateCount =
                BitConverter.ToInt32(
                    data,
                    0x14
                );


            int evasiveCount =
                BitConverter.ToInt32(
                    data,
                    0x18
                );


            int blastCount =
                BitConverter.ToInt32(
                    data,
                    0x20
                );


            int awokenCount =
                BitConverter.ToInt32(
                    data,
                    0x24
                );


            // =====================================================
            // OFFSETS
            // =====================================================

            int superOffset =
                BitConverter.ToInt32(
                    data,
                    0x28
                );


            int ultimateOffset =
                BitConverter.ToInt32(
                    data,
                    0x2C
                );


            int evasiveOffset =
                BitConverter.ToInt32(
                    data,
                    0x30
                );


            int blastOffset =
                BitConverter.ToInt32(
                    data,
                    0x38
                );


            int awokenOffset =
                BitConverter.ToInt32(
                    data,
                    0x3C
                );


            ValidateSection(
                data,
                superOffset,
                superCount,
                "Super"
            );


            ValidateSection(
                data,
                ultimateOffset,
                ultimateCount,
                "Ultimate"
            );


            ValidateSection(
                data,
                evasiveOffset,
                evasiveCount,
                "Evasive"
            );


            ValidateSection(
                data,
                blastOffset,
                blastCount,
                "Blast"
            );


            ValidateSection(
                data,
                awokenOffset,
                awokenCount,
                "Awoken"
            );


            ReadSkillSection(
                data,
                superOffset,
                superCount,
                Xv2CusSkillKind.Super,
                file.Skills
            );


            ReadSkillSection(
                data,
                ultimateOffset,
                ultimateCount,
                Xv2CusSkillKind.Ultimate,
                file.Skills
            );


            ReadSkillSection(
                data,
                evasiveOffset,
                evasiveCount,
                Xv2CusSkillKind.Evasive,
                file.Skills
            );


            ReadSkillSection(
                data,
                blastOffset,
                blastCount,
                Xv2CusSkillKind.Blast,
                file.Skills
            );


            ReadSkillSection(
                data,
                awokenOffset,
                awokenCount,
                Xv2CusSkillKind.Awoken,
                file.Skills
            );


            return file;
        }


        // =========================================================
        // READ SECTION
        // =========================================================

        private static void ReadSkillSection(
            byte[] data,
            int sectionOffset,
            int count,
            Xv2CusSkillKind kind,
            List<Xv2CusSkill> destination)
        {
            for (
                int i = 0;
                i < count;
                i++)
            {
                int offset =
                    sectionOffset +
                    (i * ModernRecordSize);


                EnsureRange(
                    data,
                    offset,
                    ModernRecordSize
                );


                string shortCode =
                    Encoding.ASCII
                        .GetString(
                            data,
                            offset + 0x00,
                            4
                        )
                        .TrimEnd('\0');


                ushort id1 =
                    BitConverter.ToUInt16(
                        data,
                        offset + 0x08
                    );


                ushort id2 =
                    BitConverter.ToUInt16(
                        data,
                        offset + 0x0A
                    );


                // 0xFFFF is the usual none/empty sentinel.
                // It should not normally appear among defined
                // skill records, but skipping it is safest.
                if (
                    id1 == ushort.MaxValue ||
                    id2 == ushort.MaxValue)
                {
                    continue;
                }


                Xv2CusSkill skill =
                    new Xv2CusSkill
                    {
                        ShortCode =
                            shortCode,

                        ID1 =
                            id1,

                        ID2 =
                            id2,

                        Kind =
                            kind
                    };


                destination.Add(
                    skill
                );
            }
        }


        // =========================================================
        // SECTION VALIDATION
        // =========================================================

        private static void ValidateSection(
            byte[] data,
            int offset,
            int count,
            string sectionName)
        {
            if (count < 0)
            {
                throw new InvalidDataException(
                    $"{sectionName} CUS count is invalid."
                );
            }


            if (count == 0)
            {
                return;
            }


            long end =
                (long)offset +
                ((long)count * ModernRecordSize);


            if (
                offset < HeaderSize ||
                end > data.Length)
            {
                throw new InvalidDataException(
                    $"{sectionName} CUS section points outside the file."
                );
            }
        }


        // =========================================================
        // RANGE CHECK
        // =========================================================

        private static void EnsureRange(
            byte[] data,
            int offset,
            int length)
        {
            if (
                offset < 0 ||
                length < 0 ||
                offset + length >
                data.Length)
            {
                throw new InvalidDataException(
                    "CUS structure points outside the file."
                );
            }
        }
    }
}