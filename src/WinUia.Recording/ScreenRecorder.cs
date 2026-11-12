using System.Diagnostics;
using WinUia.Models;

namespace WinUia.Recording;

/// <summary>
/// The recording flow behind <see cref="RecordingOptions"/>: decides whether to record, where the video goes and which
/// videos are kept. The environment is read through <c>getEnvironmentVariable</c>, so the flow can be tested without it.
/// </summary>
internal sealed class ScreenRecorder(RecordingOptions options, Func<string, string?> getEnvironmentVariable) : IAppRecorder
{
    public IDisposable Start(AppRecordingContext context)
    {
        var environmentMode = getEnvironmentVariable(RecordingModes.EnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(environmentMode) && RecordingModes.Parse(environmentMode) is null)
            Warn($"ignoring {RecordingModes.EnvironmentVariable}='{environmentMode}'; use failed, all or off.");

        var mode = RecordingModes.Resolve(options.Mode, environmentMode);
        if (mode == RecordingMode.Off)
            return Nothing.Instance;

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
            var folder = RecordingFile.ResolveFolder(options.OutputDirectory, getEnvironmentVariable(RecordingFile.DirectoryEnvironmentVariable));
            var path = RecordingFile.Choose(folder, name, recorder.FileExtension);
            var recording = ScreenRecording.Start(path, recorder, options.Area ?? ScreenArea.PrimaryMonitor());
            return new AppRecording(recording, mode, options.Keep, options.OnKept);
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
            if (options.Name?.Invoke() is { Length: > 0 } name)
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
