using WinUia.Models;

namespace WinUia.Recording.UnitTests;

[NonParallelizable]
public class RecordingOptionsTests
{
    private string _directory = null!;
    private string AllDirectory => Path.Combine(_directory, "All");
    private string FailedDirectory => Path.Combine(_directory, "Failed");

    [SetUp]
    public void Prepare()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}");
    }

    [TearDown]
    public void Restore()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }

    private RecordingOptions Options(FakeRecorder recorder, string? name, bool? failed) => new()
    {
        Recorder = recorder,
        FileName = name is null ? null : () => name,
        Failed = failed is null ? null : () => failed.Value,
        AllDirectory = AllDirectory,
        FailedDirectory = FailedDirectory,
        Area = new ScreenRect(0, 0, 100, 100),
    };

    private static readonly AppRecordingContext Context = new("Sample.exe", new AppLaunchOptions());

    private static string[] Files(string directory) => Directory.Exists(directory) ? Directory.GetFiles(directory).Select(file => Path.GetFileName(file)!).ToArray() : [];

    [Test]
    public void The_defaults_are_ffmpeg_and_no_failure_signal()
    {
        var options = new RecordingOptions();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(options.Recorder, Is.InstanceOf<FfmpegRecorder>());
            Assert.That((object?)options.Failed, Is.Null);
            Assert.That(options.AllDirectory, Is.Null);
            Assert.That(options.FailedDirectory, Is.Null);
        }
    }

    [Test]
    public void An_unavailable_recorder_does_not_stop_the_launch_and_records_nothing()
    {
        var recorder = new FakeRecorder { UnavailableReason = "not installed" };

        Assert.DoesNotThrow(() => ((IAppRecorder)Options(recorder, name: null, failed: null)).Start(Context).Dispose());
        Assert.That(recorder.Started, Is.Empty);
    }

    [Test]
    public void The_recording_is_named_after_the_test_with_the_recorders_extension_in_the_output_directory()
    {
        var recorder = new FakeRecorder { FileExtension = ".avi" };

        ((IAppRecorder)Options(recorder, "Ns.MyTest", failed: true)).Start(Context).Dispose();

        Assert.That(recorder.Started, Is.EqualTo(new[] { (Path.Combine(AllDirectory, "Ns.MyTest.avi"), (ScreenRect?)new ScreenRect(0, 0, 100, 100)) }));
    }

    [Test]
    public void Without_a_name_the_recording_is_named_after_the_app_and_the_time()
    {
        var recorder = new FakeRecorder();

        ((IAppRecorder)Options(recorder, name: null, failed: null)).Start(Context).Dispose();

        Assert.That(Path.GetFileName(recorder.Started.Single().Path), Does.Match(@"^Sample-\d{8}-\d{6}\.mp4$"));
    }

    [Test]
    public void A_packaged_app_is_named_after_its_package_not_the_text_after_the_last_dot()
    {
        var recorder = new FakeRecorder();
        var packaged = new AppRecordingContext("Microsoft.WindowsCalculator_8wekyb3d8bbwe!App", new AppLaunchOptions());

        ((IAppRecorder)Options(recorder, name: null, failed: null)).Start(packaged).Dispose();

        Assert.That(Path.GetFileName(recorder.Started.Single().Path), Does.Match(@"^Microsoft\.WindowsCalculator_8wekyb3d8bbwe-\d{8}-\d{6}\.mp4$"));
    }

    [Test]
    public void A_throwing_name_falls_back_to_the_app_name_and_still_records()
    {
        var recorder = new FakeRecorder();
        var options = new RecordingOptions
        {
            Recorder = recorder,
            FileName = () => throw new InvalidOperationException("no test context"),
            AllDirectory = AllDirectory,
            FailedDirectory = FailedDirectory,
            Area = new ScreenRect(0, 0, 100, 100),
        };

        ((IAppRecorder)options).Start(Context).Dispose();

        Assert.That(Path.GetFileName(recorder.Started.Single().Path), Does.Match(@"^Sample-\d{8}-\d{6}\.mp4$"));
    }

    [Test]
    public void A_passing_test_leaves_its_file_in_the_all_directory_only()
    {
        var recorder = new FakeRecorder { WritesFile = true };

        ((IAppRecorder)Options(recorder, "t", failed: false)).Start(Context).Dispose();

        Assert.That(Files(AllDirectory), Is.EqualTo(new[] { "t.mp4" }));
        Assert.That(Files(FailedDirectory), Is.Empty);
    }

    [Test]
    public void A_failed_test_also_gets_a_copy_in_the_failed_directory()
    {
        var recorder = new FakeRecorder { WritesFile = true };

        ((IAppRecorder)Options(recorder, "t", failed: true)).Start(Context).Dispose();

        Assert.That(Files(AllDirectory), Is.EqualTo(new[] { "t.mp4" }));
        Assert.That(Files(FailedDirectory), Is.EqualTo(new[] { "t.mp4" }));
    }

    [Test]
    public void Without_a_failure_signal_nothing_is_copied()
    {
        var recorder = new FakeRecorder { WritesFile = true };

        ((IAppRecorder)Options(recorder, "t", failed: null)).Start(Context).Dispose();

        Assert.That(Files(AllDirectory), Is.EqualTo(new[] { "t.mp4" }));
        Assert.That(Files(FailedDirectory), Is.Empty);
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
