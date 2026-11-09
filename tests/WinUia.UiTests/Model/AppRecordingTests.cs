using System.Diagnostics;
using WinUia.Models;
using WinUia.Testing.Shared;

namespace WinUia.UiTests.Model;

public sealed class AppRecordingTests
{
    [Test]
    public void Launch_starts_the_recording_before_the_process_and_Dispose_stops_it_after_the_app_closed()
    {
        var recorder = new FakeRecorder();
        var processExistedWhenStarted = true;
        recorder.OnStart = () => processExistedWhenStarted = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(AppPaths.TestApp)).Length > 0;

        var app = App.Launch(AppPaths.TestApp, new AppLaunchOptions { Recording = recorder });
        var processId = app.Process.Id;
        Assert.That(recorder.Started, Is.EqualTo(1));
        Assert.That(recorder.Stopped, Is.EqualTo(0));

        app.Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(processExistedWhenStarted, Is.False, "the recording should start before the app does");
            Assert.That(recorder.Stopped, Is.EqualTo(1));
            Assert.That(recorder.ProcessAliveWhenStopped, Is.False, "the recording should stop after the app has closed");
            Assert.That(IsRunning(processId), Is.False);
        }
    }

    [Test]
    public void Attach_does_not_start_a_recording()
    {
        using var launched = App.Launch(AppPaths.TestApp);
        RecordingApp.Recorder = new FakeRecorder();

        // Attaching in the same process nests the desktop lock, so this does not wait.
        using var attached = App.Attach<RecordingApp>(launched.Process.Id);

        Assert.That(RecordingApp.Recorder.Started, Is.EqualTo(0));
    }

    private sealed class RecordingApp : App
    {
        public static FakeRecorder Recorder { get; set; } = new();

        protected override AppLaunchOptions DefaultOptions => new() { Recording = Recorder };
    }

    [Test]
    public void The_recording_is_stopped_when_the_launch_fails()
    {
        var recorder = new FakeRecorder();

        Assert.Throws<AppProcessException>(() => App.Launch(@"C:\does\not\exist.exe", new AppLaunchOptions { Recording = recorder }));

        Assert.That(recorder.Stopped, Is.EqualTo(recorder.Started));
        Assert.That(recorder.Started, Is.EqualTo(1));
    }

    private static bool IsRunning(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private sealed class FakeRecorder : IAppRecorder
    {
        public int Started { get; private set; }

        public int Stopped { get; private set; }

        public bool ProcessAliveWhenStopped { get; private set; } = true;

        public Action? OnStart { get; set; }

        public IDisposable Start(AppRecordingContext context)
        {
            Started++;
            OnStart?.Invoke();
            return new Stop(this);
        }

        private sealed class Stop(FakeRecorder recorder) : IDisposable
        {
            public void Dispose()
            {
                recorder.Stopped++;
                recorder.ProcessAliveWhenStopped = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(AppPaths.TestApp)).Length > 0;
            }
        }
    }
}
