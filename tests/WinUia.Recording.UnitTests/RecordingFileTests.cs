namespace WinUia.Recording.UnitTests;

public class RecordingFileTests
{
    [Test]
    public void The_name_becomes_a_file_with_the_recorders_extension_in_the_given_directory()
    {
        var path = RecordingFile.Choose(@"C:\out", "MyTest", ".mp4", _ => false);

        Assert.That(path, Is.EqualTo(@"C:\out\MyTest.mp4"));
    }

    [Test]
    public void Another_recorders_extension_is_used_as_given()
    {
        var path = RecordingFile.Choose(@"C:\out", "MyTest", ".avi", _ => false);

        Assert.That(path, Is.EqualTo(@"C:\out\MyTest.avi"));
    }

    [Test]
    public void Characters_that_are_invalid_in_a_file_name_are_replaced()
    {
        var path = RecordingFile.Choose(@"C:\out", "Tests.Cases(\"a/b\")", ".mp4", _ => false);

        Assert.That(Path.GetFileName(path), Is.EqualTo("Tests.Cases(_a_b_).mp4"));
    }

    [Test]
    public void A_second_recording_with_the_same_name_gets_a_number_instead_of_overwriting()
    {
        var taken = new HashSet<string> { @"C:\out\MyTest.mp4", @"C:\out\MyTest-2.mp4" };

        var path = RecordingFile.Choose(@"C:\out", "MyTest", ".mp4", taken.Contains);

        Assert.That(path, Is.EqualTo(@"C:\out\MyTest-3.mp4"));
    }

    [Test]
    public void Without_a_name_the_time_is_used()
    {
        var path = RecordingFile.Choose(@"C:\out", name: null, ".mp4", _ => false);

        Assert.That(Path.GetFileName(path), Does.Match(@"^\d{8}-\d{6}\.mp4$"));
    }

    [Test]
    public void The_directories_default_to_All_and_Failed_under_TestResults_Recordings()
    {
        var (all, failed) = RecordingFile.ResolveDirectories(null, null);

        Assert.That(all, Is.EqualTo(Path.Combine("TestResults", "Recordings", "All")));
        Assert.That(failed, Is.EqualTo(Path.Combine("TestResults", "Recordings", "Failed")));
    }

    [Test]
    public void An_option_overrides_its_own_directory_only()
    {
        var (all, failed) = RecordingFile.ResolveDirectories(@"C:\videos", "  ");

        Assert.That(all, Is.EqualTo(@"C:\videos"));
        Assert.That(failed, Is.EqualTo(Path.Combine("TestResults", "Recordings", "Failed")));
    }
}
