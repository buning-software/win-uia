namespace WinUia.Patterns;

/// <summary>Toggle state of a control (UIA <c>ToggleState</c>).</summary>
public enum ToggleState
{
    /// <summary>Not checked.</summary>
    Off = 0,
    /// <summary>Checked.</summary>
    On = 1,
    /// <summary>Neither checked nor unchecked.</summary>
    Indeterminate = 2,
}
