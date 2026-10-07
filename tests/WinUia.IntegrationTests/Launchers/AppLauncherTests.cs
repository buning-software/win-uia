using WinUia.Models;

namespace WinUia.IntegrationTests.Launchers;

public class AppLauncherTests
{
    [Test]
    public void GetMainWindow_fails_fast_when_the_process_exits()
    {
        var ex = Assert.Throws<AppProcessException>(() =>
        {
            using var context = new AutomationContext();
            using var process = AppLauncher.LaunchExe("cmd.exe", new AppLaunchOptions { Arguments = "/c exit 3" });
            MainWindowLocator.Find(context, process, TimeSpan.FromSeconds(15));
        });
        Assert.That(ex.Message, Does.Contain("exited"));
    }

    [Test]
    public void LaunchExe_passes_the_working_directory_and_environment()
    {
        var directory = Directory.CreateTempSubdirectory("winuia-").FullName;
        try
        {
            File.WriteAllText(Path.Combine(directory, "marker.txt"), "");
            // Exits 0 only when started in the directory with the marker and with the variable set; 1 otherwise.
            var options = new AppLaunchOptions
            {
                Arguments = "/c if exist marker.txt (if \"%WINUIA_TEST_VARIABLE%\"==\"set\" (exit 0)) & exit 1",
                WorkingDirectory = directory,
                Environment = new Dictionary<string, string?> { ["WINUIA_TEST_VARIABLE"] = "set" },
            };

            using var process = AppLauncher.LaunchExe("cmd.exe", options);
            Assert.That(process.WaitForExit(10_000), Is.True);
            Assert.That(process.ExitCode, Is.Zero);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public void LaunchPackaged_with_an_unknown_AUMID_throws_an_AppProcessException()
    {
        var ex = Assert.Throws<AppProcessException>(() => AppLauncher.LaunchPackaged("WinUia.NoSuchPackage_0000000000000!App"));
        Assert.That(ex.Message, Does.Contain("WinUia.NoSuchPackage"));
    }
}
