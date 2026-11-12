namespace WinUia.Models;

/// <summary>What a recorder is told about the launch it is recording.</summary>
public sealed class AppRecordingContext(string target, AppLaunchOptions options)
{
    /// <summary>The executable path or AppUserModelID being launched.</summary>
    public string Target { get; } = target;

    /// <summary>The launch options in effect, after the page object's defaults were applied.</summary>
    public AppLaunchOptions Options { get; } = options;
}

/// <summary>
/// Something that watches an app while it runs, such as a screen recording (the <c>WinUia.Recording</c> package).
/// Pass one as <see cref="AppLaunchOptions.Recording"/>: <see cref="Start"/> is called when the app is launched, after
/// the desktop is taken and before the process starts, and the result is disposed when the app is disposed, after it
/// has been closed and before the desktop is given back.
/// </summary>
public interface IAppRecorder
{
    /// <summary>Starts recording. Dispose the result to stop. Must not take the desktop lock: the app already holds it.</summary>
    IDisposable Start(AppRecordingContext context);
}
