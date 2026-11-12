using System.Reflection;

namespace WinUia.TestFrameworks.Common;

/// <summary>What the WinUia.TestFrameworks.* test projects share about the app they drive, whichever test framework runs them.</summary>
public static class ExampleApp
{
    /// <summary>The WinForms example app, built before the test project that references this one.</summary>
    public static string Path { get; } = typeof(ExampleApp).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(a => a.Key == "ApplicationPath").Value!;

    public const string MainWindowName = "WinUia Test App";

    public const string ClickButtonId = "btnClick";

    public const string ResultLabelId = "lblResult";

    /// <summary>The text of the result label when the app starts.</summary>
    public const string ReadyText = "Ready";

    /// <summary>The text of the result label after <see cref="ClickButtonId"/> is clicked.</summary>
    public const string ClickedText = "Clicked";

    /// <summary>
    /// Whether another thread holds the desktop. The mutex behind <see cref="DesktopLock"/> belongs to the thread that
    /// took it, so the calling thread cannot take it while a lock is held.
    /// </summary>
    public static bool DesktopIsHeld()
    {
        using var mutex = new Mutex(false, @"Local\WinUia.Desktop");
        if (!mutex.WaitOne(0))
            return true;

        mutex.ReleaseMutex();
        return false;
    }
}
