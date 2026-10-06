namespace WinUia;

/// <summary>Which part of the UI Automation tree a search covers (UIA <c>TreeScope</c>).</summary>
[Flags]
public enum TreeScope
{
    /// <summary>The element itself.</summary>
    Element = 0x1,
    /// <summary>The element's direct children.</summary>
    Children = 0x2,
    /// <summary>All of the element's descendants.</summary>
    Descendants = 0x4,
    /// <summary>The element and all of its descendants.</summary>
    Subtree = Element | Children | Descendants,
    /// <summary>The element's parent.</summary>
    Parent = 0x8,
    /// <summary>All of the element's ancestors.</summary>
    Ancestors = 0x10,
}
