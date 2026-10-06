using System.Diagnostics;
using WinUia.Input;

namespace WinUia.Core.UiTests.Input;

public class CursorTests
{
    private readonly Win32InputSimulator _input = new();
    private (int X, int Y) _original;

    [SetUp]
    public void RememberCursor() => _original = _input.GetCursorPosition();

    [TearDown]
    public void RestoreCursor() => _input.MoveTo(_original.X, _original.Y);

    [Test]
    public void MoveTo_puts_the_cursor_where_GetCursorPosition_reports_it()
    {
        _input.MoveTo(210, 160);

        Assert.That(_input.GetCursorPosition(), Is.EqualTo((210, 160)));
    }

    [Test]
    public void A_smooth_move_ends_exactly_on_the_target_after_about_its_duration()
    {
        _input.MoveTo(200, 200);
        var stopwatch = Stopwatch.StartNew();

        _input.MoveTo(500, 350, TimeSpan.FromMilliseconds(300));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_input.GetCursorPosition(), Is.EqualTo((500, 350)));
            Assert.That(stopwatch.ElapsedMilliseconds, Is.InRange(250, 3000));
        }
    }

    [Test]
    public void A_smooth_move_to_where_the_cursor_already_is_returns_at_once()
    {
        _input.MoveTo(300, 250);
        var stopwatch = Stopwatch.StartNew();

        _input.MoveTo(300, 250, TimeSpan.FromSeconds(2));

        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500));
    }

    [Test]
    public void MoveTo_a_point_on_no_screen_throws_instead_of_leaving_the_cursor_elsewhere()
    {
        // Windows clamps the cursor to the screens, so it can never arrive at a point beyond all of them.
        var ex = Assert.Throws<InvalidOperationException>(() => _input.MoveTo(100_000, 100_000));

        Assert.That(ex.Message, Does.Contain("(100000, 100000)"));
    }
}
