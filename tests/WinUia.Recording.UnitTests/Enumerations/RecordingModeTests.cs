namespace WinUia.Recording.UnitTests.Enumerations;

public class RecordingModeTests
{
    [TestCase("all", "All")]
    [TestCase(" ALL ", "All")]
    [TestCase("failed", "Failed")]
    [TestCase("off", "Off")]
    public void The_environment_value_is_parsed(string value, string expected)
    {
        Assert.That(RecordingModes.Parse(value)?.ToString(), Is.EqualTo(expected));
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase("nonsense")]
    public void An_empty_or_unknown_environment_value_means_unset(string? value)
    {
        Assert.That(RecordingModes.Parse(value), Is.Null);
    }

    [Test]
    public void The_Keep_option_applies_when_the_environment_says_nothing()
    {
        Assert.That(RecordingModes.Resolve(RecordingMode.Failed, null), Is.EqualTo(RecordingMode.Failed));
        Assert.That(RecordingModes.Resolve(RecordingMode.All, null), Is.EqualTo(RecordingMode.All));
    }

    [Test]
    public void The_environment_overrides_the_Keep_option()
    {
        Assert.That(RecordingModes.Resolve(RecordingMode.Failed, "all"), Is.EqualTo(RecordingMode.All));
        Assert.That(RecordingModes.Resolve(RecordingMode.All, "failed"), Is.EqualTo(RecordingMode.Failed));
        Assert.That(RecordingModes.Resolve(RecordingMode.All, "off"), Is.EqualTo(RecordingMode.Off));
    }
}
