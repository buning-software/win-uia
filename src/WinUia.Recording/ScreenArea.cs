using System.Runtime.InteropServices;

namespace WinUia.Recording;

/// <summary>Helpers to pick the part of the virtual screen to record.</summary>
public static class ScreenArea
{
    /// <summary>The bounds, in physical pixels, of the primary monitor.</summary>
    /// <exception cref="ScreenRecordingException">The monitor bounds could not be read.</exception>
    public static ScreenRect PrimaryMonitor() => MonitorContaining(new ScreenRect(0, 0, 1, 1));

    /// <summary>
    /// The bounds, in physical pixels, of the monitor that contains most of <paramref name="rect"/>
    /// (or the nearest monitor when it is off-screen), for use with <see cref="RecordingOptions.Area"/>.
    /// </summary>
    /// <exception cref="ScreenRecordingException">The monitor bounds could not be read.</exception>
    public static ScreenRect MonitorContaining(ScreenRect rect)
    {
        var previous = SetThreadDpiAwarenessContext(DpiAwarenessContextPerMonitorAwareV2);
        try
        {
            var native = new NativeRect { Left = rect.Left, Top = rect.Top, Right = rect.Right, Bottom = rect.Bottom };
            var monitor = MonitorFromRect(ref native, MonitorDefaultToNearest);
            var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
            if (monitor == nint.Zero || !GetMonitorInfo(monitor, ref info))
            {
                throw new ScreenRecordingException($"Could not determine the monitor that contains {rect}.");
            }

            return new ScreenRect(info.Monitor.Left, info.Monitor.Top, info.Monitor.Right, info.Monitor.Bottom);
        }
        finally
        {
            if (previous != nint.Zero)
            {
                SetThreadDpiAwarenessContext(previous);
            }
        }
    }

    private const uint MonitorDefaultToNearest = 2;
    private static readonly nint DpiAwarenessContextPerMonitorAwareV2 = -4;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;
        public NativeRect Monitor;
        public NativeRect Work;
        public uint Flags;
    }

    [DllImport("user32.dll")]
    private static extern nint MonitorFromRect(ref NativeRect rect, uint flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo info);

    [DllImport("user32.dll")]
    private static extern nint SetThreadDpiAwarenessContext(nint dpiContext);
}
