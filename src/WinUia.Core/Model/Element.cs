using WinUia.Core.Interop;

namespace WinUia;

/// <summary>
/// A UI Automation element. Elements found through a search remember how they were found, and every call
/// re-finds the element and retries when UIA reports it as stale (for example after a WinUI 3 re-render).
/// </summary>
public sealed partial class Element
{
    internal Element(AutomationContext context, IUIAutomationElement raw, ElementLocator? locator)
    {
        Handle = new ElementHandle(context, raw, locator);
    }

    internal ElementHandle Handle { get; }

    /// <summary>The context this element belongs to.</summary>
    public AutomationContext Context => Handle.Context;

    /// <summary>The element's Name.</summary>
    public string Name => Handle.Get(e => e.CurrentName) ?? "";

    /// <summary>The element's AutomationId.</summary>
    public string AutomationId => Handle.Get(e => e.CurrentAutomationId) ?? "";

    /// <summary>The element's ClassName.</summary>
    public string ClassName => Handle.Get(e => e.CurrentClassName) ?? "";

    /// <summary>The element's FrameworkId (for example "Win32", "WinForm", "WPF", "XAML").</summary>
    public string FrameworkId => Handle.Get(e => e.CurrentFrameworkId) ?? "";

    /// <summary>The element's control type.</summary>
    public ControlType ControlType => (ControlType)Handle.Get(e => e.CurrentControlType);

    /// <summary>The id of the process that owns the element.</summary>
    public int ProcessId => Handle.Get(e => e.CurrentProcessId);

    /// <summary>Whether the element is enabled.</summary>
    public bool IsEnabled => Handle.Get(e => e.CurrentIsEnabled);

    /// <summary>Whether the element is scrolled or hidden out of view.</summary>
    public bool IsOffscreen => Handle.Get(e => e.CurrentIsOffscreen);

    /// <summary>Whether the element has keyboard focus.</summary>
    public bool HasKeyboardFocus => Handle.Get(e => e.CurrentHasKeyboardFocus);

    /// <summary>The native window handle, or 0 when the element is not a window.</summary>
    public nint NativeWindowHandle => Handle.Get(e => e.CurrentNativeWindowHandle);

    /// <summary>The element's bounding rectangle in physical screen pixels.</summary>
    public ScreenRect BoundingRectangle
    {
        get
        {
            var r = PhysicalDpi.Run(() => Handle.Get(e => e.CurrentBoundingRectangle));
            return new ScreenRect(r.left, r.top, r.right, r.bottom);
        }
    }

    /// <summary>The UIA runtime id, which identifies the element for its lifetime.</summary>
    public IReadOnlyList<int> RuntimeId => Handle.Get(e => e.GetRuntimeId()) ?? [];

    /// <summary>
    /// Re-finds the element through the search it came from, waiting up to one second for it to reappear. Returns
    /// false when that is not possible.
    /// </summary>
    public bool Refresh() => Handle.Refresh();

    /// <summary>Whether both elements refer to the same UI element.</summary>
    public bool IsSameAs(Element other) =>
        Handle.Get(e => other.Handle.Get(o => Context.Automation.CompareElements(e, o)));

    /// <inheritdoc />
    public override string ToString()
    {
        try
        {
            return $"{ControlType} \"{Name}\" (AutomationId=\"{AutomationId}\")";
        }
        catch (UiaException)
        {
            return $"<unavailable element: {Handle.Locator}>";
        }
    }
}
