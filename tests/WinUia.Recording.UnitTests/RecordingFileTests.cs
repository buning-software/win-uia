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
    public void A_name_too_long_for_the_folder_is_shortened_so_the_path_fits_and_keeps_its_end()
    {
        var folder = @"C:\" + new string('d', 150);
        var name = "WinUia.Examples." + new string('x', 200) + ".The_end";

        var path = RecordingFile.Choose(folder, name, ".mp4", _ => false);

        Assert.That(path.Length, Is.LessThanOrEqualTo(259 - 4));
        Assert.That(Path.GetFileName(path), Is.EqualTo("The_end.mp4"));
    }

    [Test]
    public void A_numbered_recording_in_a_long_folder_still_fits()
    {
        var folder = @"C:\" + new string('d', 150);
        var name = new string('x', 300);
        var taken = new HashSet<string>();
        var first = RecordingFile.Choose(folder, name, ".mp4", _ => false);
        taken.Add(first);

        var second = RecordingFile.Choose(folder, name, ".mp4", taken.Contains);

        Assert.That(second, Is.Not.EqualTo(first));
        Assert.That(second.Length, Is.LessThanOrEqualTo(259));
    }

    [Test]
    public void A_name_that_fits_is_left_alone()
    {
        var path = RecordingFile.Choose(@"C:\out", "WinUia.Tests.MyTest.Does_it", ".mp4", _ => false);

        Assert.That(Path.GetFileName(path), Is.EqualTo("WinUia.Tests.MyTest.Does_it.mp4"));
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
