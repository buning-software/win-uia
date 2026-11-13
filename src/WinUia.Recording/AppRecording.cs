namespace WinUia.Recording;

internal sealed class AppRecording(ScreenRecording recording, Func<bool>? failed, string failedDirectory) : IDisposable
{
    public void Dispose()
    {
        // Every video stays in the all directory; the failure signal only routes a copy.
        recording.Keep = true;
        try
        {
            recording.Dispose();
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            // Stopping the recording must not replace the test's own result.
            Console.Error.WriteLine($"WinUia.Recording: could not stop the recording {recording.OutputPath}: {exception.Message}");
        }

        if (!File.Exists(recording.OutputPath) || !HasFailed())
            return;

        try
        {
            var path = RecordingFile.Choose(failedDirectory, Path.GetFileNameWithoutExtension(recording.OutputPath), Path.GetExtension(recording.OutputPath));
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.Copy(recording.OutputPath, path);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            Console.Error.WriteLine($"WinUia.Recording: could not copy {recording.OutputPath} to {failedDirectory}: {exception.Message}");
        }
    }

    // When it cannot be decided, treat the test as failed, so a failure is never lost.
    private bool HasFailed()
    {
        try
        {
            return failed?.Invoke() == true;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            Console.Error.WriteLine($"WinUia.Recording: could not decide whether the test failed, copying the video to the failed directory: {exception.Message}");
            return true;
        }
    }
}
