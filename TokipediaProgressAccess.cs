using System;
using System.Collections.Generic;

namespace XV2SaveEditor
{
    public static class TokipediaProgressAccess
    {
        public const int BaseOffset = 532280;
        public const int CharacterStride = 25392;
        public static List<TokipediaProgressEntry> Read(byte[] data, int slot, IEnumerable<TokipediaRequirement> requirements)
        {
            List<TokipediaProgressEntry> result = new List<TokipediaProgressEntry>();
            foreach (TokipediaRequirement requirement in requirements)
            {
                int offset = BaseOffset + CharacterStride * slot + requirement.ID * 8;
                if (offset < 0 || offset + 8 > data.Length) throw new ArgumentException("Tokipedia section is outside this save.");
                result.Add(new TokipediaProgressEntry { ID = requirement.ID, Offset = offset, Flags = BitConverter.ToUInt64(data, offset), BranchingPaths = requirement.BranchingPaths, AlternatePaths = requirement.AlternatePaths });
            }
            return result;
        }
        public static void Write(byte[] data, TokipediaProgressEntry entry) => Array.Copy(BitConverter.GetBytes(entry.Flags), 0, data, entry.Offset, 8);
    }
}
