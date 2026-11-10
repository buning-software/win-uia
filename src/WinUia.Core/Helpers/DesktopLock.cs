using System.Diagnostics;
using static System.Threading.Timeout;

namespace WinUia;

/// <summary>
/// Gives one holder at a time the desktop, across processes. UI tests share one mouse, one keyboard focus and one
/// foreground window, and <c>dotnet test</c> runs test assemblies in parallel, so UI automation takes turns.
/// <para>
/// <c>App.Launch</c> and <c>App.Attach</c> take the lock for the life of the app, so tests that use <c>App</c> need
/// nothing else. Take it yourself to hold the desktop longer, for example for a whole test run, or when you use
/// <see cref="AutomationContext"/> without <c>App</c>. Within a process locks nest: while one is held, further
/// <see cref="Acquire"/> calls succeed at once, and the desktop is given back when the last one is disposed.
/// </para>
/// </summary>
public sealed class DesktopLock : IDisposable
{
    // Settable so that tests of the lock itself can use a mutex of their own instead of the machine-wide one.
    internal static string Name { get; set; } = @"Local\WinUia.Desktop";

    private static readonly Lock Gate = new();
    private static Holder? s_current;

    private readonly Holder _holder;
    private int _released;

    private DesktopLock(Holder holder) => _holder = holder;

    /// <summary>
    /// Waits for the desktop and returns the lock. Dispose it to give the desktop back.
    /// </summary>
    /// <param name="timeout">How long to wait for another holder to give the desktop back. Default 5 minutes.</param>
    /// <exception cref="TimeoutException">Another holder kept the desktop for longer than <paramref name="timeout"/>.</exception>
    public static DesktopLock Acquire(TimeSpan? timeout = null)
    {
        var wait = timeout ?? TimeSpan.FromMinutes(5);
        var infinite = wait == InfiniteTimeSpan;
        var started = Stopwatch.GetTimestamp();

        // Another thread of this process may be waiting for the desktop; it holds the gate until it has it or gives up.
        if (!Gate.TryEnter(wait))
            throw Timeout(wait);
        try
        {
            s_current ??= Holder.Take(infinite ? wait : wait - Stopwatch.GetElapsedTime(started));
            s_current.Count++;
            return new DesktopLock(s_current);
        }
        finally
        {
            Gate.Exit();
        }
    }

    /// <summary>Gives the desktop back. Safe to call more than once, and from any thread.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _released, 1) != 0)
            return;

        lock (Gate)
        {
            if (--_holder.Count > 0)
                return;

            s_current = null;
            _holder.Release();
        }
    }

    private static TimeoutException Timeout(TimeSpan wait) =>
        new($"Another process kept the desktop (UI lock '{Name}') for more than {(wait == InfiniteTimeSpan ? "the timeout" : wait)}.");

    // A mutex must be released by the thread that took it, and setup and teardown may run on different threads, so a
    // dedicated thread owns it while the lock is held.
    private sealed class Holder
    {
        private readonly ManualResetEventSlim _release = new();
        private readonly Thread _owner;

        // Guarded by Gate.
        internal int Count;

        private Holder(TimeSpan timeout, TaskCompletionSource acquired)
        {
            _owner = new Thread(() => Own(timeout, acquired))
            {
                Name = "WinUia desktop lock",
                // A process that exits while holding the lock abandons the mutex, which the next waiter takes over.
                IsBackground = true
            };
            _owner.Start();
        }

        internal static Holder Take(TimeSpan timeout)
        {
            var wait = timeout == InfiniteTimeSpan || timeout > TimeSpan.Zero ? timeout : TimeSpan.Zero;
            var acquired = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var holder = new Holder(wait, acquired);
            try
            {
                acquired.Task.GetAwaiter().GetResult();
            }
            catch
            {
                holder.Release();
                throw;
            }

            return holder;
        }

        internal void Release()
        {
            _release.Set();
            _owner.Join();
            _release.Dispose();
        }

        private void Own(TimeSpan timeout, TaskCompletionSource acquired)
        {
            try
            {
                using var mutex = new Mutex(false, Name);
                try
                {
                    if (!mutex.WaitOne(timeout))
                        throw Timeout(timeout);
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
    }
}
