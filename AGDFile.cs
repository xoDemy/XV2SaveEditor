using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XV2SaveEditor
{
    public class AGDEntry
    {
        public int Level { get; set; }

        public int XpToNextLevel { get; set; }

        public int XpToThisLevel { get; set; }

        public int AttributePointsGained { get; set; }
    }


    public class AGDFile
    {
        public List<AGDEntry> Entries { get; private set; }
            = new List<AGDEntry>();


        // =========================================================
        // LOAD AGD
        // =========================================================

        public static AGDFile Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "avatar_growth_data.agd could not be found.",
                    path
                );
            }

            byte[] data =
                File.ReadAllBytes(path);

            return Load(data);
        }


        public static AGDFile Load(byte[] data)
        {
            if (data.Length < 16)
            {
                throw new InvalidDataException(
                    "The AGD file is too small."
                );
            }

            AGDFile file =
                new AGDFile();


            int entryCount =
                BitConverter.ToInt32(
                    data,
                    8
                );

            int offset =
                BitConverter.ToInt32(
                    data,
                    12
                );


            if (entryCount <= 0)
            {
                throw new InvalidDataException(
                    "The AGD file contains no level entries."
                );
            }


            for (int i = 0; i < entryCount; i++)
            {
                if (offset + 16 > data.Length)
                {
                    throw new InvalidDataException(
                        "The AGD level table is incomplete."
                    );
                }


                AGDEntry entry =
                    new AGDEntry
                    {
                        Level =
                            BitConverter.ToInt32(
                                data,
                                offset
                            ),

                        XpToNextLevel =
                            BitConverter.ToInt32(
                                data,
                                offset + 4
                            ),

                        XpToThisLevel =
                            BitConverter.ToInt32(
                                data,
                                offset + 8
                            ),

                        AttributePointsGained =
                            BitConverter.ToInt32(
                                data,
                                offset + 12
                            )
                    };


                file.Entries.Add(
                    entry
                );

                offset += 16;
            }


            return file;
        }


        // =========================================================
        // XP FOR CURRENT LEVEL
        // =========================================================

        public int ExperienceForLevel(
            int level)
        {
            int result = 0;

            foreach (AGDEntry entry in Entries)
            {
                if (level < entry.Level)
                {
                    break;
                }

                result =
                    entry.XpToThisLevel;

                if (level == entry.Level)
                {
                    break;
                }
            }

            return result;
        }


        // =========================================================
        // XP REQUIRED FOR NEXT LEVEL
        // =========================================================

        public int ExperienceForNextLevel(
            int level)
        {
            int total = 0;

            foreach (AGDEntry entry in Entries)
            {
                if (level < entry.Level)
                {
                    break;
                }

                total +=
                    entry.XpToNextLevel;

                if (level == entry.Level)
                {
                    break;
                }
            }

            return total;
        }


        // =========================================================
        // ATTRIBUTE POINTS AVAILABLE AT LEVEL
        // =========================================================

        public int AttributePointsForLevel(
            int level)
        {
            int total = 0;

            foreach (AGDEntry entry in Entries)
            {
                if (level < entry.Level)
                {
                    break;
                }

                total +=
                    entry.AttributePointsGained;

                if (level == entry.Level)
                {
                    break;
                }
            }

            return total;
        }


        // =========================================================
        // FIX XP IF LEVEL CHANGED
        // =========================================================

        public int CalculateExperienceRequired(
            int currentExperience,
            int level)
        {
            int minimum =
                ExperienceForLevel(
                    level
                );

            int nextLevel =
                ExperienceForNextLevel(
                    level
                );


            // Existing XP is already valid
            // for the selected level.
            if (
                currentExperience >= minimum &&
                currentExperience < nextLevel)
            {
                return currentExperience;
            }


            // Otherwise use the minimum XP
            // required for this level.
            return minimum;
        }


        // =========================================================
        // MAX LEVEL
        // =========================================================

        public int GetMaximumLevel()
        {
            if (Entries.Count == 0)
            {
                return 1;
            }

            return Entries.Max(
                x => x.Level
            );
        }


        public bool IsMaxLevel(
            int level)
        {
            if (Entries.Count == 0)
            {
                return false;
            }

            return Entries[
                Entries.Count - 1
            ].Level == level;
        }
    }
}