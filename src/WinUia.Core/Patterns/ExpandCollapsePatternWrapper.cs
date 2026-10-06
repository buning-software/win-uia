using WinUia.Core.Interop;

namespace WinUia.Patterns;

/// <summary>The ExpandCollapse pattern: combo boxes, menus, tree items.</summary>
public sealed class ExpandCollapsePatternWrapper : PatternWrapper
{
    internal ExpandCollapsePatternWrapper(Element element)
        : base(element, PatternIds.ExpandCollapse, PropertyIds.IsExpandCollapsePatternAvailable, "ExpandCollapse") { }

    /// <summary>The current state.</summary>
    public ExpandCollapseState State =>
        (ExpandCollapseState)Call<IUIAutomationExpandCollapsePattern, int>(p => p.CurrentExpandCollapseState);

    /// <summary>Shows the element's children.</summary>
    public void Expand() => Call<IUIAutomationExpandCollapsePattern>(p => p.Expand());

    /// <summary>Hides the element's children.</summary>
    public void Collapse() => Call<IUIAutomationExpandCollapsePattern>(p => p.Collapse());
}
