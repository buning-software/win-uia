using WinUia.Input;

namespace WinUia.Core.IntegrationTests.Input;

internal sealed class RecordingInputSimulator : IInputSimulator
{
    private (int X, int Y) _cursor;

    public List<string> Sent { get; } = [];

    public (int X, int Y) GetCursorPosition() => _cursor;

    public void MoveTo(int x, int y) => Record($"MoveTo({x}, {y})", x, y);

    public void MoveTo(int x, int y, TimeSpan duration) => Record($"MoveTo({x}, {y}, {duration.TotalMilliseconds:0} ms)", x, y);

    public void ClickAt(int x, int y, MouseButton button = MouseButton.Left) => Record($"ClickAt({x}, {y}, {button})", x, y);

    public void SendText(string text) => Sent.Add($"SendText({text})");

    public void SendKeys(params VirtualKey[] keys) => Sent.Add($"SendKeys({string.Join("+", keys)})");

    private void Record(string call, int x, int y)
    {
        Sent.Add(call);
        _cursor = (x, y);
    }
}
