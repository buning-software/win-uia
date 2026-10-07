using System.Diagnostics;

namespace WinUia;

/// <summary>Waiting for UI state that the application updates asynchronously.</summary>
public static class Poll
{
    /// <summary>How long WinUia waits by default: 5 seconds.</summary>
    public static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);

    internal static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(100);

    private static readonly object Holds = new();

    /// <summary>
    /// Calls <paramref name="condition"/> until it returns true, polling at WinUia's default interval, and returns
    /// whether it did before <paramref name="timeout"/> (5 seconds unless given) elapsed. The condition runs at
    /// least once, so a zero timeout means "look now".
    /// </summary>
    public static bool Until(Func<bool> condition, TimeSpan? timeout = null) =>
        Until(condition, timeout ?? DefaultTimeout, DefaultInterval);

    internal static T? Until<T>(Func<T?> probe, TimeSpan timeout, TimeSpan interval) where T : class
    {
        var waited = Stopwatch.StartNew();
        while (true)
        {
            if (probe() is { } result)
                return result;

            var remaining = timeout - waited.Elapsed;
            if (remaining <= TimeSpan.Zero)
                return null;

            Thread.Sleep(remaining < interval ? remaining : interval);
        }
    }

    internal static bool Until(Func<bool> condition, TimeSpan timeout, TimeSpan interval) =>
        Until(() => condition() ? Holds : null, timeout, interval) is not null;
}
