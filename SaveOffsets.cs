namespace XV2SaveEditor
{
    public static class SaveOffsets
    {
        public const int Zeni = 0x2C;
        public const int TPMedals = 0x30;

        // Verified XV2 v30 skill ownership tables (1,024 x 8-byte records).
        public const int SuperSkills = 0xB430;
        public const int UltimateSkills = 0xD430;
        public const int EvasiveSkills = 0xF430;
        public const int AwokenSkills = 0x15430;
        public const int SkillRecordSize = 8;
        public const int SkillRecordCount = 1024;

        public const int EncryptedSize = 1221280;
        public const int DecryptedSize = 1221112;
    }
}
