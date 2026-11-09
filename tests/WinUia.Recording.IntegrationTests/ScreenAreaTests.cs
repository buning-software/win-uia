namespace WinUia.Recording.IntegrationTests;

public class ScreenAreaTests
{
    [Test]
    public void The_monitor_containing_a_point_on_the_primary_monitor_has_a_positive_size_and_contains_it()
    {
        var area = ScreenArea.MonitorContaining(new ScreenRect(10, 10, 110, 110));

        Assert.That(area.IsEmpty, Is.False);
        Assert.That(area.Left, Is.LessThanOrEqualTo(10));
        Assert.That(area.Top, Is.LessThanOrEqualTo(10));
        Assert.That(area.Right, Is.GreaterThanOrEqualTo(110));
        Assert.That(area.Bottom, Is.GreaterThanOrEqualTo(110));
    }

    [Test]
    public void Recording_an_area_produces_a_video_of_exactly_that_size()
    {
        Assume.That(new FfmpegRecorder().UnavailableReason, Is.Null, "ffmpeg is not available");
        var area = ScreenArea.MonitorContaining(new ScreenRect(10, 10, 110, 110));
        var file = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}", "area.mp4");

        try
        {
            using (var recording = ScreenRecording.Start(file, area: area))
            {
                recording.Keep = true;
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Assert.That(new FileInfo(file).Length, Is.GreaterThan(0));
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(file)!, recursive: true);
        }
    }
}
