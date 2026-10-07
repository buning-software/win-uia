using WinUia.Core.Interop;

namespace WinUia.Patterns;

/// <summary>The Invoke pattern: controls that perform a single action, such as buttons.</summary>
public sealed class InvokePatternWrapper : PatternWrapper
{
    internal InvokePatternWrapper(Element element)
        : base(element, PatternIds.Invoke, PropertyIds.IsInvokePatternAvailable, "Invoke") { }

    /// <summary>Performs the control's action.</summary>
    public void Invoke() => Call<IUIAutomationInvokePattern>(p => p.Invoke());
}
