using System.Runtime.InteropServices;
using WinUia.Core.Interop;

namespace WinUia;

/// <summary>
/// The raw UIA element behind an <see cref="Element"/>, and the self-healing around it: every call re-finds the element
/// through its <see cref="ElementLocator"/> and retries when UIA reports it as stale.
/// </summary>
internal sealed class ElementHandle(AutomationContext context, IUIAutomationElement raw, ElementLocator? locator)
{
    private ElementIdentity? _identity;

    public AutomationContext Context { get; } = context;

    public ElementLocator? Locator { get; } = locator;

    public IUIAutomationElement Raw { get; private set; } = raw;

    /// <summary>Re-finds the element through its locator, waiting up to <see cref="AutomationContext.StaleRefreshTimeout"/> for it to reappear.</summary>
    public bool Refresh()
    {
        if (Locator is null)
            return false;

        return Context.WaitFor(() =>
        {
            try
            {
                return TryResolveNow() ? this : null;
            }
            catch (UiaStaleElementException)
            {
                return null; // Ancestor is gone too; retry on the next poll.
            }
        }, Context.StaleRefreshTimeout) is not null;
    }

    public T Get<T>(Func<IUIAutomationElement, T> call)
    {
        AutomationContext.EnsureMta();
        return Retry.OnStale(() =>
        {
            var result = call(Raw);
            CaptureIdentity();
            return result;
        }, Refresh, Context.StaleRetryCount);
    }

    public void Do(Action<IUIAutomationElement> call) =>
        Get(e => { call(e); return true; });

    // Descendants re-resolve through this: one retry, no waiting, so a stale chain is walked once per poll.
    public T GetResolvingOnce<T>(Func<IUIAutomationElement, T> call)
    {
        AutomationContext.EnsureMta();
        return Retry.OnStale(() => call(Raw), TryResolveNow, maxRetries: 1);
    }

    private bool TryResolveNow()
    {
        if (Locator?.ResolveOnce(Context, _identity) is not { } fresh)
            return false;

        Raw = fresh;
        return true;
    }

    // FindAll results re-resolve by identity, not by position, so capture it on first use.
    private void CaptureIdentity()
    {
        if (_identity is not null || Locator?.Index is null)
            return;

        try
        {
            _identity = ElementIdentity.From(Raw);
        }
        catch (COMException)
        {
            // Went stale right after the call; the next call reports it.
        }
    }
}
