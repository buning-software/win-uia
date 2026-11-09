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

    private AppRecording Create(RecordingMode mode, ITestFramework? framework) =>
        new(new ScreenRecording(_file, new NoSession()), mode, framework);

    [Test]
    public void A_failed_test_keeps_its_recording_and_attaches_it()
    {
        var framework = new FakeFramework { Failed = true };

        Create(RecordingMode.Failed, framework).Dispose();

        Assert.That(File.Exists(_file), Is.True);
        Assert.That(framework.Attached, Is.EqualTo(new[] { _file }));
    }

    [Test]
    public void A_passing_test_deletes_its_recording_in_failed_mode_and_attaches_nothing()
    {
        var framework = new FakeFramework { Failed = false };

        Create(RecordingMode.Failed, framework).Dispose();

        Assert.That(File.Exists(_file), Is.False);
        Assert.That(framework.Attached, Is.Empty);
    }

    [Test]
    public void Mode_all_keeps_a_passing_tests_recording_without_asking_and_attaches_it()
    {
        var framework = new FakeFramework { Failed = false };

        Create(RecordingMode.All, framework).Dispose();

        Assert.That(File.Exists(_file), Is.True);
        Assert.That(framework.Asked, Is.False);
        Assert.That(framework.Attached, Is.EqualTo(new[] { _file }));
    }

    [Test]
    public void Without_a_test_framework_the_recording_is_kept()
    {
        Create(RecordingMode.Failed, framework: null).Dispose();

        Assert.That(File.Exists(_file), Is.True);
    }

    [Test]
    public void When_the_outcome_is_unknown_the_recording_is_kept()
    {
        Create(RecordingMode.Failed, new FakeFramework { Failed = null }).Dispose();

        Assert.That(File.Exists(_file), Is.True);
    }

    [Test]
    public void A_throwing_test_framework_keeps_the_recording_without_throwing()
    {
        var recording = Create(RecordingMode.Failed, new FakeFramework { Throw = true });

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
