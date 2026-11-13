using WinUia.Models;
using WinUia.Recording;

namespace WinUia.TestFrameworks.Common;

/// <summary>
/// The setup every framework's recording tests need: directories of their own for the videos. Dispose deletes them.
/// The test framework supplies only what is its own: how the running test is named, and whether it failed.
/// </summary>
public sealed class RecordingScenario(Func<string?> name, Func<bool> failed) : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}");

    private string AllDirectory => Path.Combine(_directory, "All");

    private string FailedDirectory => Path.Combine(_directory, "Failed");

    /// <summary>Why ffmpeg cannot be used on this machine, or null. These tests need it, so a reason is a failure.</summary>
    public string? UnavailableReason => new FfmpegRecorder().UnavailableReason;

    /// <summary>The message a test fails with when <see cref="UnavailableReason"/> is set.</summary>
    public const string FfmpegRequired = "ffmpeg is not installed, so recording cannot be validated. Install ffmpeg and add it to PATH.";

    /// <summary>Launch options that record the app with the framework's naming and failure decision.</summary>
    public AppLaunchOptions Options() => new()
    {
        Recording = new RecordingOptions
        {
            AllDirectory = AllDirectory,
            FailedDirectory = FailedDirectory,
            FileName = name,
            Failed = failed,
        },
    };

    /// <summary>The videos written so far to the all directory.</summary>
    public string[] Videos() => Files(AllDirectory);

    /// <summary>The videos copied so far to the failed directory.</summary>
    public string[] FailedVideos() => Files(FailedDirectory);

    private static string[] Files(string directory) => Directory.Exists(directory) ? Directory.GetFiles(directory, "*.mp4") : [];

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }
}
