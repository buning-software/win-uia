using WinUia.Models;

namespace WinUia.Recording;

/// <summary>
/// Records an app's launch and everything it does until it is disposed. Pass it as
/// <see cref="AppLaunchOptions.Recording"/>, for example from the app's <c>DefaultOptions</c>.
/// Set <see cref="Name"/> and <see cref="Keep"/> from your test framework to name the video after the test and keep only
/// the videos of failed tests; without them every video is kept and named after the app and the time.
/// <para>
/// The <c>WINUIA_RECORDING</c> environment variable overrides <see cref="Mode"/>: <c>failed</c>, <c>all</c>, or
/// <c>off</c> to record nothing.
/// </para>
/// </summary>
public sealed class RecordingOptions : IAppRecorder
{
    /// <summary>How the screen is captured. Defaults to a <see cref="FfmpegRecorder"/>.</summary>
    public IRecorder Recorder { get; init; } = new FfmpegRecorder();

    /// <summary>The video's file name, for example the running test's name. Null or unset: the app and the time.</summary>
    public Func<string?>? Name { get; init; }

    /// <summary>
    /// Whether to keep the video, asked when the app is disposed (so dispose it in the test's teardown), for example
    /// "the test failed". Unset: every video is kept. Ignored when <see cref="Mode"/> is <see cref="RecordingMode.All"/>.
    /// </summary>
    public Func<bool>? Keep { get; init; }

    /// <summary>
    /// Called with the video's path once the app is disposed and the video is kept, for example to attach it to the test
    /// result. An exception thrown here is logged, not rethrown.
    /// </summary>
    public Action<string>? OnKept { get; init; }

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
