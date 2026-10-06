using WinUia.Models;

namespace WinUia.UnitTests.Model;

public class AppLaunchOptionsTests
{
    [Test]
    public void Options_override_defaults_property_by_property()
    {
        var defaults = new AppLaunchOptions { Arguments = "--default", ShowPointer = true, MainWindowTimeout = TimeSpan.FromSeconds(7) };

        var effective = defaults.OverriddenBy(new AppLaunchOptions { ShowPointer = false, WorkingDirectory = @"C:\work" });

        Assert.That(effective, Is.EqualTo(new AppLaunchOptions
        {
            Arguments = "--default",
            WorkingDirectory = @"C:\work",
            ShowPointer = false,
            MainWindowTimeout = TimeSpan.FromSeconds(7),
        }));
    }

    [Test]
    public void No_options_keep_the_defaults()
    {
        var defaults = new AppLaunchOptions { ShowPointer = true };

        Assert.That(defaults.OverriddenBy(null), Is.SameAs(defaults));
    }
}
