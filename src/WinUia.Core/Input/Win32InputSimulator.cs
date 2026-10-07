using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WinUia.Core.Interop;
using static WinUia.Core.Interop.User32;

namespace WinUia.Input;

/// <summary>
/// Physical mouse and keyboard input through <c>SendInput</c>. Coordinates are physical screen pixels.
/// Physical input needs an interactive desktop, goes to whatever window is under the cursor or focused, and is
/// silently dropped by Windows while the foreground window belongs to a process with higher rights (elevated, or a
/// system service), unless this process has them too (UIPI).
/// </summary>
public sealed class Win32InputSimulator : IInputSimulator
{
    /// <summary>
    /// Pause between the positions of a smooth move: smooth to the eye and in recordings. Default 10 ms (in practice
    /// Windows' timer makes it about 15 ms).
    /// </summary>
    public TimeSpan MoveStepInterval { get; init; } = TimeSpan.FromMilliseconds(10);

    /// <summary>How often a move is sent before <see cref="MoveTo(int, int)"/> gives up. Default 3.</summary>
    public int MoveAttempts { get; init; } = 3;

    /// <summary>How long each attempt waits for the cursor to arrive. Default 100 ms.</summary>
    public TimeSpan MoveArrivalTimeout { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Moves the cursor to (<paramref name="x"/>, <paramref name="y"/>) and makes sure it got there: a click after a
    /// dropped move would land wherever the cursor happens to be, so the move is checked and sent again. Throws
    /// <see cref="InvalidOperationException"/> when the cursor still is not there, naming the foreground window's
    /// process, whose rights are the usual reason (see <see cref="Win32InputSimulator"/>).
    /// </summary>
    public void MoveTo(int x, int y)
    {
        for (var attempt = 1; ; attempt++)
        {
            SendMove(x, y);
            if (CursorArrives(x, y))
                return;

            if (attempt >= MoveAttempts)
            {
                throw new InvalidOperationException(
                    $"The cursor could not be moved to ({x}, {y}); it is at {GetCursorPosition()}. Windows drops " +
                    $"simulated input while the foreground window belongs to an elevated process or a system service; " +
                    $"the foreground window now belongs to '{ForegroundProcessName()}'. Otherwise the point may be off " +
                    "every screen, or someone is moving the mouse at the same time.");
            }
        }
    }

    /// <summary>
    /// Moves the cursor from where it is to (<paramref name="x"/>, <paramref name="y"/>) over
    /// <paramref name="duration"/>, along a straight line that starts and ends slowly, so the movement can be followed
    /// on screen. Ends exactly on the target; a zero duration, or a cursor already there, moves at once.
    /// </summary>
    public void MoveTo(int x, int y, TimeSpan duration)
    {
        var (startX, startY) = GetCursorPosition();
        if (duration > TimeSpan.Zero && (startX, startY) != (x, y))
        {
            // Follow the clock, not a step count: Thread.Sleep is coarse (about 15 ms), so counting steps would overrun the duration.
            var elapsed = Stopwatch.StartNew();
            for (var t = 0.0; t < 1; t = elapsed.Elapsed / duration)
            {
                var progress = EaseInOut(t);
                SendMove(startX + (int)Math.Round((x - startX) * progress), startY + (int)Math.Round((y - startY) * progress));
                Thread.Sleep(MoveStepInterval);
            }
        }

        MoveTo(x, y);
    }

    /// <inheritdoc />
    public (int X, int Y) GetCursorPosition() => PhysicalDpi.Run(() =>
    {
        if (!GetCursorPos(out var point))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "GetCursorPos failed; is the desktop locked?");
        return (point.x, point.y);
    });

    /// <inheritdoc />
    public void ClickAt(int x, int y, MouseButton button = MouseButton.Left)
    {
        MoveTo(x, y);
        Thread.Sleep(20);

        var (down, up) = button == MouseButton.Left
            ? (MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP)
            : (MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP);
        PhysicalDpi.Run(() => Send([MouseInput(down, 0, 0, absolute: false), MouseInput(up, 0, 0, absolute: false)]));
    }

    /// <summary>Types <paramref name="text"/> into the focused control as Unicode characters; "\r\n", "\r" and "\n" are Enter.</summary>
    public void SendText(string text)
    {
        var inputs = new List<INPUT>(text.Length * 2);
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                continue;

            if (ch is '\r' or '\n')
            {
                inputs.Add(KeyInput((ushort)VirtualKey.Enter, 0, 0));
                inputs.Add(KeyInput((ushort)VirtualKey.Enter, 0, KEYEVENTF_KEYUP));
                continue;
            }

            inputs.Add(KeyInput(0, ch, KEYEVENTF_UNICODE));
            inputs.Add(KeyInput(0, ch, KEYEVENTF_UNICODE | KEYEVENTF_KEYUP));
        }

        if (inputs.Count > 0)
            Send([.. inputs]);
    }

    /// <summary>
    /// Presses the keys in order and releases them in reverse order: one key (<c>SendKeys(VirtualKey.Enter)</c>) or
    /// a combination (<c>SendKeys(VirtualKey.Control, VirtualKey.A)</c>).
    /// </summary>
    public void SendKeys(params VirtualKey[] keys)
    {
        var inputs = new List<INPUT>(keys.Length * 2);
        inputs.AddRange(keys.Select(k => KeyInput((ushort)k, 0, 0)));
        inputs.AddRange(keys.Reverse().Select(k => KeyInput((ushort)k, 0, KEYEVENTF_KEYUP)));
        Send([.. inputs]);
    }

    // SendInput queues the move; the cursor follows once Windows has processed it, usually at once.
    private bool CursorArrives(int x, int y)
    {
        var waited = Stopwatch.StartNew();
        while (GetCursorPosition() != (x, y))
        {
            if (waited.Elapsed >= MoveArrivalTimeout)
                return false;
            Thread.Sleep(5);
        }

        return true;
    }

    private static string ForegroundProcessName()
    {
        try
        {
            GetWindowThreadProcessId(GetForegroundWindow(), out var processId);
            using var process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return "unknown"; // No foreground window, or its process just exited.
        }
    }

    private static void SendMove(int x, int y) => PhysicalDpi.Run(() =>
        Send([MouseInput(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK, x, y)]));

    private static double EaseInOut(double t) => t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;

    private static INPUT MouseInput(uint flags, int x, int y, bool absolute = true)
    {
        var mi = new MOUSEINPUT { dwFlags = flags };
        if (absolute)
        {
            // Absolute coordinates are normalised to 0..65535 over the virtual desktop; rounding up lands on exactly the requested pixel.
            var left = GetSystemMetrics(SM_XVIRTUALSCREEN);
            var top = GetSystemMetrics(SM_YVIRTUALSCREEN);
            var width = Math.Max(GetSystemMetrics(SM_CXVIRTUALSCREEN), 1);
            var height = Math.Max(GetSystemMetrics(SM_CYVIRTUALSCREEN), 1);
            mi.dx = Normalise(x - left, width);
            mi.dy = Normalise(y - top, height);
        }

        return new INPUT { type = INPUT_MOUSE, u = new InputUnion { mi = mi } };
    }

    private static int Normalise(int offset, int size) =>
        Math.Clamp((int)Math.Ceiling(offset * 65536.0 / size), 0, 65535);

    private static INPUT KeyInput(ushort vk, ushort scan, uint flags) =>
        new() { type = INPUT_KEYBOARD, u = new InputUnion { ki = new KEYBDINPUT { wVk = vk, wScan = scan, dwFlags = flags } } };

    private static void Send(INPUT[] inputs)
    {
        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        if (sent != inputs.Length)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "SendInput was blocked; is the desktop locked or the target elevated?");
    }
}
