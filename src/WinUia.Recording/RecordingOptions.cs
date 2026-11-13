using WinUia.Models;

namespace WinUia.Recording;

/// <summary>
/// Records an app's launch and everything it does until it is disposed. Pass it as
/// <see cref="AppLaunchOptions.Recording"/>, for example from the app's <c>DefaultOptions</c>: recording is on exactly
/// when it is configured, and every run is recorded (also locally, which needs ffmpeg installed).
/// Every video is written to <see cref="AllDirectory"/>; the video of a failed test (see <see cref="Failed"/>) is also
/// copied to <see cref="FailedDirectory"/>. What to keep or upload is up to the pipeline, which picks from those directories.
/// Set <see cref="FileName"/> and <see cref="Failed"/> from your test framework to name the video after the test and to
/// route the videos of failed tests.
/// </summary>
public sealed class RecordingOptions : IAppRecorder
{
    /// <summary>How the screen is captured. Defaults to a <see cref="FfmpegRecorder"/>.</summary>
    public IRecorder Recorder { get; init; } = new FfmpegRecorder();

    /// <summary>The video's file name, for example the running test's name. Null or unset: the app and the time.</summary>
    public Func<string?>? FileName { get; init; }

    /// <summary>
    /// Whether the test failed, asked when the app is disposed (so dispose it in the test's teardown). When true the
    /// video is also copied to <see cref="FailedDirectory"/>; it never deletes a video. Unset: nothing is copied.
    /// </summary>
    public Func<bool>? Failed { get; init; }

    /// <summary>
    /// The part of the virtual screen to record, in physical pixels. When null, the primary monitor: the app has no window
    /// yet when recording starts. See <see cref="ScreenArea.MonitorContaining"/>.
    /// </summary>
    public ScreenRect? Area { get; init; }

    /// <summary>Where every video is written. When null, <c>TestResults/Recordings/All</c> in the current directory.</summary>
    public string? AllDirectory { get; init; }

    /// <summary>Where the video of a failed test is copied to. When null, <c>TestResults/Recordings/Failed</c> in the current directory.</summary>
    public string? FailedDirectory { get; init; }

    IDisposable IAppRecorder.Start(AppRecordingContext context) => new ScreenRecorder(this).Start(context);
}
