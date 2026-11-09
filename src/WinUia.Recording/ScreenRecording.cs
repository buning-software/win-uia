namespace WinUia.Recording;

/// <summary>
/// A recording in progress. Dispose stops it and deletes the file unless <see cref="Keep"/> is true.
/// Does not take the desktop lock: start it after the desktop is held and dispose it before it is released
/// (<see cref="RecordingOptions"/> does that for an app).
/// </summary>
public sealed class ScreenRecording : IDisposable
{
    private readonly IDisposable _session;
    private bool _disposed;

    internal ScreenRecording(string outputPath, IDisposable session)
    {
        OutputPath = outputPath;
        _session = session;
    }

    /// <summary>The file being written.</summary>
    public string OutputPath { get; }

    /// <summary>Whether the file is kept on dispose. Defaults to false, so only recordings you ask for survive.</summary>
    public bool Keep { get; set; }

    /// <summary>
    /// Starts recording to <paramref name="outputPath"/>, creating its directory. A null <paramref name="recorder"/> means a
    /// <see cref="FfmpegRecorder"/>; <paramref name="area"/> is in physical pixels, null records the whole desktop.
    /// </summary>
    /// <exception cref="ScreenRecordingException">The recorder is unavailable or failed to start.</exception>
    public static ScreenRecording Start(string outputPath, IRecorder? recorder = null, ScreenRect? area = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        recorder ??= new FfmpegRecorder();

        var fullPath = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        return new ScreenRecording(fullPath, recorder.Start(fullPath, area));
    }

    /// <summary>Stops the recording and deletes the file unless <see cref="Keep"/> is true.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            _session.Dispose();
        }
        finally
        {
            if (!Keep && File.Exists(OutputPath))
            {
                File.Delete(OutputPath);
            }
        }
    }
}
