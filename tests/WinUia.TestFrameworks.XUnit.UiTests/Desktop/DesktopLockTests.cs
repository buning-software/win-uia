using WinUia.TestFrameworks.Common;

namespace WinUia.TestFrameworks.XUnit.UiTests.Desktop;

public sealed class DesktopLockTests
{
    [Fact]
    public void The_desktop_is_held_for_the_whole_test_run()
    {
        Assert.True(ExampleApp.DesktopIsHeld());
    }

    [Fact]
    public void Taking_the_desktop_again_in_a_test_succeeds_at_once_and_leaves_it_held()
    {
        using (DesktopLock.Acquire(TimeSpan.Zero))
        {
            Assert.True(ExampleApp.DesktopIsHeld());
        }

        Assert.True(ExampleApp.DesktopIsHeld());
    }
}
