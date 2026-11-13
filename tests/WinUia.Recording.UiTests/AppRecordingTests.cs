using WinUia.Models;
using WinUia.Testing.Common;

namespace WinUia.Recording.UiTests;

[NonParallelizable]
public class AppRecordingTests
{
    private string _directory = null!;

    [SetUp]
    public void Prepare()
    {
        Assume.That(new FfmpegRecorder().UnavailableReason, Is.Null, "ffmpeg is not available");
        _directory = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}");
    }

    [TearDown]
    public void Clean()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }

    private IEnumerable<string> Videos(string directory) => Directory.Exists(Path.Combine(_directory, directory)) ? Directory.GetFiles(Path.Combine(_directory, directory), "*.mp4") : [];

    private RecordingOptions Options(bool? failed) => new()
    {
        AllDirectory = Path.Combine(_directory, "All"),
        FailedDirectory = Path.Combine(_directory, "Failed"),
        FileName = () => "launch",
        Failed = failed is null ? null : () => failed.Value,
    };

    private static void LaunchAndClose(RecordingOptions recording)
    {
        using var app = App.Launch(AppPaths.TestApp, new AppLaunchOptions { Recording = recording });
        Assert.That(app.MainWindow.Name, Is.Not.Empty);
    }

    [Test]
    public void A_failed_test_leaves_a_playable_video_named_after_the_test_in_both_directories()
    {
        LaunchAndClose(Options(failed: true));

        using (Assert.EnterMultipleScope())
        {
            foreach (var directory in new[] { "All", "Failed" })
            {
                var video = Videos(directory).Single();
                Assert.That(Path.GetFileName(video), Is.EqualTo("launch.mp4"));
                Assert.That(new FileInfo(video).Length, Is.GreaterThan(0));
            }
        }
    }

    [Test]
    public void A_passing_test_leaves_its_video_in_the_all_directory_only()
    {
        LaunchAndClose(Options(failed: false));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Videos("All").Count(), Is.EqualTo(1));
            Assert.That(Videos("Failed"), Is.Empty);
        }
    }

    [Test]
    public void Without_a_failure_signal_the_video_is_in_the_all_directory_only()
    {
        LaunchAndClose(Options(failed: null));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Videos("All").Count(), Is.EqualTo(1));
            Assert.That(Videos("Failed"), Is.Empty);
        }
    }
}
