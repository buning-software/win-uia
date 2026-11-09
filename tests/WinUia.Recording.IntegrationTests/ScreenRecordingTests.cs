namespace WinUia.Recording.IntegrationTests;

public class ScreenRecordingTests
{
    [Test]
    public void Recording_for_a_second_produces_a_non_empty_mp4()
    {
        Assume.That(new FfmpegRecorder().UnavailableReason, Is.Null, "ffmpeg is not available");
        var file = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}", "recording.mp4");

        try
        {
            using (var recording = ScreenRecording.Start(file))
            {
                recording.Keep = true;
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Assert.That(new FileInfo(file).Length, Is.GreaterThan(0));
            // An mp4 starts with an ftyp box.
            var header = File.ReadAllBytes(file).Take(12).ToArray();
            Assert.That(System.Text.Encoding.ASCII.GetString(header, 4, 4), Is.EqualTo("ftyp"));
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(file)!, recursive: true);
        }
    }

    [Test]
    public void Recording_without_Keep_leaves_no_file()
    {
        Assume.That(new FfmpegRecorder().UnavailableReason, Is.Null, "ffmpeg is not available");
        var file = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}", "recording.mp4");

        try
        {
            using (ScreenRecording.Start(file))
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Assert.That(File.Exists(file), Is.False);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(file)!, recursive: true);
        }
    }
}
