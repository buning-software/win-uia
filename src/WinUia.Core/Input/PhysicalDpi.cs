using WinUia.Core.Interop;

namespace WinUia;

internal static class PhysicalDpi
{
    internal static void MakeProcessAware() =>
        _ = User32.SetProcessDpiAwarenessContext(User32.DpiAwarenessContextPerMonitorAwareV2);

    internal static T Run<T>(Func<T> action)
    {
        var previous = User32.SetThreadDpiAwarenessContext(User32.DpiAwarenessContextPerMonitorAwareV2);
        try
        {
            return action();
        }
        finally
        {
            if (previous != 0)
                User32.SetThreadDpiAwarenessContext(previous);
        }
    }

    internal static void Run(Action action) => Run(() => { action(); return true; });
}
