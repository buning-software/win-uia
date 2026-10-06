using WinUia.Core.Interop;

namespace WinUia.Patterns;

/// <summary>The SelectionItem pattern: selectable items, such as list items and radio buttons.</summary>
public sealed class SelectionItemPatternWrapper : PatternWrapper
{
    internal SelectionItemPatternWrapper(Element element)
        : base(element, PatternIds.SelectionItem, PropertyIds.IsSelectionItemPatternAvailable, "SelectionItem") { }

    /// <summary>Whether the item is selected.</summary>
    public bool IsSelected => Call<IUIAutomationSelectionItemPattern, bool>(p => p.CurrentIsSelected);

    /// <summary>Selects the item, deselecting others.</summary>
    public void Select() => Call<IUIAutomationSelectionItemPattern>(p => p.Select());

    /// <summary>Adds the item to the selection (multi-select containers).</summary>
    public void AddToSelection() => Call<IUIAutomationSelectionItemPattern>(p => p.AddToSelection());

    /// <summary>Removes the item from the selection.</summary>
    public void RemoveFromSelection() => Call<IUIAutomationSelectionItemPattern>(p => p.RemoveFromSelection());
}
