using System;

namespace XV2SaveEditor
{
    /// <summary>Mirrors LazyBones SAV_File.ValidateLevelFlags for XV2 v30 saves.</summary>
    public static class LevelCapFlagValidator
    {
        private const int AccountFlagsOffset = 0x4C;

        public static void Apply(byte[] data) => Apply(data, Enumerable.Range(0, CharacterReader.CharacterCount));

        public static void Apply(byte[] data, IEnumerable<int> characterSlots)
        {
            if (data.Length < AccountFlagsOffset + sizeof(uint))
                throw new ArgumentException("Account flag section is outside this save.", nameof(data));

            uint accountFlags = BitConverter.ToUInt32(data, AccountFlagsOffset);
            int highestLevel = 0;
            HashSet<int> targets = characterSlots.Distinct().ToHashSet();
            if (targets.Any(slot => slot is < 0 or >= CharacterReader.CharacterCount)) throw new ArgumentOutOfRangeException(nameof(characterSlots));

            for (int slot = 0; slot < CharacterReader.CharacterCount; slot++)
            {
                int levelOffset = CharacterReader.CharacterSectionOffset +
                    CharacterReader.CharacterStride * slot + 172;
                int level = BitConverter.ToInt32(data, levelOffset);
                highestLevel = Math.Max(highestLevel, level);
                if (!targets.Contains(slot)) continue;

                // Verified LazyBones per-CaC level-cap system flags.
                if (level > 80) { SystemFlagAccess.Write(data, slot, 17, true); SystemFlagAccess.Write(data, slot, 291, true); }
                if (level > 85) { SystemFlagAccess.Write(data, slot, 18, true); SystemFlagAccess.Write(data, slot, 292, true); }
                if (level > 90) { SystemFlagAccess.Write(data, slot, 19, true); SystemFlagAccess.Write(data, slot, 293, true); }
                if (level > 95) { SystemFlagAccess.Write(data, slot, 20, true); SystemFlagAccess.Write(data, slot, 294, true); }
                if (level > 99) { SystemFlagAccess.Write(data, slot, 7316, true); SystemFlagAccess.Write(data, slot, 5227, true); }
                // Verified against a legitimate 179/180 save: every high-level
                // CaC has 7317 set while its level-69 CaC does not.
                if (level > 120) SystemFlagAccess.Write(data, slot, 7317, true);
                // Verified by controlled in-game transitions on the user's
                // save. Giving Whis Sushi (140 -> 160) sets both flags.
                if (level > 140)
                {
                    SystemFlagAccess.Write(data, slot, 5213, true);
                    SystemFlagAccess.Write(data, slot, 5231, true);
                }
                // Giving Whis the Octopus Ball (160 -> 180) sets these three.
                if (level > 160)
                {
                    SystemFlagAccess.Write(data, slot, 5233, true);
                    SystemFlagAccess.Write(data, slot, 5234, true);
                    SystemFlagAccess.Write(data, slot, 7342, true);
                }
                // Verified by comparing the pre-199 level-180 save with the
                // current level-199 save: all level-199 CaCs set these three.
                if (level > 180)
                {
                    // Verified by a controlled completion of Whis' final
                    // 180 -> 199 training challenge on the user's save.
                    SystemFlagAccess.Write(data, slot, 5235, true);
                    SystemFlagAccess.Write(data, slot, 7344, true);
                    SystemFlagAccess.Write(data, slot, 7345, true);
                    SystemFlagAccess.Write(data, slot, 7346, true);
                    SystemFlagAccess.Write(data, slot, 7347, true);
                }
            }

            if (highestLevel >= 199)
            {
                // Exact clean flag template from the user's known-good level-199
                // imported CaC. Apply only these cap flags, never unrelated flags.
                int[] capTemplate =
                {
                    17, 18, 19, 20, 291, 292, 293, 294,
                    5213, 5227, 5231, 5233, 5234, 5235,
                    7316, 7317, 7342, 7344, 7345, 7346, 7347
                };
                foreach (int slot in targets)
                {
                    int nameOffset = CharacterReader.CharacterSectionOffset + CharacterReader.CharacterStride * slot + 68;
                    if (data[nameOffset] == 0) continue;
                    foreach (int flag in capTemplate) SystemFlagAccess.Write(data, slot, flag, true);
                }
            }

            // Verified LazyBones account-wide level-cap bits.
            if (highestLevel > 80) accountFlags |= 0x40;
            if (highestLevel > 85) accountFlags |= 0x80;
            if (highestLevel > 90) accountFlags |= 0x100;
            if (highestLevel > 95) accountFlags |= 0x200;
            if (highestLevel > 99) accountFlags |= 0x40000000;

            // Verified Whis level-cap sequence. The user's live saves gained
            // 0x00020000, 0x00040000, and 0x00080000 as the game advanced
            // through the 120/140/160 gates. Preserved level-180 saves also
            // contain 0x00100000. These four consecutive account bits match
            // the four Whis food unlocks exactly.
            if (highestLevel > 99) accountFlags |= 0x00020000;
            if (highestLevel > 120) accountFlags |= 0x00040000;
            if (highestLevel > 140) accountFlags |= 0x00080000;
            if (highestLevel > 160) accountFlags |= 0x00100000;

            // Verified by the current web editor and the user's level-199
            // reference: bit 31 is the final 180 -> 199 Whis challenge gate.
            if (highestLevel > 180) accountFlags |= 0x80000000;

            Array.Copy(BitConverter.GetBytes(accountFlags), 0, data, AccountFlagsOffset, sizeof(uint));
        }
    }
}
