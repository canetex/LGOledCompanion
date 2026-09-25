// NativeIdle.cs

using System.Runtime.InteropServices;

namespace LGOledCompanion;

internal static class NativeIdle
{
    private const int SystemExecutionState = 16;
    private const int EsDisplayRequired = 0x00000002;

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint Size;
        public uint Time;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LastInputInfo info);

    [DllImport("ntdll.dll")]
    private static extern int NtPowerInformation(
        int informationLevel,
        IntPtr inputBuffer,
        int inputBufferLength,
        out int outputBuffer,
        int outputBufferLength);

    public static TimeSpan GetIdleTime()
    {
        var info = new LastInputInfo { Size = (uint)Marshal.SizeOf<LastInputInfo>() };
        if (!GetLastInputInfo(ref info))
        {
            return TimeSpan.Zero;
        }

        var idle_ms = unchecked((uint)Environment.TickCount - info.Time);
        return TimeSpan.FromMilliseconds(idle_ms);
    }

    public static bool IsDisplayRequired()
    {
        var status = NtPowerInformation(SystemExecutionState, IntPtr.Zero, 0, out var state, sizeof(int));
        return status == 0 && (state & EsDisplayRequired) != 0;
    }
}
