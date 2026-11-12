using WinUia.Models;
using WinUia.Recording;

namespace WinUia.TestFrameworks.Common;

/// <summary>
/// The setup every framework's recording tests need: a folder of their own for the videos, and <c>WINUIA_RECORDING</c>
/// (set on CI) out of the way, because it overrides the mode a test asks for. Dispose restores and deletes both.
/// The test framework supplies only what is its own: how the running test is named, and whether to keep its video.
/// </summary>
public sealed class RecordingScenario : IDisposable
{
    private const string ModeVariable = "WINUIA_RECORDING";

    private readonly Func<string?> _name;
    private readonly Func<bool> _keep;
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}");
    private readonly string? _previousMode = Environment.GetEnvironmentVariable(ModeVariable);

    public RecordingScenario(Func<string?> name, Func<bool> keep)
    {
        _name = name;
        _keep = keep;
        Environment.SetEnvironmentVariable(ModeVariable, null);
    }

    /// <summary>Why ffmpeg cannot be used on this machine, or null. These tests need it, so a reason is a failure.</summary>
    public string? UnavailableReason => new FfmpegRecorder().UnavailableReason;

    /// <summary>The message a test fails with when <see cref="UnavailableReason"/> is set.</summary>
    public const string FfmpegRequired = "ffmpeg is not installed, so recording cannot be validated. Install ffmpeg and add it to PATH.";

    /// <summary>Launch options that record the app with the framework's naming and keep decision.</summary>
    public AppLaunchOptions Options(RecordingMode mode) => new()
    {
        Recording = new RecordingOptions
        {
            OutputDirectory = _directory,
            Mode = mode,
            Name = _name,
            Keep = _keep,
        },
    };

    /// <summary>The videos written so far.</summary>
    public string[] Videos() => Directory.Exists(_directory) ? Directory.GetFiles(_directory, "*.mp4") : [];

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(ModeVariable, _previousMode);
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }
}
