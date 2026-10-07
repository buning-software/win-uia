using WinUia.Core.Interop;

namespace WinUia.Patterns;

/// <summary>The Window pattern: top-level and MDI windows.</summary>
public sealed class WindowPatternWrapper : PatternWrapper
{
    internal WindowPatternWrapper(Element element)
        : base(element, PatternIds.Window, PropertyIds.IsWindowPatternAvailable, "Window") { }

    /// <summary>Whether the window can be maximized.</summary>
    public bool CanMaximize => Call<IUIAutomationWindowPattern, bool>(p => p.CurrentCanMaximize);

    /// <summary>Whether the window can be minimized.</summary>
    public bool CanMinimize => Call<IUIAutomationWindowPattern, bool>(p => p.CurrentCanMinimize);

    /// <summary>Whether the window is modal.</summary>
    public bool IsModal => Call<IUIAutomationWindowPattern, bool>(p => p.CurrentIsModal);

    /// <summary>Whether the window is topmost.</summary>
    public bool IsTopmost => Call<IUIAutomationWindowPattern, bool>(p => p.CurrentIsTopmost);

    /// <summary>The window's visual state.</summary>
    public WindowVisualState VisualState => (WindowVisualState)Call<IUIAutomationWindowPattern, int>(p => p.CurrentWindowVisualState);

    /// <summary>Changes the window's visual state.</summary>
    public void SetVisualState(WindowVisualState state) => Call<IUIAutomationWindowPattern>(p => p.SetWindowVisualState((int)state));

    /// <summary>Waits until the window is ready for input. Returns false on timeout.</summary>
    public bool WaitForInputIdle(TimeSpan timeout) =>
        Call<IUIAutomationWindowPattern, bool>(p => p.WaitForInputIdle((int)timeout.TotalMilliseconds));

    /// <summary>Asks the window to close.</summary>
    public void Close() => Call<IUIAutomationWindowPattern>(p => p.Close());
}
