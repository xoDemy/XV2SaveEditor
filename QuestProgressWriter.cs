using System;

namespace XV2SaveEditor
{
    public static class QuestProgressWriter
    {
        public static void Write(byte[] data, XV2QuestProgress quest)
        {
            int offset = quest.RecordOffset;
            if (offset < 0 || offset + 24 > data.Length) throw new ArgumentOutOfRangeException(nameof(quest.RecordOffset));
            if (BitConverter.ToInt32(data, offset) != quest.Type || BitConverter.ToInt32(data, offset + 4) != quest.ID)
                throw new InvalidOperationException("Quest record identity changed; refusing to write.");
            WriteInt(data, offset + 8, quest.State);
            WriteInt(data, offset + 12, quest.Rank);
            WriteInt(data, offset + 16, quest.WinCondition);
            WriteInt(data, offset + 20, quest.Score);
        }

        private static void WriteInt(byte[] data, int offset, int value) => Array.Copy(BitConverter.GetBytes(value), 0, data, offset, 4);
    }
}
