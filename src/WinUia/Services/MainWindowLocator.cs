using System.Diagnostics;

namespace WinUia;

internal static class MainWindowLocator
{
    public static Element Find(AutomationContext context, Process process, TimeSpan? timeout = null)
    {
        var processId = process.Id;
        var root = context.GetRootElement();

        return context.WaitForOrThrow(() =>
        {
            if (process.HasExited)
                throw new AppProcessException($"Process {processId} exited with code {process.ExitCode} before its main window appeared.");

            var own = root.TryFind(e => e.ProcessId == processId && e.ControlType == ControlType.Window, TreeScope.Children, TimeSpan.Zero);
            if (own is not null)
                return own;

            // UWP apps show their window inside ApplicationFrameHost's frame.
            foreach (var frame in root.FindAll(e => e.ClassName == "ApplicationFrameWindow", TreeScope.Children))
            {
                if (frame.TryFind(e => e.ProcessId == processId, TreeScope.Children, TimeSpan.Zero) is not null)
                    return frame;
            }

            process.Refresh();
            return process.MainWindowHandle != 0 ? context.FromHandle(process.MainWindowHandle) : null;
        }, () => $"main window of process {processId}", timeout);
    }
}
