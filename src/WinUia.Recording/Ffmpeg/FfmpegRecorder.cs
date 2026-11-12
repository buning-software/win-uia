namespace WinUia.Recording;

/// <summary>
/// Records with ffmpeg's <c>gdigrab</c>, run as an external process: nothing is bundled, ffmpeg must be installed.
/// It is found through <see cref="FfmpegPath"/>, then the <c>WINUIA_FFMPEG</c> environment variable, then <c>PATH</c>.
/// </summary>
public sealed class FfmpegRecorder : IRecorder
{
    /// <summary>Frames per second. Defaults to 15.</summary>
    public int FrameRate { get; set; } = 15;

    /// <summary>
    /// Full path to ffmpeg.exe. When null, the <c>WINUIA_FFMPEG</c> environment variable and then <c>PATH</c> are searched.
    /// </summary>
    public string? FfmpegPath { get; set; }

    /// <summary>Whether the mouse cursor is drawn in the video. Defaults to true.</summary>
    public bool DrawMouse { get; set; } = true;

    /// <inheritdoc />
    public string FileExtension => ".mp4";

    /// <inheritdoc />
    public string? UnavailableReason =>
        FfmpegLocator.TryLocate(FfmpegPath, out var searched) is null ? FfmpegLocator.NotFoundMessage(searched) : null;

    /// <inheritdoc />
    public IDisposable Start(string outputPath, ScreenRect? area)
    {
        var ffmpeg = FfmpegLocator.Locate(FfmpegPath);
        return FfmpegProcess.Start(ffmpeg, FfmpegArguments.Build(outputPath, this, area));
    }
}
