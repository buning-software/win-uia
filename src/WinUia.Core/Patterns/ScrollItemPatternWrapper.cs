using WinUia.Core.Interop;

namespace WinUia.Patterns;

internal sealed class ScrollItemPatternWrapper : PatternWrapper
{
    internal ScrollItemPatternWrapper(Element element)
        : base(element, PatternIds.ScrollItem, PropertyIds.IsScrollItemPatternAvailable, "ScrollItem") { }

    internal void ScrollIntoView() => Call<IUIAutomationScrollItemPattern>(p => p.ScrollIntoView());
}
