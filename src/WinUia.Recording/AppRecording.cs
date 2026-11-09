namespace WinUia.Recording;

internal sealed class AppRecording(ScreenRecording recording, RecordingMode mode, ITestFramework? testFramework) : IDisposable
{
    public void Dispose()
    {
        // Keep the video when it cannot be decided, so a failure is never lost.
        var keep = true;
        try
        {
            keep = mode == RecordingMode.All || testFramework?.CurrentTestFailed != false;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            Console.Error.WriteLine($"WinUia.Recording: could not determine the test outcome, keeping the video: {exception.Message}");
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

        if (keep)
        {
            try
            {
                testFramework?.Attach(recording.OutputPath, "Screen recording");
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                // The video is on disk; a failing attachment must not replace the test's own result.
                Console.Error.WriteLine($"WinUia.Recording: could not attach {recording.OutputPath}: {exception.Message}");
            }
        }
    }
}
