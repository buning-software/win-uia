using WinUia.Models;

namespace WinUia.UnitTests.Model;

public class AppTests
{
    [Test]
    public void Launch_without_a_path_needs_a_page_object_that_declares_its_executable()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => App.Launch<UnconnectedApp>());

        Assert.That(ex.Message, Does.Contain("ExecutablePath"));
    }

    [Test]
    public void LaunchPackaged_rejects_options_Windows_decides_for_packaged_apps()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.Throws<ArgumentException>(() =>
                App.LaunchPackaged("Any.App_0000000000000!App", new AppLaunchOptions { WorkingDirectory = @"C:\" }));
            Assert.Throws<ArgumentException>(() =>
                App.LaunchPackaged("Any.App_0000000000000!App",
                    new AppLaunchOptions { Environment = new Dictionary<string, string?> { ["A"] = "1" } }));
        }
    }

    [Test]
    public void A_derived_app_created_directly_is_not_connected()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            using var unconnected = new UnconnectedApp();
            _ = unconnected.Process;
        });
    }
}
