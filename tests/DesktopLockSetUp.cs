using WinUia;

// Compiled into every .UiTests project (see tests/Directory.Build.props). A SetUpFixture outside any namespace covers the
// whole assembly: the desktop is held from before the first test until after the last, across test processes.
[SetUpFixture]
public sealed class DesktopLockSetUp
{
    private DesktopLock? _desktop;

    [OneTimeSetUp]
    public void TakeDesktop() => _desktop = DesktopLock.Acquire();

    [OneTimeTearDown]
    public void ReleaseDesktop() => _desktop?.Dispose();
}
