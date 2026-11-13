using WinUia.Recording;
using WinUia.TestFrameworks.Common;

namespace WinUia.TestFrameworks.MsTest.UiTests;

[TestClass]
public sealed class RecordingTests
{
    private RecordingScenario _recording = null!;
    private App? _app;
    private bool _expectNoFailedVideo;

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void Prepare()
    {
        _recording = new RecordingScenario(
            name: () => TestContext.TestName,
            failed: () => TestContext.CurrentTestOutcome is not (UnitTestOutcome.Passed or UnitTestOutcome.NotRunnable));

        Assert.IsNull(_recording.UnavailableReason, RecordingScenario.FfmpegRequired);
    }

    [TestCleanup]
    public void Clean()
    {
        // The outcome of a test is known in cleanup, which is where the failed video is copied.
        _app?.Dispose();
        var failedVideos = _recording.FailedVideos();
        _recording.Dispose();

        if (!_expectNoFailedVideo)
            return;

        Assert.AreNotEqual(UnitTestOutcome.InProgress, TestContext.CurrentTestOutcome, "MSTest did not report the outcome of the test in cleanup");
        if (TestContext.CurrentTestOutcome == UnitTestOutcome.Passed)
            Assert.AreEqual(0, failedVideos.Length, "a passing test should leave no failed video");
    }

    [TestMethod]
    public void The_video_is_named_after_the_test_and_a_passing_test_leaves_no_failed_video()
    {
        _expectNoFailedVideo = true;

        _app = App.Launch(ExampleApp.Path, _recording.Options());

        var video = _recording.Videos().Single();
        Assert.Contains(nameof(The_video_is_named_after_the_test_and_a_passing_test_leaves_no_failed_video), Path.GetFileName(video));
    }

    [TestMethod]
    public void A_video_has_content()
    {
        var app = App.Launch(ExampleApp.Path, _recording.Options());
        app.Dispose();

        Assert.IsGreaterThan(0L, new FileInfo(_recording.Videos().Single()).Length);
    }
}
