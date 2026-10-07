using WinUia.Core.Interop;

namespace WinUia.Patterns;

/// <summary>
/// Base for control-pattern wrappers. The pattern object is fetched for every call, so a call on an element
/// that went stale re-resolves the element and fetches the pattern again.
/// </summary>
public abstract class PatternWrapper
{
    private readonly int _patternId;
    private readonly int _availabilityPropertyId;
    private readonly string _patternName;

    private protected PatternWrapper(Element element, int patternId, int availabilityPropertyId, string patternName)
    {
        Element = element;
        _patternId = patternId;
        _availabilityPropertyId = availabilityPropertyId;
        _patternName = patternName;
    }

    /// <summary>The element the pattern belongs to.</summary>
    public Element Element { get; }

    /// <summary>Whether the element currently supports this pattern.</summary>
    public bool IsSupported => Element.Handle.Get(e => e.GetCurrentPropertyValue(_availabilityPropertyId) is true);

    private protected T Call<TPattern, T>(Func<TPattern, T> call) where TPattern : class =>
        Element.Handle.Get(e => call(Resolve<TPattern>(e)));

    private protected void Call<TPattern>(Action<TPattern> call) where TPattern : class =>
        Element.Handle.Do(e => call(Resolve<TPattern>(e)));

    // GetCurrentPattern returns null when the element does not support the pattern.
    private TPattern Resolve<TPattern>(IUIAutomationElement raw) where TPattern : class =>
        raw.GetCurrentPattern(_patternId) as TPattern
        ?? throw new UiaPatternNotSupportedException($"{Element} does not support the {_patternName} pattern.");
}
