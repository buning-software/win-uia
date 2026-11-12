using WinUia.TestFrameworks.XUnit.UiTests.Desktop;

// An assembly fixture is created before the first test and disposed after the last: the desktop is held for the whole
// run, across test processes.
[assembly: AssemblyFixture(typeof(DesktopLockSetUp))]

namespace WinUia.TestFrameworks.XUnit.UiTests.Desktop;

public sealed class DesktopLockSetUp : IDisposable
{
    private readonly DesktopLock _desktop = DesktopLock.Acquire(TimeSpan.FromMinutes(60));

    public void Dispose()
    {
        _desktop.Dispose();
    }
}
