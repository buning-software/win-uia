using WinUia.Recording;
using WinUia.TestFrameworks.Common;

namespace WinUia.TestFrameworks.MsTest.UiTests;

[TestClass]
public sealed class RecordingTests
{
    private RecordingScenario _recording = null!;
    private App? _app;
    private bool _expectNoVideo;

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void Prepare()
    {
        _recording = new RecordingScenario(
            name: () => TestContext.TestName,
            keep: () => TestContext.CurrentTestOutcome is not (UnitTestOutcome.Passed or UnitTestOutcome.NotRunnable));

        Assert.IsNull(_recording.UnavailableReason, RecordingScenario.FfmpegRequired);
    }

    [TestCleanup]
    public void Clean()
    {
        // The outcome of a test is known in cleanup, which is where the video is kept or deleted.
        _app?.Dispose();
        var videos = _recording.Videos();
        _recording.Dispose();

        if (!_expectNoVideo)
            return;

        Assert.AreNotEqual(UnitTestOutcome.InProgress, TestContext.CurrentTestOutcome, "MSTest did not report the outcome of the test in cleanup");
        if (TestContext.CurrentTestOutcome == UnitTestOutcome.Passed)
            Assert.AreEqual(0, videos.Length, "a passing test should leave no video");
    }

    [TestMethod]
    public void The_video_is_named_after_the_test_and_a_passing_test_leaves_none()
    {
        _expectNoVideo = true;

        _app = App.Launch(ExampleApp.Path, _recording.Options(RecordingMode.Failed));

        var video = _recording.Videos().Single();
        Assert.Contains(nameof(The_video_is_named_after_the_test_and_a_passing_test_leaves_none), Path.GetFileName(video));
    }

    [TestMethod]
    public void Mode_all_keeps_a_video_with_content()
    {
        var app = App.Launch(ExampleApp.Path, _recording.Options(RecordingMode.All));
        app.Dispose();

        Assert.IsGreaterThan(0L, new FileInfo(_recording.Videos().Single()).Length);
    }
}
