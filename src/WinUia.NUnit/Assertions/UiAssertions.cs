using NUnit.Framework;

namespace WinUia.NUnit;

/// <summary>
/// Assertions for UI state that the application updates asynchronously. Import with
/// <c>using static WinUia.NUnit.UiAssertions;</c> to call <c>Eventually(...)</c> directly.
/// </summary>
public static class UiAssertions
{
    /// <summary>
    /// Fails the test unless <paramref name="condition"/> becomes true within <paramref name="timeout"/> (WinUia's
    /// default of 5 seconds unless given), polling like the library itself does (<see cref="Poll"/>).
    /// </summary>
    /// <param name="condition">The UI state to wait for, read again on every poll.</param>
    /// <param name="because">Why the condition should become true, shown when it does not.</param>
    /// <param name="timeout">How long to wait. Null keeps the default.</param>
    public static void Eventually(Func<bool> condition, string because = "", TimeSpan? timeout = null) =>
        Assert.That(Poll.Until(condition, timeout), Is.True,
            $"The condition did not become true within {(timeout ?? Poll.DefaultTimeout).TotalMilliseconds:0} ms. {because}".TrimEnd());
}
