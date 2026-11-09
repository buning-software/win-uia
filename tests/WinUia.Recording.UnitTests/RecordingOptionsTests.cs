using WinUia.Models;

namespace WinUia.Recording.UnitTests;

[NonParallelizable]
public class RecordingOptionsTests
{
    private string? _previousMode;
    private string? _previousDirectory;
    private string _directory = null!;

    [SetUp]
    public void Prepare()
    {
        _previousMode = Environment.GetEnvironmentVariable("WINUIA_RECORDING");
        _previousDirectory = Environment.GetEnvironmentVariable("WINUIA_RECORDING_DIR");
        Environment.SetEnvironmentVariable("WINUIA_RECORDING", null);
        Environment.SetEnvironmentVariable("WINUIA_RECORDING_DIR", null);
        _directory = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}");
    }

    [TearDown]
    public void Restore()
    {
        Environment.SetEnvironmentVariable("WINUIA_RECORDING", _previousMode);
        Environment.SetEnvironmentVariable("WINUIA_RECORDING_DIR", _previousDirectory);
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }

    private RecordingOptions Options(FakeRecorder recorder, FakeFramework? framework, RecordingMode mode = RecordingMode.Failed) => new()
    {
        Recorder = recorder,
        TestFramework = framework,
        Mode = mode,
        OutputDirectory = _directory,
        Area = new ScreenRect(0, 0, 100, 100),
    };

    private static readonly AppRecordingContext Context = new("Sample.exe", new AppLaunchOptions());

    private string[] Files() => Directory.Exists(_directory) ? Directory.GetFiles(_directory).Select(file => Path.GetFileName(file)!).ToArray() : [];

    [Test]
    public void The_defaults_are_ffmpeg_and_keeping_failed_tests()
    {
        var options = new RecordingOptions();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(options.Recorder, Is.InstanceOf<FfmpegRecorder>());
            Assert.That(options.Mode, Is.EqualTo(RecordingMode.Failed));
            Assert.That(options.TestFramework, Is.Null, "null means no test framework");
        }
    }

    [Test]
    public void Mode_off_starts_nothing()
    {
        Environment.SetEnvironmentVariable("WINUIA_RECORDING", "off");
        var recorder = new FakeRecorder();
        var framework = new FakeFramework { Name = "x" };

        ((IAppRecorder)Options(recorder, framework)).Start(Context).Dispose();

        Assert.That(recorder.Started, Is.Empty);
    }

    [Test]
    public void An_unavailable_recorder_does_not_stop_the_launch_and_records_nothing()
    {
        var recorder = new FakeRecorder { UnavailableReason = "not installed" };

        Assert.DoesNotThrow(() => ((IAppRecorder)Options(recorder, framework: null)).Start(Context).Dispose());
        Assert.That(recorder.Started, Is.Empty);
    }

    [Test]
    public void The_recording_is_named_after_the_test_with_the_recorders_extension_in_the_output_directory()
    {
        var recorder = new FakeRecorder { FileExtension = ".avi" };

        ((IAppRecorder)Options(recorder, new FakeFramework { Name = "Ns.MyTest", Failed = true })).Start(Context).Dispose();

        Assert.That(recorder.Started, Is.EqualTo(new[] { (Path.Combine(_directory, "Ns.MyTest.avi"), (ScreenRect?)new ScreenRect(0, 0, 100, 100)) }));
    }

    [Test]
    public void Without_a_test_framework_the_recording_is_named_after_the_app_and_the_time()
    {
        var recorder = new FakeRecorder();

        ((IAppRecorder)Options(recorder, framework: null)).Start(Context).Dispose();

        Assert.That(Path.GetFileName(recorder.Started.Single().Path), Does.Match(@"^Sample-\d{8}-\d{6}\.mp4$"));
    }

    [Test]
    public void A_passing_test_leaves_no_file_by_default()
    {
        var recorder = new FakeRecorder { WritesFile = true };
        var framework = new FakeFramework { Name = "t", Failed = false };

        ((IAppRecorder)Options(recorder, framework)).Start(Context).Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Files(), Is.Empty);
            Assert.That(framework.Attached, Is.Empty);
        }
    }

    [Test]
    public void A_failed_test_keeps_its_file_and_attaches_it()
    {
        var recorder = new FakeRecorder { WritesFile = true };
        var framework = new FakeFramework { Name = "t", Failed = true };

        ((IAppRecorder)Options(recorder, framework)).Start(Context).Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Files(), Is.EqualTo(new[] { "t.mp4" }));
            Assert.That(framework.Attached, Is.EqualTo(new[] { Path.Combine(_directory, "t.mp4") }));
        }
    }

    [Test]
    public void Keep_all_keeps_a_passing_tests_file()
    {
        var recorder = new FakeRecorder { WritesFile = true };

        ((IAppRecorder)Options(recorder, new FakeFramework { Name = "t", Failed = false }, RecordingMode.All)).Start(Context).Dispose();

        Assert.That(Files(), Is.EqualTo(new[] { "t.mp4" }));
    }

    [Test]
    public void The_environment_overrides_Keep()
    {
        Environment.SetEnvironmentVariable("WINUIA_RECORDING", "all");
        var recorder = new FakeRecorder { WritesFile = true };

        ((IAppRecorder)Options(recorder, new FakeFramework { Name = "t", Failed = false }, RecordingMode.Failed)).Start(Context).Dispose();

        Assert.That(Files(), Is.EqualTo(new[] { "t.mp4" }));
    }

    [Test]
    public void It_is_an_AppLaunchOptions_recording()
    {
        var options = new AppLaunchOptions { Recording = new RecordingOptions() };

        Assert.That(options.Recording, Is.InstanceOf<RecordingOptions>());
    }

    private sealed class FakeRecorder : IRecorder
    {
        public string FileExtension { get; init; } = ".mp4";

        public string? UnavailableReason { get; init; }

        public bool WritesFile { get; init; }

        public List<(string Path, ScreenRect? Area)> Started { get; } = [];

        public IDisposable Start(string outputPath, ScreenRect? area)
        {
            Started.Add((outputPath, area));
            if (WritesFile)
                File.WriteAllText(outputPath, "video");
            return new Session();
        }

        private sealed class Session : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
