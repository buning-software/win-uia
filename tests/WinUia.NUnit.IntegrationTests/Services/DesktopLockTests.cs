
namespace WinUia.NUnit.IntegrationTests.Services;

public class DesktopLockTests
{
    [Test]
    public void Disposing_the_lock_releases_the_desktop()
    {
        var first = DesktopLock.Acquire(TimeSpan.FromMinutes(5));
        first.Dispose();
        first.Dispose();

        using var second = DesktopLock.Acquire(TimeSpan.FromMinutes(5));
        Assert.That(second, Is.Not.Null);
    }

    [Test]
    public void Acquire_times_out_while_another_holder_keeps_the_desktop()
    {
        using var holder = DesktopLock.Acquire(TimeSpan.FromMinutes(5));

        Exception? caught = null;
        var other = new Thread(() =>
        {
            try { using var _ = DesktopLock.Acquire(TimeSpan.FromMilliseconds(200)); }
            catch (Exception ex) { caught = ex; }
        });
        other.Start();
        other.Join();

        Assert.That(caught, Is.TypeOf<TimeoutException>());
    }

    [Test]
    public void Acquire_takes_over_the_desktop_from_a_holder_that_died_without_releasing_it()
    {
        // A thread that ends while holding the mutex abandons it, as a killed test host does.
        var died = new Thread(() =>
        {
            var mutex = new Mutex(false, DesktopLock.Name);
            mutex.WaitOne();
        });
        died.Start();
        died.Join();

        using var desktop = DesktopLock.Acquire(TimeSpan.FromSeconds(5));

        Assert.That(desktop, Is.Not.Null);
    }

    [Test]
    public void The_lock_can_be_released_from_another_thread_than_the_one_that_took_it()
    {
        var desktop = DesktopLock.Acquire(TimeSpan.FromMinutes(5));
        var releaser = new Thread(desktop.Dispose);
        releaser.Start();
        releaser.Join();

        using var next = DesktopLock.Acquire(TimeSpan.FromSeconds(5));

        Assert.That(next, Is.Not.Null);
    }
}
