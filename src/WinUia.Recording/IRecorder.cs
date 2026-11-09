using WinUia.Models;

namespace WinUia.Recording;

/// <summary>
/// How the screen is captured. <see cref="FfmpegRecorder"/> ships; another one is passed as
/// <see cref="RecordingOptions.Recorder"/>.
/// </summary>
public interface IRecorder
{
    /// <summary>The extension of the files it writes, with the dot, for example <c>.mp4</c>.</summary>
    string FileExtension { get; }

    /// <summary>
    /// Why this recorder cannot be used on this machine (for example a missing tool), or null when it can.
    /// Asked before <see cref="Start"/>, so an app can still launch without a recording.
    /// </summary>
    string? UnavailableReason { get; }

    /// <summary>
    /// Starts recording <paramref name="area"/> of the virtual screen, in physical pixels (null: the whole desktop), to
    /// <paramref name="outputPath"/>. Disposing the result stops the recording and finalises the file.
    /// </summary>
    /// <exception cref="ScreenRecordingException">The recording could not be started.</exception>
    IDisposable Start(string outputPath, ScreenRect? area);
}
