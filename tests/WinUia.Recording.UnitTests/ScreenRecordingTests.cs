namespace WinUia.Recording.UnitTests;

public class ScreenRecordingTests
{
    private string _file = null!;

    [SetUp]
    public void CreateFile()
    {
        _file = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}.mp4");
        File.WriteAllText(_file, "video");
    }

    [TearDown]
    public void DeleteFile() => File.Delete(_file);

    [Test]
    public void Dispose_stops_the_recorder_and_deletes_the_file_by_default()
    {
        var session = new FakeSession();

        new ScreenRecording(_file, session).Dispose();

        Assert.That(session.StopCount, Is.EqualTo(1));
        Assert.That(File.Exists(_file), Is.False);
    }

    [Test]
    public void Dispose_keeps_the_file_when_Keep_is_true()
    {
        var recording = new ScreenRecording(_file, new FakeSession()) { Keep = true };

        recording.Dispose();

        Assert.That(File.Exists(_file), Is.True);
    }

    [Test]
    public void Dispose_twice_stops_the_recorder_once()
    {
        var session = new FakeSession();
        var recording = new ScreenRecording(_file, session);

        recording.Dispose();
        recording.Dispose();

        Assert.That(session.StopCount, Is.EqualTo(1));
    }

    [Test]
    public void The_file_is_still_deleted_when_stopping_the_recorder_fails()
    {
        var recording = new ScreenRecording(_file, new FakeSession { Throw = true });

        Assert.Throws<InvalidOperationException>(recording.Dispose);

        Assert.That(File.Exists(_file), Is.False);
    }

    [Test]
    public void Start_uses_the_given_recorder_and_passes_the_full_path_and_area()
    {
        var recorder = new FakeRecorder();
        var area = new ScreenRect(10, 20, 110, 220);
        var directory = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "sub", "x.avi");

        try
        {
            using var recording = ScreenRecording.Start(path, recorder, area);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(recorder.StartedWith, Is.EqualTo((Path.GetFullPath(path), (ScreenRect?)area)));
                Assert.That(recording.OutputPath, Is.EqualTo(Path.GetFullPath(path)));
                Assert.That(Directory.Exists(Path.GetDirectoryName(path)), Is.True, "the directory should be created");
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public void Start_without_ffmpeg_throws_a_ScreenRecordingException_that_says_where_it_looked()
    {
        var recorder = new FfmpegRecorder { FfmpegPath = Path.Combine(Path.GetTempPath(), "does-not-exist", "ffmpeg.exe") };
        var previousPath = Environment.GetEnvironmentVariable("PATH");
        var previousFfmpeg = Environment.GetEnvironmentVariable("WINUIA_FFMPEG");
        Environment.SetEnvironmentVariable("PATH", string.Empty);
        Environment.SetEnvironmentVariable("WINUIA_FFMPEG", Path.Combine(Path.GetTempPath(), "does-not-exist", "ffmpeg.exe"));
        try
        {
            var exception = Assert.Throws<ScreenRecordingException>(() => ScreenRecording.Start(_file, recorder));

            Assert.That(exception.Message, Does.Contain("FfmpegPath").And.Contain("WINUIA_FFMPEG").And.Contain("PATH"));
            Assert.That(recorder.UnavailableReason, Is.EqualTo(exception.Message));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", previousPath);
            Environment.SetEnvironmentVariable("WINUIA_FFMPEG", previousFfmpeg);
        }
    }

    private sealed class FakeSession : IDisposable
    {
        public int StopCount { get; private set; }

        public bool Throw { get; init; }

        public void Dispose()
        {
            StopCount++;
            if (Throw)
            {
                throw new InvalidOperationException();
            }
        }
    }

    private sealed class FakeRecorder : IRecorder
    {
        public string FileExtension => ".avi";

        public string? UnavailableReason => null;

        public (string Path, ScreenRect? Area)? StartedWith { get; private set; }

        public IDisposable Start(string outputPath, ScreenRect? area)
        {
            StartedWith = (outputPath, area);
            return new FakeSession();
        }
    }
}
