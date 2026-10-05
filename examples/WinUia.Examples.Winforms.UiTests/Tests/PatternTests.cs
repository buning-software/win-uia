using WinUia.Core;
using WinUia.Core.Exceptions;
using WinUia.Core.Patterns;
using WinUia.Examples.Winforms.UiTests.Application;

namespace WinUia.Examples.Winforms.UiTests;

/// <summary>Control patterns, searches, tree navigation and physical input, exercised on the example app's controls.</summary>

[UiTest]
public sealed class PatternTests
{
    private WinFormsApp _app = null!;

    [SetUp]
    public void SetUp() => _app = App.Launch<WinFormsApp>();

    [TearDown]
    public void TearDown() => _app.Dispose();

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(nint hwnd, int index);

    private static bool IsTopmostWindow(nint hwnd) => (GetWindowLongPtr(hwnd, -20 /* GWL_EXSTYLE */) & 0x8 /* WS_EX_TOPMOST */) != 0;

    [Test]
    public void Unsupported_pattern_throws()
    {
        var label = _app.ResultLabel;

        Assert.That(label.InvokePattern.IsSupported, Is.False);
        Assert.Throws<UiaPatternNotSupportedException>(() => label.InvokePattern.Invoke());
    }

    [Test]
    public void Window_pattern_reports_window_state()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_app.Window.WindowPattern.IsSupported, Is.True);
            Assert.That(_app.Window.WindowPattern.VisualState, Is.EqualTo(WindowVisualState.Normal));
        }
    }

    [Test]
    public void GetText_reads_the_value_of_an_edit()
    {
        var input = _app.InputBox;
        input.SetValue("some text");

        Assert.That(input.GetText(), Is.EqualTo("some text"));
    }

    [Test]
    public void Clickable_point_is_inside_the_bounding_rectangle()
    {
        var button = _app.Button;

        var point = button.GetClickablePoint();
        var rect = button.BoundingRectangle;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.X, Is.InRange(rect.Left, rect.Right));
            Assert.That(point.Y, Is.InRange(rect.Top, rect.Bottom));
        }
    }

    [Test]
    public void Composite_conditions_search_correctly()
    {
        var both = _app.Window.Find(e => e.AutomationId == "btnClick" && e.ControlType == ControlType.Button);
        var either = _app.Window.Find(e => e.AutomationId == "nope" || e.AutomationId == "chkToggle");
        var notButton = _app.Window.FindAll(e => !(e.ControlType == ControlType.Button), TreeScope.Children);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(both.AutomationId, Is.EqualTo("btnClick"));
            Assert.That(either.AutomationId, Is.EqualTo("chkToggle"));
            Assert.That(notButton, Has.None.Matches<Element>(e => e.ControlType == ControlType.Button));
        }
    }

    [Test]
    public void Predicate_searches_find_the_expected_elements()
    {
        var button = _app.Window.Find(e => e.AutomationId == "btnClick" && e.ControlType == ControlType.Button);
        var items = _app.ItemsList.FindAll(e => e.ControlType == ControlType.ListItem && e.Name != "Beta", TreeScope.Children);
        var missing = _app.Window.TryFind(e => e.Name == "nope", timeout: TimeSpan.Zero);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.AutomationId, Is.EqualTo("btnClick"));
            Assert.That(items.Select(i => i.Name), Is.EqualTo(["Alpha", "Gamma"]));
            Assert.That(missing, Is.Null);
        }
    }

    [Test]
    public void FromPoint_and_GetFocusedElement_return_the_expected_elements()
    {
        var input = _app.InputBox;
        input.Focus();

        Eventually(() => _app.Context.GetFocusedElement().AutomationId == "txtInput", "SetFocus moves keyboard focus");
        Assert.That(_app.Context.FromPoint(_app.Button.GetClickablePoint()).AutomationId, Is.EqualTo("btnClick"));
    }

    [Test]
    public void Parent_and_Ancestors_walk_up_to_the_root()
    {
        var button = _app.Button;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Parent!.IsSameAs(_app.Window), Is.True);
            Assert.That(button.Ancestors().Last().IsSameAs(_app.Context.GetRootElement()), Is.True);
        }
    }

    [Test]
    public void AddToSelection_and_RemoveFromSelection_change_a_multi_select_list()
    {
        var list = _app.ItemsList;
        var alpha = list.Find(e => e.Name == "Alpha");
        var gamma = list.Find(e => e.Name == "Gamma");

        alpha.Select();
        gamma.SelectionItemPattern.AddToSelection();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(alpha.SelectionItemPattern.IsSelected, Is.True);
            Assert.That(gamma.SelectionItemPattern.IsSelected, Is.True);
        }

        alpha.SelectionItemPattern.RemoveFromSelection();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(alpha.SelectionItemPattern.IsSelected, Is.False);
            Assert.That(gamma.SelectionItemPattern.IsSelected, Is.True);
        }
    }

    [Test]
    public void Window_pattern_members_work()
    {
        var window = _app.Window.WindowPattern;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(window.CanMaximize, Is.True);
            Assert.That(window.CanMinimize, Is.True);
            Assert.That(window.IsModal, Is.False);
            // Compare with Win32 rather than assume the fixture's TopMost: it is the vtable slot under test here.
            Assert.That(window.IsTopmost, Is.EqualTo(IsTopmostWindow(_app.Window.NativeWindowHandle)));
        }

        // The WinForms provider returns E_NOTIMPL for WaitForInputIdle.
        Assert.Throws<UiaPatternNotSupportedException>(() => window.WaitForInputIdle(TimeSpan.FromSeconds(5)));

        window.SetVisualState(WindowVisualState.Maximized);
        Eventually(() => window.VisualState == WindowVisualState.Maximized, "the window maximizes");
        window.SetVisualState(WindowVisualState.Normal);
        Eventually(() => window.VisualState == WindowVisualState.Normal, "the window restores");
    }

    [Test]
    public void With_ShowPointer_the_cursor_moves_to_each_element_before_the_interaction()
    {
        _app.Context.ShowPointer = true;
        _app.Context.Input.MoveTo(5, 5);

        _app.Button.Click();
        var afterClick = _app.Context.Input.GetCursorPosition();
        _app.InputBox.SetValue("pointed at");
        var afterSetValue = _app.Context.Input.GetCursorPosition();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Contains(_app.Button.BoundingRectangle, afterClick), Is.True, "the cursor is on the button after Click");
            Assert.That(Contains(_app.InputBox.BoundingRectangle, afterSetValue), Is.True, "the cursor is on the text box after SetValue");
            Assert.That(_app.InputBox.ValuePattern.Value, Is.EqualTo("pointed at"));
        }

        Eventually(() => _app.ResultLabel.Name == "Clicked", "the click still goes through");
    }

    [Test]
    public void Without_ShowPointer_interactions_do_not_move_the_cursor_to_the_elements()
    {
        _app.Context.ShowPointer = false; // WinFormsApp turns it on by default.
        _app.Context.Input.MoveTo(5, 5); // Away from the app's window.

        _app.Button.Click();
        var afterClick = _app.Context.Input.GetCursorPosition();
        _app.InputBox.SetValue("not pointed at");
        var afterSetValue = _app.Context.Input.GetCursorPosition();

        // Not "the cursor stays at (5, 5)": someone using the mouse during the run would break that. What ShowPointer
        // controls is whether WinUia moves the cursor onto the elements, and nobody else does that by accident.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Contains(_app.Button.BoundingRectangle, afterClick), Is.False, "Click does not point at the button");
            Assert.That(Contains(_app.InputBox.BoundingRectangle, afterSetValue), Is.False, "SetValue does not point at the text box");
            Assert.That(_app.InputBox.ValuePattern.Value, Is.EqualTo("not pointed at"));
        }
    }

    private static bool Contains(ScreenRect rect, (int X, int Y) point) =>
        point.X >= rect.Left && point.X <= rect.Right && point.Y >= rect.Top && point.Y <= rect.Bottom;

    [Test]
    public void Physical_click_and_typing_reach_the_app()
    {
        var input = _app.InputBox;

        input.PhysicalClick();
        Eventually(() => input.HasKeyboardFocus, "a physical click focuses the text box");

        _app.Context.Input.SendText("typed");
        Eventually(() => input.ValuePattern.Value == "typed", "SendText types into the focused text box");
    }
}
