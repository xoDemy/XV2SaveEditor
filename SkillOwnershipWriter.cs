using System;

namespace XV2SaveEditor
{
    public static class SkillOwnershipWriter
    {
        public static void WriteOwned(byte[] data, XV2SkillOwnership skill)
        {
            int offset = SkillOwnershipReader.GetOffset(skill.Category);
            SkillOwnershipReader.ValidateSection(data, offset);
            if (skill.SlotIndex < 0 || skill.SlotIndex >= SaveOffsets.SkillRecordCount)
                throw new ArgumentOutOfRangeException(nameof(skill.SlotIndex));
            int record = offset + skill.SlotIndex * SaveOffsets.SkillRecordSize;
            if (BitConverter.ToUInt16(data, record + 2) != skill.ID1 ||
                BitConverter.ToUInt16(data, record + 4) != skill.Type ||
                BitConverter.ToUInt16(data, record + 6) != skill.ID2)
                throw new InvalidOperationException("Skill record identity changed; refusing to write.");
            byte[] value = BitConverter.GetBytes((short)(skill.Owned ? 1 : 0));
            data[record] = value[0];
            data[record + 1] = value[1];
        }
    }
}
