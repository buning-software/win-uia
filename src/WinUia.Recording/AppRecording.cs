namespace WinUia.Recording;

internal sealed class AppRecording(ScreenRecording recording, RecordingMode mode, Func<bool>? keepVideo, Action<string>? onKept = null) : IDisposable
{
    public void Dispose()
    {
        // Keep the video when it cannot be decided, so a failure is never lost.
        var keep = true;
        try
        {
            keep = mode == RecordingMode.All || keepVideo?.Invoke() != false;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            Console.Error.WriteLine($"WinUia.Recording: could not decide whether to keep the video, keeping it: {exception.Message}");
        }

        recording.Keep = keep;
        try
        {
            recording.Dispose();
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            // Stopping the recording must not replace the test's own result.
            Console.Error.WriteLine($"WinUia.Recording: could not stop the recording {recording.OutputPath}: {exception.Message}");
        }

        if (keep && onKept is not null && File.Exists(recording.OutputPath))
        {
            try
            {
                onKept(recording.OutputPath);
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                Console.Error.WriteLine($"WinUia.Recording: OnKept failed for {recording.OutputPath}: {exception.Message}");
            }
        }
    }
}
