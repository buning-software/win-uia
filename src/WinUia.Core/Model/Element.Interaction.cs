using WinUia.Core.Interop;
using WinUia.Input;
using WinUia.Patterns;

namespace WinUia;

public sealed partial class Element
{
    /// <summary>The Invoke pattern.</summary>
    public InvokePatternWrapper InvokePattern => new(this);

    /// <summary>The Value pattern.</summary>
    public ValuePatternWrapper ValuePattern => new(this);

    /// <summary>The Toggle pattern.</summary>
    public TogglePatternWrapper TogglePattern => new(this);

    /// <summary>The SelectionItem pattern.</summary>
    public SelectionItemPatternWrapper SelectionItemPattern => new(this);

    /// <summary>The ExpandCollapse pattern.</summary>
    public ExpandCollapsePatternWrapper ExpandCollapsePattern => new(this);

    internal ScrollItemPatternWrapper ScrollItemPattern => new(this);

    internal TextPatternWrapper TextPattern => new(this);

    /// <summary>The Window pattern.</summary>
    public WindowPatternWrapper WindowPattern => new(this);

    /// <summary>
    /// Clicks the element through the first pattern that means "click" for it: Invoke, Toggle, SelectionItem,
    /// then ExpandCollapse. Falls back to a physical mouse click (<see cref="PhysicalClick"/>).
    /// With <see cref="AutomationContext.ShowPointer"/> on, the cursor moves to the element first, as it does for the
    /// other interactions.
    /// <para>
    /// Native Win32 and WinForms buttons are clicked by posting <c>BM_CLICK</c> to their window instead of through
    /// the Invoke pattern. Their Invoke runs the click handler inside the UIA call, so a handler that shows a modal
    /// dialog would keep the application's UI thread inside that call until the dialog closes, and every other UIA
    /// request to the application would fail or time out meanwhile. A posted click runs from the application's own
    /// message loop, so the dialog can be automated like any other window.
    /// </para>
    /// </summary>
    public void Click()
    {
        EnsureEnabled();
        MovePointerHere();

        if (InvokePattern.IsSupported)
        {
            if (!TryPostNativeButtonClick())
                InvokePattern.Invoke();
        }
        else if (TogglePattern.IsSupported)
            TogglePattern.Toggle();
        else if (SelectionItemPattern.IsSupported)
            SelectionItemPattern.Select();
        else if (ExpandCollapsePattern.IsSupported)
            ToggleExpandCollapse();
        else
            PhysicalClick();
    }

    private bool TryPostNativeButtonClick()
    {
        if (ControlType != ControlType.Button)
            return false;

        var hwnd = NativeWindowHandle;
        if (hwnd == 0 || !ClassName.Contains("BUTTON", StringComparison.OrdinalIgnoreCase))
            return false;

        // Posted, not sent: runs from the app's own message loop (see Click).
        return User32.PostMessage(hwnd, User32.BM_CLICK, 0, 0);
    }

    /// <summary>Scrolls the element into view if possible and clicks its clickable point with the mouse.</summary>
    public void PhysicalClick(MouseButton button = MouseButton.Left)
    {
        EnsureEnabled();

        if (ScrollItemPattern.IsSupported)
            ScrollItemPattern.ScrollIntoView();

        var point = GetClickablePoint();
        MovePointerTo(point);
        Context.Input.ClickAt(point.X, point.Y, button);
    }

    /// <summary>
    /// A screen point that clicks the element: UIA's clickable point, or the centre of the bounding rectangle
    /// when UIA reports none but the element is visible.
    /// </summary>
    public ScreenPoint GetClickablePoint() =>
        TryGetClickablePoint() ?? throw new UiaException($"{this} has no clickable point.", HResults.UIA_E_NOCLICKABLEPOINT);

    private ScreenPoint? TryGetClickablePoint()
    {
        var (found, point) = PhysicalDpi.Run(() => Handle.Get(e => (e.GetClickablePoint(out var p), p)));
        if (found)
            return new ScreenPoint(point.x, point.y);

        var rect = BoundingRectangle;
        return !rect.IsEmpty && !IsOffscreen ? rect.Center : null;
    }

    /// <summary>
    /// Sets the element's text: through the Value pattern when it is available and writable, otherwise by
    /// focusing the element, selecting all and typing.
    /// </summary>
    public void SetValue(string value)
    {
        EnsureEnabled();
        MovePointerHere();

        if (ValuePattern.IsSupported && !ValuePattern.IsReadOnly)
        {
            ValuePattern.SetValue(value);
            return;
        }

        Focus();
        Context.Input.SendKeys(VirtualKey.Control, VirtualKey.A);
        if (value.Length == 0)
            Context.Input.SendKeys(VirtualKey.Delete);
        else
            Context.Input.SendText(value);
    }

    /// <summary>The element's text: Text pattern, then Value pattern, then Name.</summary>
    public string GetText()
    {
        if (TextPattern.IsSupported)
            return TextPattern.GetText();
        if (ValuePattern.IsSupported)
            return ValuePattern.Value;
        return Name;
    }

    /// <summary>Toggles the element (Toggle pattern).</summary>
    public void Toggle() => WhenEnabled(TogglePattern.Toggle);

    /// <summary>Selects the element (SelectionItem pattern).</summary>
    public void Select() => WhenEnabled(SelectionItemPattern.Select);

    /// <summary>Expands the element (ExpandCollapse pattern).</summary>
    public void Expand() => WhenEnabled(ExpandCollapsePattern.Expand);

    /// <summary>Collapses the element (ExpandCollapse pattern).</summary>
    public void Collapse() => WhenEnabled(ExpandCollapsePattern.Collapse);

    /// <summary>Gives the element keyboard focus.</summary>
    public void Focus()
    {
        MovePointerHere();
        Handle.Do(e => e.SetFocus());
    }

    // Shows where an interaction happens; an element with nowhere to point at is simply not shown.
    private void MovePointerHere()
    {
        if (Context.ShowPointer && TryGetClickablePoint() is { } point)
            MovePointerTo(point);
    }

    private void MovePointerTo(ScreenPoint point)
    {
        if (Context.ShowPointer)
            Context.Input.MoveTo(point.X, point.Y, Context.PointerMoveDuration);
    }

    private void ToggleExpandCollapse()
    {
        if (ExpandCollapsePattern.State == ExpandCollapseState.Collapsed)
            ExpandCollapsePattern.Expand();
        else
            ExpandCollapsePattern.Collapse();
    }

    private void EnsureEnabled()
    {
        if (!IsEnabled)
            throw new UiaException($"{this} is not enabled.", HResults.UIA_E_ELEMENTNOTENABLED);
    }

    private void WhenEnabled(Action action)
    {
        EnsureEnabled();
        MovePointerHere();
        action();
    }
}
