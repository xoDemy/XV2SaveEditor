namespace XV2SaveEditor;

public static class SteamOwnership
{
    public const ulong SteamId64Base = 76561197960265728UL;
    private const uint VersionXor = 0x65636F6E;
    private const int Offset = 0x08;

    public static ulong ReadSteamId64(byte[] decryptedData) =>
        SteamId64Base + (BitConverter.ToUInt32(decryptedData, Offset) ^ VersionXor);

    public static void WriteSteamId64(byte[] decryptedData, ulong input)
    {
        ulong steamId64 = Normalize(input);
        uint accountId = checked((uint)(steamId64 - SteamId64Base));
        BitConverter.GetBytes(accountId ^ VersionXor).CopyTo(decryptedData, Offset);
    }

    public static ulong Normalize(ulong input)
    {
        ulong result = input <= uint.MaxValue ? SteamId64Base + input : input;
        if (result < SteamId64Base || result - SteamId64Base > uint.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(input), "Enter a valid SteamID64 or 32-bit Steam account ID.");
        return result;
    }
}
