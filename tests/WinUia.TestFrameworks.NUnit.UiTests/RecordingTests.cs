using NUnit.Framework.Interfaces;
using WinUia.Recording;
using WinUia.TestFrameworks.Common;

namespace WinUia.TestFrameworks.NUnit.UiTests;

public sealed class RecordingTests
{
    private RecordingScenario _recording = null!;
    private App? _app;
    private bool _expectNoVideo;

    [SetUp]
    public void Prepare()
    {
        _recording = new RecordingScenario(
            name: () => TestContext.CurrentContext.Test.Name,
            keep: () => TestContext.CurrentContext.Result.Outcome.Status is TestStatus.Failed or TestStatus.Inconclusive);

        Assert.That(_recording.UnavailableReason, Is.Null, RecordingScenario.FfmpegRequired);
    }

    [TearDown]
    public void Clean()
    {
        // The outcome of a test is known in teardown, which is where the video is kept or deleted.
        _app?.Dispose();
        var videos = _recording.Videos();
        _recording.Dispose();

        if (!_expectNoVideo)
            return;

        Assert.That(TestContext.CurrentContext.Result.Outcome.Status, Is.Not.EqualTo(TestStatus.Inconclusive), "NUnit did not report the outcome of the test in teardown");
        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed)
            Assert.That(videos, Is.Empty, "a passing test should leave no video");
    }

    [Test]
    public void The_video_is_named_after_the_test_and_a_passing_test_leaves_none()
    {
        _expectNoVideo = true;

        _app = App.Launch(ExampleApp.Path, _recording.Options(RecordingMode.Failed));

        Assert.That(Path.GetFileName(_recording.Videos().Single()), Does.Contain(nameof(The_video_is_named_after_the_test_and_a_passing_test_leaves_none)));
    }

    [Test]
    public void Mode_all_keeps_a_video_with_content()
    {
        var app = App.Launch(ExampleApp.Path, _recording.Options(RecordingMode.All));
        app.Dispose();

        Assert.That(new FileInfo(_recording.Videos().Single()).Length, Is.GreaterThan(0));
    }
}
