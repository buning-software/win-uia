using System.Linq.Expressions;
using System.Runtime.InteropServices;
using WinUia.Core.Interop;
using WinUia.Input;

namespace WinUia;

/// <summary>
/// Owns the UI Automation client object and the settings used by every element created from it.
/// Must be created on an MTA thread: UIA client calls from an STA thread that pumps messages can deadlock
/// against the automated application.
/// </summary>
public sealed class AutomationContext : IDisposable
{
    private IUIAutomation? _automation;

    /// <summary>Creates the UIA client. Throws <see cref="InvalidOperationException"/> on an STA thread.</summary>
    public AutomationContext()
    {
        EnsureMta();

        // UIA and SendInput share the caller's DPI space; per-monitor awareness for the process covers UIA builds that ignore thread scopes.
        PhysicalDpi.MakeProcessAware();

        // ReSharper disable once SuspiciousTypeConversion.Global (a COM coclass: the object behind it implements the interface)
        _automation = (IUIAutomation)new CUIAutomation8();
    }

    /// <summary>How long <c>Find</c> methods wait for an element. Default 5 seconds.</summary>
    public TimeSpan DefaultTimeout { get; set; } = Poll.DefaultTimeout;

    internal TimeSpan PollingInterval { get; set; } = Poll.DefaultInterval;

    /// <summary>How many times a call on a stale element is retried after re-resolving it. Default 3.</summary>
    public int StaleRetryCount { get; set; } = 3;

    internal TimeSpan StaleRefreshTimeout { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Whether interactions first move the mouse cursor to the element, so you can follow on screen (or in a recording)
    /// where each click and text entry happens. Interactions still go through UIA patterns as usual; the move only
    /// shows the target. Off by default: each move takes about 300 ms, and hovering can show
    /// tooltips or hover effects. Starts on when the environment variable <c>WINUIA_SHOW_POINTER</c> is <c>1</c> or
    /// <c>true</c>, so a CI job can switch it on without code changes.
    /// </summary>
    public bool ShowPointer { get; set; } = Environment.GetEnvironmentVariable("WINUIA_SHOW_POINTER") is { } value
        && (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase));

    internal TimeSpan PointerMoveDuration { get; set; } = TimeSpan.FromMilliseconds(300);

    /// <summary>
    /// The mouse and keyboard every element of this context uses for physical input (<c>PhysicalClick</c>, typing when
    /// a control has no Value pattern, <see cref="ShowPointer"/>). Default the real one, <see cref="Win32InputSimulator"/>;
    /// a test can substitute its own, for example one that records what would be sent.
    /// </summary>
    public IInputSimulator Input { get; set; } = new Win32InputSimulator();

    internal IUIAutomation Automation => _automation ?? throw new ObjectDisposedException(nameof(AutomationContext));

    /// <summary>The desktop (root) element.</summary>
    public Element GetRootElement() =>
        new(this, ComExceptionMapper.Invoke(() => Automation.GetRootElement()), ElementLocator.Root);

    /// <summary>The element for a native window handle.</summary>
    public Element FromHandle(nint hwnd) =>
        new(this, ComExceptionMapper.Invoke(() => Automation.ElementFromHandle(hwnd)), locator: null);

    /// <summary>The element under a screen point (physical pixels).</summary>
    public Element FromPoint(ScreenPoint point) =>
        new(this, PhysicalDpi.Run(() => ComExceptionMapper.Invoke(() => Automation.ElementFromPoint(new tagPOINT { x = point.X, y = point.Y }))), locator: null);

    /// <summary>The element that has keyboard focus.</summary>
    public Element GetFocusedElement() =>
        new(this, ComExceptionMapper.Invoke(() => Automation.GetFocusedElement()), locator: null);

    /// <summary>
    /// Calls <paramref name="probe"/> every 100 ms until it returns non-null, and returns that
    /// result; returns null when <paramref name="timeout"/> (<see cref="DefaultTimeout"/> unless given) elapses first.
    /// The probe runs at least once, so a zero timeout means "look now".
    /// </summary>
    public T? WaitFor<T>(Func<T?> probe, TimeSpan? timeout = null) where T : class =>
        Poll.Until(probe, timeout ?? DefaultTimeout, PollingInterval);

    /// <summary>
    /// Like <see cref="WaitFor{T}"/>, but throws <see cref="UiaElementNotFoundException"/> instead of returning null:
    /// "No <paramref name="what"/> was found within N ms." <paramref name="what"/> is only evaluated on failure.
    /// </summary>
    public T WaitForOrThrow<T>(Func<T?> probe, Func<string> what, TimeSpan? timeout = null) where T : class
    {
        var effectiveTimeout = timeout ?? DefaultTimeout;
        return WaitFor(probe, effectiveTimeout)
            ?? throw new UiaElementNotFoundException($"No {what()} was found within {effectiveTimeout.TotalMilliseconds:0} ms.");
    }

    internal static void EnsureMta()
    {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        {
            throw new InvalidOperationException(
                "UI Automation must be used from an MTA thread. Run the automation code on a thread-pool thread " +
                "(for example with Task.Run) or on a thread created with ApartmentState.MTA.");
        }
    }

    internal IUIAutomationCondition CreateCondition(Expression<Func<Element, bool>> search)
    {
        EnsureMta();
        var automation = Automation;
        return ComExceptionMapper.Invoke(() => ElementPredicate.ToUia(search, automation));
    }

    /// <summary>Releases the UIA client object.</summary>
    public void Dispose()
    {
        var automation = Interlocked.Exchange(ref _automation, null);
        if (automation is not null)
            Marshal.FinalReleaseComObject(automation);
    }
}
