using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace XV2SaveEditor;

public enum SavePlatform
{
    PC,
    PlayStation,
    PlayStationEncrypted,
    Xbox
}

public sealed record PlatformSaveData(SavePlatform Platform, byte[] DecryptedData, byte[] OriginalData);

public static class PlatformSaveAdapter
{
    private const int PlayStationSize = 1_221_120;
    private const int XboxSize = 1_221_088;

    public static PlatformSaveData Load(string path)
    {
        byte[] original = File.ReadAllBytes(path);
        if (original.Length == SaveOffsets.EncryptedSize)
            return new(SavePlatform.PC, Crypt.DecryptV30(original), original);

        // PS save revisions do not all place their duplicated #SAV markers at
        // the same offsets. The converter performs the authoritative header
        // validation; the platform container size is stable and unambiguous.
        if (original.Length == PlayStationSize && HasPlayStationMagic(original))
        {
            bool decrypted = HasSavMarker(original, 0x20) && HasSavMarker(original, 0xA0);
            byte[] psData = decrypted ? original : DecryptPlayStationGameLayer(original);
            return new(decrypted ? SavePlatform.PlayStation : SavePlatform.PlayStationEncrypted,
                ConvertPlayStationToEditor(psData), original);
        }

        if (original.Length == XboxSize)
            return new(SavePlatform.Xbox, ConvertXboxToEditor(original), original);

        throw new InvalidDataException($"Unsupported save format or size.\n\nFound: {original.Length:N0} bytes");
    }

    public static byte[] Encode(SavePlatform platform, byte[] decryptedData) => platform switch
    {
        SavePlatform.PC => Crypt.EncryptV30(decryptedData),
        SavePlatform.PlayStation => ConvertEditorToPlayStation(decryptedData),
        SavePlatform.PlayStationEncrypted => EncryptPlayStationGameLayer(ConvertEditorToPlayStation(decryptedData)),
        SavePlatform.Xbox => ConvertEditorToXbox(decryptedData),
        _ => throw new InvalidDataException("Unsupported save platform.")
    };

    public static string DisplayName(SavePlatform platform) => platform switch
    {
        SavePlatform.PlayStationEncrypted => "PlayStation (encrypted)",
        SavePlatform.PlayStation => "PlayStation (decrypted)",
        _ => platform.ToString()
    };

    private static bool HasSavMarker(byte[] data, int offset) =>
        data.Length >= offset + 4 && data[offset] == (byte)'#' && data[offset + 1] == (byte)'S' && data[offset + 2] == (byte)'A' && data[offset + 3] == (byte)'V';

    private static bool HasPlayStationMagic(byte[] data) => data.Length >= 4 &&
        data[0] == 0x48 && data[1] == 0x89 && data[2] == 0x01 && data[3] == 0x4C;

    private static byte[] DecryptPlayStationGameLayer(byte[] source)
    {
        byte[] data = (byte[])source.Clone();
        TransformCtr(data, 0x20, 0x80, Encoding.ASCII.GetBytes("PR]-<Q9*WxHsV8rcW!JuH7k_ug:T5ApX"), Encoding.ASCII.GetBytes("_Y7]mD1ziyH#Ar=0"));
        int keyOffset = (data[0x25] & 0x04) != 0 ? 0x4C : 0x1C;
        byte[] key = data.AsSpan(0x20 + keyOffset, 32).ToArray();
        byte[] iv = data.AsSpan(0x40 + keyOffset, 16).ToArray();
        TransformCtr(data, 0xA0, data.Length - 0xA0, key, iv);
        if (!HasSavMarker(data, 0x20) || !HasSavMarker(data, 0xA0))
            throw new InvalidDataException("The PlayStation game-encryption layer could not be validated.");
        return data;
    }

    private static byte[] EncryptPlayStationGameLayer(byte[] source)
    {
        byte[] data = (byte[])source.Clone();
        byte[] header = data.AsSpan(0x20, 0x80).ToArray();
        header[0x1A] = 0;
        for (int i = 5; i < data.Length / 0x20; i++) header[0x1A] += data[i * 0x20];
        Buffer.BlockCopy(header, 0, data, 0x20, header.Length);

        int keyOffset = (header[5] & 0x04) != 0 ? 0x4C : 0x1C;
        byte[] key = header.AsSpan(keyOffset, 32).ToArray();
        byte[] iv = header.AsSpan(keyOffset + 0x20, 16).ToArray();
        TransformCtr(data, 0xA0, data.Length - 0xA0, key, iv);

        header = data.AsSpan(0x20, 0x80).ToArray();
        header[0x15] = 0; for (int i = 0; i < 14; i++) header[0x15] += header[0x06 + i];
        header[0x16] = 0; for (int i = 0; i < 8; i++) header[0x16] += header[0x1C + i * 4];
        header[0x17] = 0; for (int i = 0; i < 8; i++) header[0x17] += header[0x4C + i * 4];
        header[0x18] = 0; for (int i = 0; i < 4; i++) header[0x18] += header[0x3C + i * 4];
        header[0x19] = 0; for (int i = 0; i < 4; i++) header[0x19] += header[0x6C + i * 4];
        header[0x1B] = 0; for (int i = 5; i < data.Length / 0x20; i++) header[0x1B] += data[i * 0x20];
        header[0x14] = header[0x05]; for (int i = 0; i < 7; i++) header[0x14] += header[0x15 + i];
        Buffer.BlockCopy(header, 0, data, 0x20, header.Length);

        TransformCtr(data, 0x20, 0x80, Encoding.ASCII.GetBytes("PR]-<Q9*WxHsV8rcW!JuH7k_ug:T5ApX"), Encoding.ASCII.GetBytes("_Y7]mD1ziyH#Ar=0"));
        MD5.HashData(data.AsSpan(0x20)).CopyTo(data.AsSpan(0x10, 16));
        return data;
    }

    private static void TransformCtr(byte[] data, int offset, int count, byte[] key, byte[] iv)
    {
        using Aes aes = Aes.Create();
        aes.Mode = CipherMode.ECB; aes.Padding = PaddingMode.None; aes.Key = key;
        using ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] counter = (byte[])iv.Clone();
        byte[] stream = new byte[16];
        for (int position = 0; position < count; position += 16)
        {
            encryptor.TransformBlock(counter, 0, 16, stream, 0);
            int block = Math.Min(16, count - position);
            for (int i = 0; i < block; i++) data[offset + position + i] ^= stream[i];
            for (int i = 15; i >= 0 && ++counter[i] == 0; i--) { }
        }
    }

    private static byte[] ConvertPlayStationToEditor(byte[] source) => InTemporaryDirectory(folder =>
    {
        string input = Path.Combine(folder, "SDATA000.DAT");
        File.WriteAllBytes(input, source);
        RunConverter(GetToolPath("xv2_converter.exe"), $"\"{input}\" auto", folder);
        return ReadExpected(Path.Combine(folder, "EditorReady.sav"), SaveOffsets.DecryptedSize);
    });

    private static byte[] ConvertEditorToPlayStation(byte[] source) => InTemporaryDirectory(folder =>
    {
        string input = Path.Combine(folder, "EditorReady.sav");
        File.WriteAllBytes(input, source);
        RunConverter(GetToolPath("xv2_converter.exe"), $"\"{input}\" auto", folder);
        return ReadExpected(Path.Combine(folder, "SDATA000.DAT"), PlayStationSize);
    });

    private static byte[] ConvertXboxToEditor(byte[] source) => InTemporaryDirectory(folder =>
    {
        string input = Path.Combine(folder, "input.dat");
        File.WriteAllBytes(input, source);
        RunConverter(GetToolPath("xv2_xbox_converter.exe"), $"\"{input}\"", folder, tolerateExitCode: true);
        string output = Directory.GetFiles(folder, "*.pc.sav.dec").SingleOrDefault()
            ?? throw new InvalidDataException("The Xbox converter did not produce PC-layout data.");
        return ReadExpected(output, SaveOffsets.DecryptedSize);
    });

    private static byte[] ConvertEditorToXbox(byte[] source) => InTemporaryDirectory(folder =>
    {
        string input = Path.Combine(folder, "editor.pc.sav.dec");
        File.WriteAllBytes(input, source);
        RunConverter(GetToolPath("xv2_xbox_converter.exe"), $"\"{input}\"", folder, tolerateExitCode: true);
        string output = Directory.GetFiles(folder, "*.dat").SingleOrDefault()
            ?? throw new InvalidDataException("The Xbox converter did not produce an encrypted save.");
        return ReadExpected(output, XboxSize);
    });

    private static T InTemporaryDirectory<T>(Func<string, T> action)
    {
        string folder = Path.Combine(Path.GetTempPath(), "XV2SaveEditor", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try { return action(folder); }
        finally { try { Directory.Delete(folder, true); } catch { } }
    }

    private static string GetToolPath(string name)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Tools", "PlatformConverters", name);
        if (!File.Exists(path)) throw new FileNotFoundException($"Required platform converter is missing: {name}", path);
        return path;
    }

    private static void RunConverter(string executable, string arguments, string workingDirectory, bool tolerateExitCode = false)
    {
        using Process process = Process.Start(new ProcessStartInfo(executable, arguments)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        }) ?? throw new InvalidOperationException("Could not start the platform converter.");
        process.StandardInput.Close();
        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(30_000)) { process.Kill(true); throw new TimeoutException("The platform converter timed out."); }
        string output = outputTask.GetAwaiter().GetResult();
        string error = errorTask.GetAwaiter().GetResult();
        if (process.ExitCode != 0 && !tolerateExitCode)
            throw new InvalidDataException($"Platform conversion failed.\n\n{output}\n{error}".Trim());
    }

    private static byte[] ReadExpected(string path, int size)
    {
        if (!File.Exists(path)) throw new InvalidDataException("The platform converter did not create its expected output.");
        byte[] data = File.ReadAllBytes(path);
        if (data.Length != size) throw new InvalidDataException($"Converted save has an unexpected size: {data.Length:N0} bytes.");
        return data;
    }
}
