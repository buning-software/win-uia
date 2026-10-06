namespace WinUia.Patterns;

/// <summary>Expand/collapse state (UIA <c>ExpandCollapseState</c>).</summary>
public enum ExpandCollapseState
{
    /// <summary>Children are hidden.</summary>
    Collapsed = 0,
    /// <summary>Children are shown.</summary>
    Expanded = 1,
    /// <summary>Some children are shown.</summary>
    PartiallyExpanded = 2,
    /// <summary>The element has no children to show.</summary>
    LeafNode = 3,
}
