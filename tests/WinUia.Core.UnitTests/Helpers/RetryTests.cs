using System.Runtime.InteropServices;

namespace WinUia.Core.UnitTests.Helpers;

public class RetryTests
{
    private static COMException Stale() => new("stale", unchecked((int)0x80040201));

    [Test]
    public void OnStale_refreshes_and_retries_until_success()
    {
        var calls = 0;
        var refreshes = 0;

        var result = Retry.OnStale(() => ++calls < 3 ? throw Stale() : "ok", () => { refreshes++; return true; }, maxRetries: 3);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo("ok"));
            Assert.That(calls, Is.EqualTo(3));
            Assert.That(refreshes, Is.EqualTo(2));
        }
    }

    [Test]
    public void OnStale_rethrows_when_refresh_is_not_possible()
    {
        var calls = 0;

        Assert.Throws<UiaStaleElementException>(() =>
            Retry.OnStale<int>(() => { calls++; throw Stale(); }, () => false, maxRetries: 3));
        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public void OnStale_gives_up_after_max_retries()
    {
        var calls = 0;

        Assert.Throws<UiaStaleElementException>(() =>
            Retry.OnStale<int>(() => { calls++; throw Stale(); }, () => true, maxRetries: 3));
        Assert.That(calls, Is.EqualTo(4));
    }

    [Test]
    public void OnStale_does_not_retry_other_failures()
    {
        var calls = 0;

        Assert.Throws<UiaException>(() =>
            Retry.OnStale<int>(() => { calls++; throw new COMException("x", unchecked((int)0x80040200)); }, () => true, maxRetries: 3));
        Assert.That(calls, Is.EqualTo(1));
    }
}
