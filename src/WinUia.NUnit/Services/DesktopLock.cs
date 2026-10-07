namespace WinUia.NUnit;

// A mutex must be released by the thread that took it, and setup and teardown may run on different threads, so a dedicated thread owns it while the lock is held.
internal sealed class DesktopLock : IDisposable
{
    internal const string Name = @"Local\WinUia.Desktop";

    private readonly ManualResetEventSlim _release = new();
    private readonly Thread _owner;
    private int _released;

    private DesktopLock(TimeSpan timeout, TaskCompletionSource acquired)
    {
        _owner = new Thread(() => Own(timeout, acquired))
        {
            Name = "WinUia desktop lock",
            // A process that exits while holding the lock abandons the mutex, which the next waiter takes over.
            IsBackground = true
        };
        _owner.Start();
    }

    public static DesktopLock Acquire(TimeSpan timeout)
    {
        var acquired = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var desktop = new DesktopLock(timeout, acquired);
        try
        {
            acquired.Task.GetAwaiter().GetResult();
        }
        catch
        {
            desktop.Dispose();
            throw;
        }

        return desktop;
    }

    private void Own(TimeSpan timeout, TaskCompletionSource acquired)
    {
        try
        {
            using var mutex = new Mutex(false, Name);
            try
            {
                if (!mutex.WaitOne(timeout))
                    throw new TimeoutException($"Another process kept the desktop (UI test lock '{Name}') for more than {timeout}.");
            }
            catch (AbandonedMutexException)
            {
                // The previous holder died without releasing it. The mutex is ours now.
            }

            acquired.SetResult();
            _release.Wait();
            mutex.ReleaseMutex();
        }
        catch (Exception ex)
        {
            acquired.TrySetException(ex);
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _released, 1) != 0)
            return;
        _release.Set();
        _owner.Join();
        _release.Dispose();
    }
}
