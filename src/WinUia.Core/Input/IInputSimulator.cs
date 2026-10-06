namespace WinUia.Input;

/// <summary>
/// Physical mouse and keyboard input, in physical screen pixels. WinUia sends all of its mouse and keyboard input
/// through the <see cref="IInputSimulator"/> of the <c>AutomationContext</c>, so a test can substitute its own (for
/// example one that records what would be sent). <see cref="Win32InputSimulator"/> is the real one.
/// </summary>
public interface IInputSimulator
{
    /// <summary>The cursor position in physical screen pixels.</summary>
    (int X, int Y) GetCursorPosition();

    /// <summary>Moves the cursor to (<paramref name="x"/>, <paramref name="y"/>) at once, and makes sure it got there.</summary>
    void MoveTo(int x, int y);

    /// <summary>Moves the cursor to (<paramref name="x"/>, <paramref name="y"/>) over <paramref name="duration"/>, so the movement can be followed.</summary>
    void MoveTo(int x, int y, TimeSpan duration);

    /// <summary>Moves the cursor to (<paramref name="x"/>, <paramref name="y"/>) and clicks.</summary>
    void ClickAt(int x, int y, MouseButton button = MouseButton.Left);

    /// <summary>Types <paramref name="text"/> into the focused control.</summary>
    void SendText(string text);

    /// <summary>Presses the keys in order and releases them in reverse order, for example Control+A.</summary>
    void SendKeys(params VirtualKey[] keys);
}
