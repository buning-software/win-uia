
namespace WinUia.Core.IntegrationTests.Helpers;

public class DesktopLockTests
{
    private const string MachineWideName = @"Local\WinUia.Desktop";

    // The real lock is machine-wide, so other test assemblies holding it at the same moment would interfere.
    [SetUp]
    public void UseAMutexOfItsOwn() => DesktopLock.Name = $@"Local\WinUia.Desktop.Tests.{Guid.NewGuid():N}";

    [TearDown]
    public void UseTheMachineWideMutex() => DesktopLock.Name = MachineWideName;

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
    public void Acquire_times_out_while_another_process_keeps_the_desktop()
    {
        // Another process holds the named mutex; a thread stands in for it.
        using var taken = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var otherProcess = new Thread(() =>
        {
            using var mutex = new Mutex(false, DesktopLock.Name);
            mutex.WaitOne();
            taken.Set();
            release.Wait();
            mutex.ReleaseMutex();
        });
        otherProcess.Start();
        taken.Wait();

        try
        {
            Assert.That(() => DesktopLock.Acquire(TimeSpan.FromMilliseconds(200)), Throws.TypeOf<TimeoutException>());
        }
        finally
        {
            release.Set();
            otherProcess.Join();
        }
    }

    [Test]
    public void Locks_nest_within_a_process_and_the_desktop_is_given_back_with_the_last_one()
    {
        var outer = DesktopLock.Acquire(TimeSpan.FromMinutes(5));
        var inner = DesktopLock.Acquire(TimeSpan.FromMilliseconds(200));   // returns at once, it does not wait for outer
        outer.Dispose();

        Assert.That(DesktopHeldByAnotherProcess(), Is.True, "the inner lock still holds the desktop");

        inner.Dispose();

        Assert.That(DesktopHeldByAnotherProcess(), Is.False, "the last lock gave it back");
    }

    [Test]
    public void Another_thread_can_take_the_lock_while_this_process_holds_it()
    {
        using var held = DesktopLock.Acquire(TimeSpan.FromMinutes(5));

        DesktopLock? other = null;
        var thread = new Thread(() => other = DesktopLock.Acquire(TimeSpan.FromMilliseconds(200)));
        thread.Start();
        thread.Join();

        Assert.That(other, Is.Not.Null);
        other!.Dispose();
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

    // Whether a mutex of the same name, which is how another process would see it, cannot be taken right now.
    private static bool DesktopHeldByAnotherProcess()
    {
        var held = false;
        var probe = new Thread(() =>
        {
            using var mutex = new Mutex(false, DesktopLock.Name);
            if (mutex.WaitOne(TimeSpan.FromMilliseconds(200)))
                mutex.ReleaseMutex();
            else
                held = true;
        });
        probe.Start();
        probe.Join();
        return held;
    }
}
