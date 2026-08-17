using System;
using System.Collections.Generic;

namespace XV2SaveEditor
{
    public enum PlayDataValueType
    {
        Byte,
        UInt16,
        Int32
    }

    public sealed record PlayDataField(
        string Group,
        string Name,
        int RelativeOffset,
        PlayDataValueType ValueType,
        decimal Minimum,
        decimal Maximum,
        string Description = "");

    public static class PlayDataAccess
    {
        // Verified against Xv2CoreLib.SAV.PlayData.Read/Write. The record starts
        // 1,300 bytes into each already-verified 50,888-byte CaC Base block.
        public const int OffsetWithinCharacter = 1300;
        public const int RecordLength = 208;

        public static readonly PlayDataField OnlineWinsTotalField =
            new("Online Record", "Online wins", 64, PlayDataValueType.UInt16, 0, 65535);

        public static readonly IReadOnlyList<PlayDataField> Fields = new[]
        {
            new PlayDataField("Training", "Current mentor", 0, PlayDataValueType.Byte, 0, 255),
            new PlayDataField("Training", "Training class", 1, PlayDataValueType.Byte, 0, 5,
                "0 Beginner, 1 Intermediate, 2 Advanced, 3 Kai, 4 God, 5 Super"),
            new PlayDataField("Training", "Training experience", 2, PlayDataValueType.UInt16, 0, 65535),

            new PlayDataField("Base Activity", "Capsule Corporation progress (%)", 128, PlayDataValueType.Byte, 0, 100),
            new PlayDataField("Base Activity", "Hercule House progress (%)", 129, PlayDataValueType.Byte, 0, 100),
            new PlayDataField("Base Activity", "Guru's House progress (%)", 130, PlayDataValueType.Byte, 0, 100),
            new PlayDataField("Base Activity", "Frieza Spaceship progress (%)", 131, PlayDataValueType.Byte, 0, 100),
            new PlayDataField("Base Activity", "Buu's House progress (%)", 132, PlayDataValueType.Byte, 0, 100),
            new PlayDataField("Base Activity", "Times trained with Vegeta", 120, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Base Activity", "Times food given to Majin", 114, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Base Activity", "Times defended Dragon Balls", 116, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Base Activity", "Saviors", 118, PlayDataValueType.UInt16, 0, 65535),

            new PlayDataField("Play Trends", "Favorite character", 136, PlayDataValueType.Int32, -1, int.MaxValue),
            new PlayDataField("Play Trends", "Favorite Super Skill", 140, PlayDataValueType.Int32, -1, int.MaxValue),
            new PlayDataField("Play Trends", "Favorite Ultimate Skill", 144, PlayDataValueType.Int32, -1, int.MaxValue),
            new PlayDataField("Play Trends", "Favorite Evasive Skill", 148, PlayDataValueType.Int32, -1, int.MaxValue),
            new PlayDataField("Play Trends", "Favorite finisher", 152, PlayDataValueType.Int32, -1, int.MaxValue),
            new PlayDataField("Play Trends", "Highest combo", 106, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Play Trends", "Highest damage", 108, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Play Trends", "Ki sent to allies", 204, PlayDataValueType.Int32, 0, 65535),
            new PlayDataField("Play Trends", "Allies freed from mind control", 110, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Play Trends", "Wishes made to Shenron", 112, PlayDataValueType.UInt16, 0, 65535),

            new PlayDataField("Other", "Expert Missions cleared", 74, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Other", "Expert Mission enemies defeated", 104, PlayDataValueType.UInt16, 0, 65535)
            ,
            new PlayDataField("Online Record", "Player Matches", 82, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Online Record", "Player Wins", 88, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Online Record", "Player Losses", 90, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Online Record", "Ranked Matches", 84, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Online Record", "Ranked Wins", 92, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Online Record", "Ranked Losses", 94, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Online Record", "Endless Battles", 86, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Online Record", "Endless Wins", 96, PlayDataValueType.UInt16, 0, 65535),
            new PlayDataField("Online Record", "Endless Losses", 98, PlayDataValueType.UInt16, 0, 65535)
        };

        public static (int Battles, int Wins, int Losses, decimal WinRate) ReadOnlineSummary(byte[] data, int characterSlot)
        {
            int battles = ReadUInt16(data, characterSlot, 82) + ReadUInt16(data, characterSlot, 84) +
                ReadUInt16(data, characterSlot, 86);
            int wins = Read(data, characterSlot, OnlineWinsTotalField);
            int losses = ReadUInt16(data, characterSlot, 90) + ReadUInt16(data, characterSlot, 94) +
                ReadUInt16(data, characterSlot, 98);
            decimal rate = battles == 0 ? 0 : Math.Round((decimal)wins * 100M / battles, 2);
            return (battles, wins, losses, rate);
        }

        public static void SynchronizeOnlineWins(byte[] data, int characterSlot)
        {
            int wins = ReadUInt16(data, characterSlot, 88) + ReadUInt16(data, characterSlot, 92) +
                ReadUInt16(data, characterSlot, 96);
            Write(data, characterSlot, OnlineWinsTotalField, Math.Min(ushort.MaxValue, wins));
        }

        public static int RecordOffset(int characterSlot)
        {
            if (characterSlot < 0 || characterSlot >= CharacterReader.CharacterCount)
                throw new ArgumentOutOfRangeException(nameof(characterSlot));
            return CharacterReader.CharacterSectionOffset +
                CharacterReader.CharacterStride * characterSlot + OffsetWithinCharacter;
        }

        public static int Read(byte[] data, int characterSlot, PlayDataField field)
        {
            int offset = CheckedOffset(data, characterSlot, field);
            return field.ValueType switch
            {
                PlayDataValueType.Byte => data[offset],
                PlayDataValueType.UInt16 => BitConverter.ToUInt16(data, offset),
                _ => BitConverter.ToInt32(data, offset)
            };
        }

        public static void Write(byte[] data, int characterSlot, PlayDataField field, int value)
        {
            int offset = CheckedOffset(data, characterSlot, field);
            if (value < field.Minimum || value > field.Maximum)
                throw new ArgumentOutOfRangeException(nameof(value));
            switch (field.ValueType)
            {
                case PlayDataValueType.Byte:
                    data[offset] = (byte)value;
                    break;
                case PlayDataValueType.UInt16:
                    BitConverter.GetBytes((ushort)value).CopyTo(data, offset);
                    break;
                default:
                    BitConverter.GetBytes(value).CopyTo(data, offset);
                    break;
            }
        }

        public static int CountChangedFields(byte[] data, byte[]? snapshot)
        {
            if (snapshot == null || snapshot.Length != data.Length) return 0;
            int changed = 0;
            for (int slot = 0; slot < CharacterReader.CharacterCount; slot++)
            {
                foreach (PlayDataField field in Fields)
                    if (Read(data, slot, field) != Read(snapshot, slot, field)) changed++;
                if (Read(data, slot, OnlineWinsTotalField) != Read(snapshot, slot, OnlineWinsTotalField)) changed++;
            }
            return changed;
        }

        private static int ReadUInt16(byte[] data, int characterSlot, int relativeOffset) =>
            BitConverter.ToUInt16(data, RecordOffset(characterSlot) + relativeOffset);

        private static int CheckedOffset(byte[] data, int characterSlot, PlayDataField field)
        {
            int width = field.ValueType == PlayDataValueType.Byte ? 1 :
                field.ValueType == PlayDataValueType.UInt16 ? 2 : 4;
            int offset = RecordOffset(characterSlot) + field.RelativeOffset;
            if (field.RelativeOffset < 0 || field.RelativeOffset + width > RecordLength || offset + width > data.Length)
                throw new InvalidOperationException("The verified Play Data record is outside this save.");
            return offset;
        }
    }
}
