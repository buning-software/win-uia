using WinUia.Core.Interop;

namespace WinUia.Patterns;

/// <summary>The Toggle pattern: controls that cycle through states, such as check boxes.</summary>
public sealed class TogglePatternWrapper : PatternWrapper
{
    internal TogglePatternWrapper(Element element)
        : base(element, PatternIds.Toggle, PropertyIds.IsTogglePatternAvailable, "Toggle") { }

    /// <summary>The current toggle state.</summary>
    public ToggleState State => (ToggleState)Call<IUIAutomationTogglePattern, int>(p => p.CurrentToggleState);

    /// <summary>Moves to the next toggle state.</summary>
    public void Toggle() => Call<IUIAutomationTogglePattern>(p => p.Toggle());
}
