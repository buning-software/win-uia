using WinUia.Recording;
using WinUia.TestFrameworks.Common;

namespace WinUia.TestFrameworks.XUnit.UiTests;

public sealed class RecordingTests : IDisposable
{
    private readonly RecordingScenario _recording = new(
        name: () => TestContext.Current.Test?.TestDisplayName,
        failed: () => TestContext.Current.TestState?.Result is not (TestResult.Passed or TestResult.Skipped));

    private App? _app;
    private bool _expectNoFailedVideo;

    public void Dispose()
    {
        // The outcome of a test is known in Dispose, which is where the failed video is copied.
        _app?.Dispose();
        var failedVideos = _recording.FailedVideos();
        _recording.Dispose();

        if (!_expectNoFailedVideo)
            return;

        var result = TestContext.Current.TestState?.Result;
        Assert.True(result is not null, "xUnit did not report the outcome of the test in Dispose");
        if (result == TestResult.Passed)
            Assert.Empty(failedVideos);
    }

    [Fact]
    public void The_video_is_named_after_the_test_and_a_passing_test_leaves_no_failed_video()
    {
        Assert.True(_recording.UnavailableReason is null, RecordingScenario.FfmpegRequired);
        _expectNoFailedVideo = true;

        _app = App.Launch(ExampleApp.Path, _recording.Options());

        Assert.Contains(nameof(The_video_is_named_after_the_test_and_a_passing_test_leaves_no_failed_video), Path.GetFileName(Assert.Single(_recording.Videos())));
    }

    [Fact]
    public void A_video_has_content()
    {
        Assert.True(_recording.UnavailableReason is null, RecordingScenario.FfmpegRequired);

        var app = App.Launch(ExampleApp.Path, _recording.Options());
        app.Dispose();

        Assert.True(new FileInfo(Assert.Single(_recording.Videos())).Length > 0);
    }
}
