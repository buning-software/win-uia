using WinUia.TestFrameworks.Common;

namespace WinUia.TestFrameworks.NUnit.UiTests.Desktop;

public sealed class DesktopLockTests
{
    [Test]
    public void The_desktop_is_held_for_the_whole_test_run()
    {
        Assert.That(ExampleApp.DesktopIsHeld(), Is.True);
    }

    [Test]
    public void Taking_the_desktop_again_in_a_test_succeeds_at_once_and_leaves_it_held()
    {
        using (DesktopLock.Acquire(TimeSpan.Zero))
        {
            Assert.That(ExampleApp.DesktopIsHeld(), Is.True);
        }

        Assert.That(ExampleApp.DesktopIsHeld(), Is.True);
    }
}
