using System.Diagnostics;
using WinUia.Models;
using WinUia.Testing.Shared;

namespace WinUia.UiTests.Launchers;

public sealed class AppLauncherTests
{
    private AutomationContext _context = null!;
    private List<Process> _processes = null!;

    [SetUp]
    public void CreateContext()
    {
        _context = new AutomationContext();
        _processes = [];
    }

    [TearDown]
    public void KillProcesses()
    {
        foreach (var process in _processes)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            process.Dispose();
        }

        _context.Dispose();
    }

    private Process Track(Process process)
    {
        _processes.Add(process);
        return process;
    }

    [Test]
    public void LaunchExe_and_GetMainWindow_find_the_fixture_window()
    {
        var process = Track(AppLauncher.LaunchExe(AppPaths.TestApp));

        var window = MainWindowLocator.Find(_context, process, TimeSpan.FromSeconds(15));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(window.ProcessId, Is.EqualTo(process.Id));
            Assert.That(window.Name, Is.EqualTo("WinUia Test App"));
            Assert.That(window.ControlType, Is.EqualTo(ControlType.Window));
        }
    }

    [Test]
    public void Close_closes_the_window_gracefully()
    {
        var process = Track(AppLauncher.LaunchExe(AppPaths.TestApp));
        MainWindowLocator.Find(_context, process, TimeSpan.FromSeconds(15));

        AppCloser.Close(_context, process, TimeSpan.FromSeconds(10));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(process.HasExited, Is.True);
            Assert.That(process.ExitCode, Is.Zero);
        }
    }

    [Test]
    public void Close_kills_a_window_that_refuses_to_close()
    {
        var process = Track(AppLauncher.LaunchExe(AppPaths.TestApp, new AppLaunchOptions { Arguments = "--ignore-close" }));
        MainWindowLocator.Find(_context, process, TimeSpan.FromSeconds(15));

        AppCloser.Close(_context, process, TimeSpan.FromSeconds(1));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(process.HasExited, Is.True);
            Assert.That(process.ExitCode, Is.Not.Zero);
        }
    }

    [Test]
    public void Attach_finds_a_running_process_by_id()
    {
        var process = Track(AppLauncher.LaunchExe(AppPaths.TestApp));
        MainWindowLocator.Find(_context, process, TimeSpan.FromSeconds(15));

        using var byId = AppLauncher.Attach(process.Id);

        Assert.That(byId.Id, Is.EqualTo(process.Id));
    }
}
