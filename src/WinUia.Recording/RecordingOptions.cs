using WinUia.Models;

namespace WinUia.Recording;

/// <summary>
/// Records an app's launch and everything it does until it is disposed. Pass it as
/// <see cref="AppLaunchOptions.Recording"/>, for example from the app's <c>DefaultOptions</c>.
/// With a <see cref="TestFramework"/> (for example <c>NUnitTestFramework</c> from WinUia.Recording.NUnit) the video is
/// named after the test, kept when the test failed, and attached to its result; without one every video is kept and
/// named after the time.
/// <para>
/// The <c>WINUIA_RECORDING</c> environment variable overrides <see cref="Mode"/>: <c>failed</c>, <c>all</c>, or
/// <c>off</c> to record nothing.
/// </para>
/// </summary>
public sealed class RecordingOptions : IAppRecorder
{
    /// <summary>How the screen is captured. Defaults to a <see cref="FfmpegRecorder"/>.</summary>
    public IRecorder Recorder { get; init; } = new FfmpegRecorder();

    /// <summary>What is known about the running test. Null: every recording is kept and named after the time.</summary>
    public ITestFramework? TestFramework { get; init; }

    /// <summary>Whether to record and which recordings are kept. Defaults to <see cref="RecordingMode.Failed"/>.</summary>
    public RecordingMode Mode { get; init; }

    /// <summary>
    /// The part of the virtual screen to record, in physical pixels. When null, the primary monitor: the app has no window
    /// yet when recording starts. See <see cref="ScreenArea.MonitorContaining"/>.
    /// </summary>
    public ScreenRect? Area { get; init; }

    /// <summary>
    /// Where recordings are written. When null, the <c>WINUIA_RECORDING_DIR</c> environment variable, then
    /// <c>TestResults/Recordings</c> in the current directory.
    /// </summary>
    public string? OutputDirectory { get; init; }

    IDisposable IAppRecorder.Start(AppRecordingContext context) =>
        new ScreenRecorder(this, Environment.GetEnvironmentVariable).Start(context);
}
