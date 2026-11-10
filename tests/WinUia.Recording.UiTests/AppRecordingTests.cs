using WinUia.Models;
using WinUia.Testing.Common;

namespace WinUia.Recording.UiTests;

[NonParallelizable]
public class AppRecordingTests
{
    private string _directory = null!;
    private string? _previousMode;

    [SetUp]
    public void Prepare()
    {
        Assume.That(new FfmpegRecorder().UnavailableReason, Is.Null, "ffmpeg is not available");
        _directory = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}");
        _previousMode = Environment.GetEnvironmentVariable("WINUIA_RECORDING");
        Environment.SetEnvironmentVariable("WINUIA_RECORDING", null);
    }

    [TearDown]
    public void Clean()
    {
        Environment.SetEnvironmentVariable("WINUIA_RECORDING", _previousMode);
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }

    private IEnumerable<string> Videos() => Directory.Exists(_directory) ? Directory.GetFiles(_directory, "*.mp4") : [];

    private RecordingOptions Options(bool failed, RecordingMode mode = RecordingMode.Failed) => new()
    {
        OutputDirectory = _directory,
        Mode = mode,
        Name = () => "launch",
        Keep = () => failed,
    };

    private static void LaunchAndClose(RecordingOptions recording)
    {
        using var app = App.Launch(AppPaths.TestApp, new AppLaunchOptions { Recording = recording });
        Assert.That(app.MainWindow.Name, Is.Not.Empty);
    }

    [Test]
    public void A_failed_test_keeps_a_playable_video_named_after_the_test()
    {
        LaunchAndClose(Options(failed: true));

        var video = Videos().Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Path.GetFileName(video), Is.EqualTo("launch.mp4"));
            Assert.That(new FileInfo(video).Length, Is.GreaterThan(0));
        }
    }

    [Test]
    public void A_passing_test_leaves_no_video()
    {
        LaunchAndClose(Options(failed: false));

        Assert.That(Videos(), Is.Empty);
    }

    [Test]
    public void Keep_all_keeps_the_video_of_a_passing_test()
    {
        LaunchAndClose(Options(failed: false, RecordingMode.All));

        Assert.That(Videos().Count(), Is.EqualTo(1));
    }

    [Test]
    public void WINUIA_RECORDING_all_keeps_the_video_of_a_passing_test()
    {
        Environment.SetEnvironmentVariable("WINUIA_RECORDING", "all");

        LaunchAndClose(Options(failed: false));

        Assert.That(Videos().Count(), Is.EqualTo(1));
    }

    [Test]
    public void WINUIA_RECORDING_off_records_nothing()
    {
        Environment.SetEnvironmentVariable("WINUIA_RECORDING", "off");

        LaunchAndClose(Options(failed: true));

        Assert.That(Videos(), Is.Empty);
    }

    [Test]
    public void Without_a_Keep_decision_the_video_is_kept()
    {
        var options = new RecordingOptions { OutputDirectory = _directory };

        LaunchAndClose(options);

        Assert.That(Videos(), Has.Exactly(1).Items);
    }
}
