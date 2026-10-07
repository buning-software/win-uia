namespace WinUia;

/// <summary>Fluent helpers for navigating and querying the element tree.</summary>
public static class ElementExtensions
{
    /// <summary>Walks up the control view from the parent to the root.</summary>
    public static IEnumerable<Element> Ancestors(this Element element)
    {
        for (var current = element.Parent; current is not null; current = current.Parent)
            yield return current;
    }
}
