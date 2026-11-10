// The desktop is held from before the first test until after the last, across test processes.
namespace WinUia.TestFrameworks.MsTest.UiTests.Desktop;

[TestClass]
public static class DesktopLockSetUp
{
    private static DesktopLock? s_desktop;

    [AssemblyInitialize]
    public static void TakeDesktop(TestContext context)
    {
        s_desktop = DesktopLock.Acquire();
    }

    [AssemblyCleanup]
    public static void ReleaseDesktop()
    {
        s_desktop?.Dispose();
    }
}
