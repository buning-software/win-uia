using System.Diagnostics;
using WinUia.Models;
using WinUia.Testing.Shared;

namespace WinUia.UiTests.Model;

public sealed class AppTests
{
    private App _app = null!;

    [SetUp]
    public void LaunchApp() => _app = App.Launch(AppPaths.TestApp);

    [TearDown]
    public void CloseApp() => _app.Dispose();

    [Test]
    public void Launch_finds_the_main_window()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_app.MainWindow.Name, Is.EqualTo("WinUia Test App"));
            Assert.That(_app.MainWindow.ProcessId, Is.EqualTo(_app.Process.Id));
        }
    }

    [Test]
    public void Find_of_a_missing_element_throws_after_the_timeout_and_TryFind_returns_null()
    {
        _ = _app.MainWindow; // The first Find also waits for the window to appear; keep that out of the timing.

        var stopwatch = Stopwatch.StartNew();
        Assert.Throws<UiaElementNotFoundException>(() => _app.Find("doesNotExist", TimeSpan.FromMilliseconds(300)));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.InRange(290, 5000));

        Assert.That(_app.TryFind("doesNotExist", TimeSpan.FromMilliseconds(100)), Is.Null);
    }

    [Test]
    public void Close_ends_the_process()
    {
        _app.Close();

        Assert.That(_app.Process.HasExited, Is.True);
    }

    [Test]
    public void Dispose_closes_a_launched_app_but_not_an_attached_one()
    {
        using (var attached = App.Attach(_app.Process.Id))
        {
            Assert.That(attached.MainWindow.Name, Is.EqualTo("WinUia Test App"));
        }

        Assert.That(_app.Process.HasExited, Is.False);

        var pid = _app.Process.Id;
        _app.Dispose();
        Assert.Throws<ArgumentException>(() => Process.GetProcessById(pid));
    }

    [Test]
    public void The_generic_factories_return_the_page_object_connected()
    {
        using var launched = App.Launch<DerivedApp>(AppPaths.TestApp);
        using (var attached = App.Attach<DerivedApp>(launched.Process.Id))
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(launched.Title, Is.EqualTo("WinUia Test App"));
                Assert.That(attached.Title, Is.EqualTo("WinUia Test App"));
            }
        }

        Assert.That(launched.Process.HasExited, Is.False, "disposing the attached page object leaves the app running");
    }

    [Test]
    public void Launch_applies_the_options_to_the_app()
    {
        using var app = App.Launch<DerivedApp>(AppPaths.TestApp, new AppLaunchOptions
        {
            ShowPointer = true,
            MainWindowTimeout = TimeSpan.FromSeconds(7),
        });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(app.Context.ShowPointer, Is.True);
            Assert.That(app.MainWindowTimeout, Is.EqualTo(TimeSpan.FromSeconds(7)));
            Assert.That(app.Title, Is.EqualTo("WinUia Test App"));
        }
    }

    [Test]
    public void Launch_with_arguments_passes_them_to_the_app()
    {
        // The test app refuses to close with --ignore-close, so Close has to fall back to killing it.
        using var app = App.Launch(AppPaths.TestApp, new AppLaunchOptions { Arguments = "--ignore-close" });
        _ = app.MainWindow;

        app.Close(TimeSpan.FromSeconds(1));

        Assert.That(app.Process.ExitCode, Is.Not.Zero);
    }

    [Test]
    public void Launch_without_a_path_uses_the_page_objects_executable_and_defaults()
    {
        using var app = App.Launch<LaunchableApp>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(app.Title, Is.EqualTo("WinUia Test App"));
            Assert.That(app.Context.ShowPointer, Is.True);
            Assert.That(app.MainWindowTimeout, Is.EqualTo(TimeSpan.FromSeconds(7)));
        }
    }

    [Test]
    public void Options_passed_to_Launch_override_the_page_objects_defaults()
    {
        using var app = App.Launch<LaunchableApp>(options: new AppLaunchOptions { ShowPointer = false });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(app.Context.ShowPointer, Is.False, "overridden");
            Assert.That(app.MainWindowTimeout, Is.EqualTo(TimeSpan.FromSeconds(7)), "kept from the defaults");
        }
    }
}
