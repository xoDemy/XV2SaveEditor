using System;
using System.Collections.Generic;
using System.IO;

namespace XV2SaveEditor
{
    public static class SkillOwnershipReader
    {
        public static List<XV2SkillOwnership> Read(byte[] data, NamedValueKind category, Xv2NameDatabase names)
        {
            int offset = GetOffset(category);
            ValidateSection(data, offset);
            List<XV2SkillOwnership> result = new List<XV2SkillOwnership>();
            for (int slot = 0; slot < SaveOffsets.SkillRecordCount; slot++)
            {
                int record = offset + slot * SaveOffsets.SkillRecordSize;
                if (BitConverter.ToInt32(data, record) == -1 || data[record + 4] == byte.MaxValue) continue;
                ushort id1 = BitConverter.ToUInt16(data, record + 2);
                ushort id2 = BitConverter.ToUInt16(data, record + 6);
                result.Add(new XV2SkillOwnership
                {
                    SlotIndex = slot,
                    Owned = BitConverter.ToInt16(data, record) != 0,
                    ID1 = id1,
                    Type = BitConverter.ToUInt16(data, record + 4),
                    ID2 = id2,
                    Category = category,
                    Name = names.GetOrUnknown(category, id1).Name
                });
            }
            return result;
        }

        public static int GetOffset(NamedValueKind category) => category switch
        {
            NamedValueKind.SuperSkill => SaveOffsets.SuperSkills,
            NamedValueKind.UltimateSkill => SaveOffsets.UltimateSkills,
            NamedValueKind.EvasiveSkill => SaveOffsets.EvasiveSkills,
            NamedValueKind.AwokenSkill => SaveOffsets.AwokenSkills,
            _ => throw new ArgumentOutOfRangeException(nameof(category), "Not a verified skill ownership category.")
        };

        public static void ValidateSection(byte[] data, int offset)
        {
            int length = SaveOffsets.SkillRecordCount * SaveOffsets.SkillRecordSize;
            if (offset < 0 || offset + length > data.Length)
                throw new InvalidDataException("Verified skill ownership section is outside this save.");
        }
    }
}
