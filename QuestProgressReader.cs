using System;
using System.Collections.Generic;
using System.IO;

namespace XV2SaveEditor
{
    public static class QuestProgressReader
    {
        private const int CharacterStride = 50888;
        private const int ExtendedStride = 25392;

        public static List<XV2QuestProgress> Read(byte[] data, int characterSlot)
        {
            if (characterSlot < 0 || characterSlot >= 8) throw new ArgumentOutOfRangeException(nameof(characterSlot));
            List<XV2QuestProgress> result = new List<XV2QuestProgress>();
            ReadSection(data, result, characterSlot, XV2QuestCategory.TimePatrol, 97840 + CharacterStride * characterSlot, 128);
            ReadSection(data, result, characterSlot, XV2QuestCategory.Parallel, 100912 + CharacterStride * characterSlot, 192);
            ReadSection(data, result, characterSlot, XV2QuestCategory.TimeRift, 105520 + CharacterStride * characterSlot, 128);
            ReadSection(data, result, characterSlot, XV2QuestCategory.Mentor, 108592 + CharacterStride * characterSlot, 256);
            ReadSection(data, result, characterSlot, XV2QuestCategory.Expert, 114736 + CharacterStride * characterSlot, 96);
            ReadSection(data, result, characterSlot, XV2QuestCategory.Raid, 117040 + CharacterStride * characterSlot, 96);
            ReadSection(data, result, characterSlot, XV2QuestCategory.ElderKai, 119344 + CharacterStride * characterSlot, 64);
            ReadSection(data, result, characterSlot, XV2QuestCategory.FriezaSiege, 120880 + CharacterStride * characterSlot, 128);
            ReadSection(data, result, characterSlot, XV2QuestCategory.TimePatrol, 527144 + ExtendedStride * characterSlot, 30);
            return result;
        }

        private static void ReadSection(byte[] data, List<XV2QuestProgress> result, int characterSlot, XV2QuestCategory category, int offset, int count)
        {
            const int size = 24;
            if (offset < 0 || offset + count * size > data.Length) throw new InvalidDataException("Verified quest section is outside this save.");
            for (int i = 0; i < count; i++)
            {
                int record = offset + i * size;
                int type = BitConverter.ToInt32(data, record);
                if (type == -1) break;
                result.Add(new XV2QuestProgress
                {
                    CharacterSlot = characterSlot, Category = category, RecordOffset = record,
                    Type = type, ID = BitConverter.ToInt32(data, record + 4),
                    State = BitConverter.ToInt32(data, record + 8), Rank = BitConverter.ToInt32(data, record + 12),
                    WinCondition = BitConverter.ToInt32(data, record + 16), Score = BitConverter.ToInt32(data, record + 20)
                });
            }
        }
    }
}
