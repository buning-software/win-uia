using WinUia.TestFrameworks.Common;

namespace WinUia.TestFrameworks.MsTest.UiTests.Desktop;

[TestClass]
public sealed class DesktopLockTests
{
    [TestMethod]
    public void The_desktop_is_held_for_the_whole_test_run()
    {
        Assert.IsTrue(ExampleApp.DesktopIsHeld());
    }

    [TestMethod]
    public void Taking_the_desktop_again_in_a_test_succeeds_at_once_and_leaves_it_held()
    {
        using (DesktopLock.Acquire(TimeSpan.Zero))
        {
            Assert.IsTrue(ExampleApp.DesktopIsHeld());
        }

        Assert.IsTrue(ExampleApp.DesktopIsHeld());
    }
}
