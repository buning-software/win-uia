using WinUia.Core.Interop;

namespace WinUia.Patterns;

internal sealed class TextPatternWrapper : PatternWrapper
{
    internal TextPatternWrapper(Element element)
        : base(element, PatternIds.Text, PropertyIds.IsTextPatternAvailable, "Text") { }

    internal string GetText() =>
        Call<IUIAutomationTextPattern, string?>(p => p.DocumentRange.GetText(-1)) ?? "";
}
