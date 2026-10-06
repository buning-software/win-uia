using System.ComponentModel;
using System.Diagnostics;
using WinUia.Models;

namespace WinUia;

internal static class AppLauncher
{
    public static Process LaunchExe(string path, AppLaunchOptions? options = null)
    {
        var startInfo = new ProcessStartInfo(path, options?.Arguments ?? "") { UseShellExecute = false };
        if (options?.WorkingDirectory is { } workingDirectory)
            startInfo.WorkingDirectory = workingDirectory;
        foreach (var (name, value) in options?.Environment ?? new Dictionary<string, string?>())
        {
            if (value is null)
                startInfo.Environment.Remove(name);
            else
                startInfo.Environment[name] = value;
        }

        try
        {
            return Process.Start(startInfo) ?? throw new AppProcessException($"Could not start '{path}'.");
        }
        catch (Win32Exception ex)
        {
            throw new AppProcessException($"Could not start '{path}': {ex.Message}", ex);
        }
    }

    public static Process LaunchPackaged(string appUserModelId, AppLaunchOptions? options = null)
    {
        EnsurePackagedOptions(options);
        return Attach(PackagedAppActivator.Activate(appUserModelId, options?.Arguments));
    }

    private static void EnsurePackagedOptions(AppLaunchOptions? options)
    {
        if (options?.WorkingDirectory is not null || options?.Environment is not null)
            throw new ArgumentException(
                "A packaged app starts with the working directory and environment Windows gives it; " +
                "WorkingDirectory and Environment apply to executables only.", nameof(options));
    }

    public static Process Attach(int processId)
    {
        try
        {
            return Process.GetProcessById(processId);
        }
        catch (ArgumentException ex)
        {
            throw new AppProcessException($"No process with id {processId} is running.", ex);
        }
    }
}
