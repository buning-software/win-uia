using System.Diagnostics;

namespace WinUia;

internal static class AppCloser
{
    public static void Close(AutomationContext context, Process process, TimeSpan? timeout = null)
    {
        if (process.HasExited)
            return;

        var effectiveTimeout = timeout ?? context.DefaultTimeout;
        try
        {
            MainWindowLocator.Find(context, process, TimeSpan.Zero).WindowPattern.Close();
        }
        catch (Exception ex) when (ex is UiaException or AppProcessException)
        {
            // No window, no Window pattern, or the process just exited: fall through to waiting and killing.
        }

        if (process.WaitForExit(effectiveTimeout))
            return;

        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            // Exited between the wait and the kill.
        }

        process.WaitForExit(effectiveTimeout);
    }
}
