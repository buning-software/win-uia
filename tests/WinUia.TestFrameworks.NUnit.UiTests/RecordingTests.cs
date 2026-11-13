using NUnit.Framework.Interfaces;
using WinUia.Recording;
using WinUia.TestFrameworks.Common;

namespace WinUia.TestFrameworks.NUnit.UiTests;

public sealed class RecordingTests
{
    private RecordingScenario _recording = null!;
    private App? _app;
    private bool _expectNoFailedVideo;

    [SetUp]
    public void Prepare()
    {
        _recording = new RecordingScenario(
            name: () => TestContext.CurrentContext.Test.Name,
            failed: () => TestContext.CurrentContext.Result.Outcome.Status is TestStatus.Failed or TestStatus.Inconclusive);

        Assert.That(_recording.UnavailableReason, Is.Null, RecordingScenario.FfmpegRequired);
    }

    [TearDown]
    public void Clean()
    {
        // The outcome of a test is known in teardown, which is where the failed video is copied.
        _app?.Dispose();
        var failedVideos = _recording.FailedVideos();
        _recording.Dispose();

        if (!_expectNoFailedVideo)
            return;

        Assert.That(TestContext.CurrentContext.Result.Outcome.Status, Is.Not.EqualTo(TestStatus.Inconclusive), "NUnit did not report the outcome of the test in teardown");
        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed)
            Assert.That(failedVideos, Is.Empty, "a passing test should leave no failed video");
    }

    [Test]
    public void The_video_is_named_after_the_test_and_a_passing_test_leaves_no_failed_video()
    {
        _expectNoFailedVideo = true;

        _app = App.Launch(ExampleApp.Path, _recording.Options());

        Assert.That(Path.GetFileName(_recording.Videos().Single()), Does.Contain(nameof(The_video_is_named_after_the_test_and_a_passing_test_leaves_no_failed_video)));
    }

    [Test]
    public void A_video_has_content()
    {
        var app = App.Launch(ExampleApp.Path, _recording.Options());
        app.Dispose();

        Assert.That(new FileInfo(_recording.Videos().Single()).Length, Is.GreaterThan(0));
    }
}
