namespace WinUia.Recording.UnitTests.Ffmpeg;

public class FfmpegLocatorTests
{
    private static string? Locate(string? explicitPath, Dictionary<string, string> environment, params string[] existingFiles) =>
        FfmpegLocator.TryLocate(
            explicitPath,
            name => environment.GetValueOrDefault(name),
            existingFiles.Contains,
            out _);

    [Test]
    public void The_explicit_path_wins_over_the_environment_variable_and_PATH()
    {
        var environment = new Dictionary<string, string> { ["WINUIA_FFMPEG"] = @"C:\env\ffmpeg.exe", ["PATH"] = @"C:\bin" };

        var found = Locate(@"C:\explicit\ffmpeg.exe", environment, @"C:\explicit\ffmpeg.exe", @"C:\env\ffmpeg.exe", @"C:\bin\ffmpeg.exe");

        Assert.That(found, Is.EqualTo(@"C:\explicit\ffmpeg.exe"));
    }

    [Test]
    public void The_environment_variable_wins_over_PATH()
    {
        var environment = new Dictionary<string, string> { ["WINUIA_FFMPEG"] = @"C:\env\ffmpeg.exe", ["PATH"] = @"C:\bin" };

        var found = Locate(null, environment, @"C:\env\ffmpeg.exe", @"C:\bin\ffmpeg.exe");

        Assert.That(found, Is.EqualTo(@"C:\env\ffmpeg.exe"));
    }

    [Test]
    public void A_missing_explicit_path_falls_through_to_the_next_source()
    {
        var environment = new Dictionary<string, string> { ["PATH"] = @"C:\one;C:\two" };

        var found = Locate(@"C:\missing\ffmpeg.exe", environment, @"C:\two\ffmpeg.exe");

        Assert.That(found, Is.EqualTo(@"C:\two\ffmpeg.exe"));
    }

    [Test]
    public void When_nothing_is_found_every_searched_place_is_reported()
    {
        var environment = new Dictionary<string, string> { ["WINUIA_FFMPEG"] = @"C:\env\ffmpeg.exe", ["PATH"] = @"C:\bin" };

        var found = FfmpegLocator.TryLocate(@"C:\explicit\ffmpeg.exe", name => environment.GetValueOrDefault(name), _ => false, out var searched);

        Assert.That(found, Is.Null);
        Assert.That(searched, Has.Count.EqualTo(3));
        Assert.That(string.Join(" ", searched), Does.Contain("FfmpegPath").And.Contain("WINUIA_FFMPEG").And.Contain("PATH"));
    }
}
