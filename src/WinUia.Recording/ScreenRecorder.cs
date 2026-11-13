using System.Diagnostics;
using WinUia.Models;

namespace WinUia.Recording;

/// <summary>
/// The recording flow behind <see cref="RecordingOptions"/>: starts the recording and says where the video goes.
/// </summary>
internal sealed class ScreenRecorder(RecordingOptions options) : IAppRecorder
{
    public IDisposable Start(AppRecordingContext context)
    {
        var recorder = options.Recorder;

        try
        {
            // A launch must not fail because the recorder is unavailable (a machine without ffmpeg, say): warn and run without a video.
            if (recorder.UnavailableReason is { } reason)
            {
                Warn($"this run is not recorded. {reason}");
                return Nothing.Instance;
            }

            var name = ChooseName(context);
            var (allDirectory, failedDirectory) = RecordingFile.ResolveDirectories(options.AllDirectory, options.FailedDirectory);
            var path = RecordingFile.Choose(allDirectory, name, recorder.FileExtension);
            var recording = ScreenRecording.Start(path, recorder, options.Area ?? ScreenArea.PrimaryMonitor());
            return new AppRecording(recording, options.Failed, failedDirectory);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            // Same rule: a recorder that fails to start (locked desktop, bad area, unwritable folder) must not fail the test.
            Warn($"this run is not recorded. {exception.Message}");
            return Nothing.Instance;
        }
    }

    // The name is cosmetic: a Name that throws or gives nothing must not cost the video.
    private string ChooseName(AppRecordingContext context)
    {
        try
        {
            if (options.FileName?.Invoke() is { Length: > 0 } name)
                return name;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            Warn($"could not name the video after the test, using the app name: {exception.Message}");
        }

        return DefaultName(context);
    }

    private static readonly char[] PathSeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    // Without a test, name the video after the app and the time, so several apps recorded in one run stay apart.
    private static string DefaultName(AppRecordingContext context)
    {
        // A packaged app's target is an AppUserModelID (PackageFamilyName!AppId): the dot in the name is not an extension.
        var target = context.Target;
        var app = target.Contains('!') && target.IndexOfAny(PathSeparators) < 0
            ? target[..target.IndexOf('!')]
            : Path.GetFileNameWithoutExtension(target);
        var time = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        return string.IsNullOrWhiteSpace(app) ? time : $"{app}-{time}";
    }

    // Trace has no listener in most test hosts, so also write to the console, which test runners capture.
    private static void Warn(string message)
    {
        Trace.TraceWarning($"WinUia.Recording: {message}");
        Console.Error.WriteLine($"WinUia.Recording: {message}");
    }

    private sealed class Nothing : IDisposable
    {
        public static readonly Nothing Instance = new();

        public void Dispose()
        {
        }
    }
}
