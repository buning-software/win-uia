namespace WinUia.Recording.UnitTests;

public class AppRecordingTests
{
    private string _directory = null!;
    private string _file = null!;
    private string _failedDirectory = null!;

    [SetUp]
    public void CreateFile()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}");
        _failedDirectory = Path.Combine(_directory, "Failed");
        Directory.CreateDirectory(_directory);
        _file = Path.Combine(_directory, "t.mp4");
        File.WriteAllText(_file, "video");
    }

    [TearDown]
    public void DeleteFiles() => Directory.Delete(_directory, recursive: true);

    private AppRecording Create(Func<bool>? failed) => new(new ScreenRecording(_file, new NoSession()), failed, _failedDirectory);

    private string[] Failed() => Directory.Exists(_failedDirectory) ? Directory.GetFiles(_failedDirectory).Select(f => Path.GetFileName(f)!).ToArray() : [];

    [Test]
    public void A_failed_test_copies_its_recording_to_the_failed_directory_and_keeps_the_original()
    {
        Create(() => true).Dispose();

        Assert.That(File.Exists(_file), Is.True);
        Assert.That(Failed(), Is.EqualTo(new[] { "t.mp4" }));
    }

    [Test]
    public void A_passing_test_keeps_its_recording_and_copies_nothing()
    {
        Create(() => false).Dispose();

        Assert.That(File.Exists(_file), Is.True);
        Assert.That(Failed(), Is.Empty);
    }

    [Test]
    public void Without_a_failure_signal_the_recording_is_kept_and_nothing_is_copied()
    {
        Create(failed: null).Dispose();

        Assert.That(File.Exists(_file), Is.True);
        Assert.That(Failed(), Is.Empty);
    }

    [Test]
    public void A_throwing_failure_signal_is_treated_as_failed_without_throwing()
    {
        var recording = Create(() => throw new InvalidOperationException());

        Assert.DoesNotThrow(recording.Dispose);

        Assert.That(File.Exists(_file), Is.True);
        Assert.That(Failed(), Is.EqualTo(new[] { "t.mp4" }));
    }

    [Test]
    public void A_name_already_taken_in_the_failed_directory_gets_a_number()
    {
        Directory.CreateDirectory(_failedDirectory);
        File.WriteAllText(Path.Combine(_failedDirectory, "t.mp4"), "older");

        Create(() => true).Dispose();

        Assert.That(Failed(), Is.EquivalentTo(new[] { "t.mp4", "t-2.mp4" }));
    }

    [Test]
    public void A_failed_copy_does_not_throw()
    {
        File.WriteAllText(_failedDirectory, "a file where the directory should be");

        Assert.DoesNotThrow(Create(() => true).Dispose);
    }

    private sealed class NoSession : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
