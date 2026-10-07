using WinUia.Core.Interop;

namespace WinUia.Patterns;

/// <summary>The Value pattern: controls with a string value, such as text boxes.</summary>
public sealed class ValuePatternWrapper : PatternWrapper
{
    internal ValuePatternWrapper(Element element)
        : base(element, PatternIds.Value, PropertyIds.IsValuePatternAvailable, "Value") { }

    /// <summary>The current value.</summary>
    public string Value => Call<IUIAutomationValuePattern, string?>(p => p.CurrentValue) ?? "";

    /// <summary>Whether the value can be changed.</summary>
    public bool IsReadOnly => Call<IUIAutomationValuePattern, bool>(p => p.CurrentIsReadOnly);

    /// <summary>Sets the value.</summary>
    public void SetValue(string value) => Call<IUIAutomationValuePattern>(p => p.SetValue(value));
}
