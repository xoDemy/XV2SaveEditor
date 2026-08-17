using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace XV2LivePrototype;

internal readonly record struct PointerCandidate(nint Address, nint Target, int Offset, int Level);

internal sealed class GameProcessSession : IDisposable
{
    private const uint ProcessVmRead = 0x0010;
    private const uint ProcessQueryLimitedInformation = 0x1000;
    private IntPtr handle;

    public Process Process { get; }
    public bool IsAttached => handle != IntPtr.Zero && !Process.HasExited;
    public IntPtr ModuleBase => Process.MainModule?.BaseAddress ?? IntPtr.Zero;
    public int ModuleSize => Process.MainModule?.ModuleMemorySize ?? 0;
    public string ExecutablePath => Process.MainModule?.FileName ?? "Unavailable";
    public string Version => Process.MainModule?.FileVersionInfo.FileVersion ?? "Unknown";

    private GameProcessSession(Process process, IntPtr processHandle)
    {
        Process = process;
        handle = processHandle;
    }

    public static GameProcessSession Attach(Process process)
    {
        IntPtr handle = OpenProcess(ProcessQueryLimitedInformation | ProcessVmRead, false, process.Id);
        if (handle == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not open DBXV2.exe with read-only access.");
        return new GameProcessSession(process, handle);
    }

    public byte[] ReadBytes(IntPtr address, int count)
    {
        if (!IsAttached) throw new InvalidOperationException("The game is not attached.");
        byte[] buffer = new byte[count];
        if (!ReadProcessMemory(handle, address, buffer, count, out nuint read) || read != (nuint)count)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not read the requested game memory.");
        return buffer;
    }

    public IReadOnlyList<nint> ScanInt32(int value, int maximumResults = 250000)
    {
        if (!IsAttached) throw new InvalidOperationException("The game is not attached.");
        List<nint> results = new();
        nint address = 0;
        nint maximum = IntPtr.Size == 8 ? unchecked((nint)0x00007FFFFFFEFFFFL) : (nint)0x7FFEFFFF;
        byte[] target = BitConverter.GetBytes(value);
        while (address < maximum && results.Count < maximumResults)
        {
            nuint queried = VirtualQueryEx(handle, address, out MemoryBasicInformation info, (nuint)Marshal.SizeOf<MemoryBasicInformation>());
            if (queried == 0 || info.RegionSize == 0) break;
            nint next = info.BaseAddress + checked((nint)info.RegionSize);
            if (info.State == 0x1000 && IsReadable(info.Protect) && (info.Protect & 0x100) == 0)
                ScanRegion(info.BaseAddress, info.RegionSize, target, results, maximumResults, 4);
            if (next <= address) break;
            address = next;
        }
        return results;
    }

    public IReadOnlyList<nint> ScanPointer(nint pointedAddress, int maximumResults = 100000)
    {
        if (!IsAttached) throw new InvalidOperationException("The game is not attached.");
        List<nint> results = new();
        nint address = 0;
        nint maximum = IntPtr.Size == 8 ? unchecked((nint)0x00007FFFFFFEFFFFL) : (nint)0x7FFEFFFF;
        byte[] target = IntPtr.Size == 8 ? BitConverter.GetBytes((long)pointedAddress) : BitConverter.GetBytes((int)pointedAddress);
        while (address < maximum && results.Count < maximumResults)
        {
            nuint queried = VirtualQueryEx(handle, address, out MemoryBasicInformation info, (nuint)Marshal.SizeOf<MemoryBasicInformation>());
            if (queried == 0 || info.RegionSize == 0) break;
            nint next = info.BaseAddress + checked((nint)info.RegionSize);
            if (info.State == 0x1000 && IsReadable(info.Protect) && (info.Protect & 0x100) == 0)
                ScanRegion(info.BaseAddress, info.RegionSize, target, results, maximumResults, IntPtr.Size);
            if (next <= address) break;
            address = next;
        }
        return results;
    }

    public IReadOnlyList<PointerCandidate> ScanPointerPaths(nint pointedAddress, int maximumOffset = 0x4000, int maximumLevels = 3, int maximumResults = 100000)
    {
        if (!IsAttached) throw new InvalidOperationException("The game is not attached.");
        List<PointerCandidate> results = new();
        HashSet<nint> targets = new() { pointedAddress };
        HashSet<nint> seenAddresses = new();
        for (int level = 1; level <= maximumLevels && targets.Count > 0 && results.Count < maximumResults; level++)
        {
            int levelStart = results.Count;
            long[] sortedTargets = targets.Select(value => (long)value).OrderBy(value => value).ToArray();
            HashSet<nint> nextTargets = new();
            nint address = 0;
            nint maximum = unchecked((nint)0x00007FFFFFFEFFFFL);
            while (address < maximum && results.Count < maximumResults)
            {
                nuint queried = VirtualQueryEx(handle, address, out MemoryBasicInformation info, (nuint)Marshal.SizeOf<MemoryBasicInformation>());
                if (queried == 0 || info.RegionSize == 0) break;
                nint next = info.BaseAddress + checked((nint)info.RegionSize);
                if (info.State == 0x1000 && IsReadable(info.Protect) && (info.Protect & 0x100) == 0)
                    ScanPointerRegion(info.BaseAddress, info.RegionSize, sortedTargets, maximumOffset, level, results, nextTargets, seenAddresses, maximumResults);
                if (next <= address) break;
                address = next;
            }
            if (results.Skip(levelStart).Any(candidate => candidate.Address >= ModuleBase && candidate.Address < ModuleBase + ModuleSize))
                break;
            targets = nextTargets;
        }
        return results;
    }

    private void ScanPointerRegion(nint start, nuint regionSize, long[] targets, int maximumOffset, int level,
        List<PointerCandidate> results, HashSet<nint> nextTargets, HashSet<nint> seenAddresses, int maximumResults)
    {
        const int chunkSize = 1024 * 1024;
        ulong remaining = regionSize;
        nint cursor = start;
        while (remaining >= 8 && results.Count < maximumResults)
        {
            int requested = (int)Math.Min((ulong)chunkSize, remaining);
            requested -= requested % 8;
            if (requested == 0) break;
            byte[] chunk = new byte[requested];
            if (!ReadProcessMemory(handle, cursor, chunk, requested, out nuint read) || read < 8) break;
            int actual = (int)read - (int)read % 8;
            for (int i = 0; i < actual && results.Count < maximumResults; i += 8)
            {
                long pointer = BitConverter.ToInt64(chunk, i);
                if (pointer <= 0) continue;
                int index = Array.BinarySearch(targets, pointer);
                if (index < 0) index = ~index;
                if (index >= targets.Length) continue;
                long offset = targets[index] - pointer;
                if (offset < 0 || offset > maximumOffset) continue;
                nint pointerAddress = cursor + i;
                if (!seenAddresses.Add(pointerAddress)) continue;
                results.Add(new PointerCandidate(pointerAddress, (nint)targets[index], (int)offset, level));
                nextTargets.Add(pointerAddress);
            }
            cursor += actual;
            remaining -= (uint)actual;
            if (actual < requested) break;
        }
    }

    public IReadOnlyList<nint> RefineInt32(IEnumerable<nint> candidates, int value)
    {
        byte[] expected = BitConverter.GetBytes(value);
        List<nint> results = new();
        foreach (nint candidate in candidates)
        {
            try { if (ReadBytes(candidate, 4).SequenceEqual(expected)) results.Add(candidate); }
            catch (Win32Exception) { }
        }
        return results;
    }

    private void ScanRegion(nint start, nuint regionSize, byte[] target, List<nint> results, int maximumResults, int alignment)
    {
        const int chunkSize = 1024 * 1024;
        ulong remaining = regionSize;
        nint cursor = start;
        byte[] overlap = Array.Empty<byte>();
        while (remaining > 0 && results.Count < maximumResults)
        {
            int requested = (int)Math.Min((ulong)chunkSize, remaining);
            byte[] chunk = new byte[requested];
            if (!ReadProcessMemory(handle, cursor, chunk, requested, out nuint read) || read == 0) break;
            int actual = (int)read;
            byte[] searchable = overlap.Length == 0 ? chunk.AsSpan(0, actual).ToArray() : overlap.Concat(chunk.AsSpan(0, actual).ToArray()).ToArray();
            for (int i = 0; i <= searchable.Length - target.Length && results.Count < maximumResults; i += alignment)
                if (searchable.AsSpan(i, target.Length).SequenceEqual(target)) results.Add(cursor - overlap.Length + i);
            int overlapSize = target.Length - 1;
            overlap = searchable.Length >= overlapSize ? searchable[^overlapSize..] : searchable;
            cursor += actual; remaining -= (uint)actual;
            if (actual < requested) break;
        }
    }

    private static bool IsReadable(uint protection)
    {
        uint basic = protection & 0xFF;
        return basic is 0x02 or 0x04 or 0x20 or 0x40 or 0x80;
    }

    public void Dispose()
    {
        if (handle != IntPtr.Zero) CloseHandle(handle);
        handle = IntPtr.Zero;
        Process.Dispose();
    }

    [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr OpenProcess(uint access, bool inherit, int processId);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool ReadProcessMemory(IntPtr process, IntPtr address, [Out] byte[] buffer, int size, out nuint read);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern nuint VirtualQueryEx(IntPtr process, nint address, out MemoryBasicInformation information, nuint length);
    [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr handle);

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryBasicInformation
    {
        public nint BaseAddress;
        public nint AllocationBase;
        public uint AllocationProtect;
        public ushort PartitionId;
        public nuint RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }
}
