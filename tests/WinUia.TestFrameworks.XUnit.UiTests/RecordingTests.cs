using WinUia.Recording;
using WinUia.TestFrameworks.Common;

namespace WinUia.TestFrameworks.XUnit.UiTests;

public sealed class RecordingTests : IDisposable
{
    private readonly RecordingScenario _recording = new(
        name: () => TestContext.Current.Test?.TestDisplayName,
        keep: () => TestContext.Current.TestState?.Result is not (TestResult.Passed or TestResult.Skipped));

    private App? _app;
    private bool _expectNoVideo;

    public void Dispose()
    {
        // The outcome of a test is known in Dispose, which is where the video is kept or deleted.
        _app?.Dispose();
        var videos = _recording.Videos();
        _recording.Dispose();

        if (!_expectNoVideo)
            return;

        var result = TestContext.Current.TestState?.Result;
        Assert.True(result is not null, "xUnit did not report the outcome of the test in Dispose");
        if (result == TestResult.Passed)
            Assert.Empty(videos);
    }

    [Fact]
    public void The_video_is_named_after_the_test_and_a_passing_test_leaves_none()
    {
        Assert.True(_recording.UnavailableReason is null, RecordingScenario.FfmpegRequired);
        _expectNoVideo = true;

        _app = App.Launch(ExampleApp.Path, _recording.Options(RecordingMode.Failed));

        Assert.Contains(nameof(The_video_is_named_after_the_test_and_a_passing_test_leaves_none), Path.GetFileName(Assert.Single(_recording.Videos())));
    }

    [Fact]
    public void Mode_all_keeps_a_video_with_content()
    {
        Assert.True(_recording.UnavailableReason is null, RecordingScenario.FfmpegRequired);

        var app = App.Launch(ExampleApp.Path, _recording.Options(RecordingMode.All));
        app.Dispose();

        Assert.True(new FileInfo(Assert.Single(_recording.Videos())).Length > 0);
    }
}
