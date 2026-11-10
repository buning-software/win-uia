namespace WinUia.Recording.UnitTests;

public class AppRecordingTests
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

    private AppRecording Create(RecordingMode mode, Func<bool>? keep, Action<string>? onKept = null) =>
        new(new ScreenRecording(_file, new NoSession()), mode, keep, onKept);

    [Test]
    public void A_kept_recording_is_reported_with_its_path()
    {
        string? kept = null;

        Create(RecordingMode.Failed, () => true, path => kept = path).Dispose();

        Assert.That(kept, Is.EqualTo(_file));
    }

    [Test]
    public void A_deleted_recording_is_not_reported()
    {
        var reported = false;

        Create(RecordingMode.Failed, () => false, _ => reported = true).Dispose();

        Assert.That(reported, Is.False);
    }

    [Test]
    public void A_throwing_kept_callback_does_not_throw()
    {
        var recording = Create(RecordingMode.Failed, () => true, _ => throw new InvalidOperationException());

        Assert.DoesNotThrow(recording.Dispose);
    }

    [Test]
    public void A_failed_test_keeps_its_recording()
    {
        Create(RecordingMode.Failed, () => true).Dispose();

        Assert.That(File.Exists(_file), Is.True);
    }

    [Test]
    public void A_passing_test_deletes_its_recording_in_failed_mode()
    {
        Create(RecordingMode.Failed, () => false).Dispose();

        Assert.That(File.Exists(_file), Is.False);
    }

    [Test]
    public void Mode_all_keeps_a_passing_tests_recording_without_asking()
    {
        var asked = false;

        Create(RecordingMode.All, () => { asked = true; return false; }).Dispose();

        Assert.That(File.Exists(_file), Is.True);
        Assert.That(asked, Is.False);
    }

    [Test]
    public void Without_a_keep_decision_the_recording_is_kept()
    {
        Create(RecordingMode.Failed, keep: null).Dispose();

        Assert.That(File.Exists(_file), Is.True);
    }

    [Test]
    public void A_throwing_keep_decision_keeps_the_recording_without_throwing()
    {
        var recording = Create(RecordingMode.Failed, () => throw new InvalidOperationException());

        Assert.DoesNotThrow(recording.Dispose);

        Assert.That(File.Exists(_file), Is.True);
    }

    private sealed class NoSession : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
