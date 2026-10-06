using WinUia.Examples.Winforms.UiTests.Fixtures;
using WinUia.Patterns;

namespace WinUia.Examples.Winforms.UiTests.Tests;

public sealed partial class PatternTests : WinFormsFixture
{

    [System.Runtime.InteropServices.LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static partial nint GetWindowLongPtr(nint hwnd, int index);

    private static bool IsTopmostWindow(nint hwnd) => (GetWindowLongPtr(hwnd, -20 /* GWL_EXSTYLE */) & 0x8 /* WS_EX_TOPMOST */) != 0;

    [Test]
    public void Unsupported_pattern_throws()
    {
        var label = MainForm.ResultLabel;

        Assert.That(label.InvokePattern.IsSupported, Is.False);
        Assert.Throws<UiaPatternNotSupportedException>(() => label.InvokePattern.Invoke());
    }

    [Test]
    public void Window_pattern_reports_window_state()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(MainForm.Window.WindowPattern.IsSupported, Is.True);
            Assert.That(MainForm.Window.WindowPattern.VisualState, Is.EqualTo(WindowVisualState.Normal));
        }
    }

    [Test]
    public void GetText_reads_the_value_of_an_edit()
    {
        var input = MainForm.InputBox;
        input.SetValue("some text");

        Assert.That(input.GetText(), Is.EqualTo("some text"));
    }

    [Test]
    public void Clickable_point_is_inside_the_bounding_rectangle()
    {
        var button = MainForm.Button;

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
        var both = MainForm.Window.Find(e => e.AutomationId == "btnClick" && e.ControlType == ControlType.Button);
        var either = MainForm.Window.Find(e => e.AutomationId == "nope" || e.AutomationId == "chkToggle");
        var notButton = MainForm.Window.FindAll(e => !(e.ControlType == ControlType.Button), TreeScope.Children);

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
        var button = MainForm.Window.Find(e => e.AutomationId == "btnClick" && e.ControlType == ControlType.Button);
        var items = MainForm.ItemsList.FindAll(e => e.ControlType == ControlType.ListItem && e.Name != "Beta", TreeScope.Children);
        var missing = MainForm.Window.TryFind(e => e.Name == "nope", timeout: TimeSpan.Zero);

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
        var input = MainForm.InputBox;
        input.Focus();

        Eventually(() => WinFormsApp.Context.GetFocusedElement().AutomationId == "txtInput", "SetFocus moves keyboard focus");
        Assert.That(WinFormsApp.Context.FromPoint(MainForm.Button.GetClickablePoint()).AutomationId, Is.EqualTo("btnClick"));
    }

    [Test]
    public void Parent_and_Ancestors_walk_up_to_the_root()
    {
        var button = MainForm.Button;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Parent!.IsSameAs(MainForm.Window), Is.True);
            Assert.That(button.Ancestors().Last().IsSameAs(WinFormsApp.Context.GetRootElement()), Is.True);
        }
    }

    [Test]
    public void AddToSelection_and_RemoveFromSelection_change_a_multi_select_list()
    {
        var list = MainForm.ItemsList;
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
        var window = MainForm.Window.WindowPattern;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(window.CanMaximize, Is.True);
            Assert.That(window.CanMinimize, Is.True);
            Assert.That(window.IsModal, Is.False);
            Assert.That(window.IsTopmost, Is.EqualTo(IsTopmostWindow(MainForm.Window.NativeWindowHandle)));
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
        WinFormsApp.Context.ShowPointer = true;
        WinFormsApp.Context.Input.MoveTo(5, 5);

        MainForm.Button.Click();
        var afterClick = WinFormsApp.Context.Input.GetCursorPosition();
        MainForm.InputBox.SetValue("pointed at");
        var afterSetValue = WinFormsApp.Context.Input.GetCursorPosition();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Contains(MainForm.Button.BoundingRectangle, afterClick), Is.True, "the cursor is on the button after Click");
            Assert.That(Contains(MainForm.InputBox.BoundingRectangle, afterSetValue), Is.True, "the cursor is on the text box after SetValue");
            Assert.That(MainForm.InputBox.ValuePattern.Value, Is.EqualTo("pointed at"));
        }

        Eventually(() => MainForm.ResultLabel.Name == "Clicked", "the click still goes through");
    }

    [Test]
    public void Without_ShowPointer_interactions_do_not_move_the_cursor_to_the_elements()
    {
        WinFormsApp.Context.ShowPointer = false;
        WinFormsApp.Context.Input.MoveTo(5, 5);

        MainForm.Button.Click();
        var afterClick = WinFormsApp.Context.Input.GetCursorPosition();
        MainForm.InputBox.SetValue("not pointed at");
        var afterSetValue = WinFormsApp.Context.Input.GetCursorPosition();

        // Asserts the cursor is not on the elements rather than that it stays at (5, 5), which user mouse movement would break.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Contains(MainForm.Button.BoundingRectangle, afterClick), Is.False, "Click does not point at the button");
            Assert.That(Contains(MainForm.InputBox.BoundingRectangle, afterSetValue), Is.False, "SetValue does not point at the text box");
            Assert.That(MainForm.InputBox.ValuePattern.Value, Is.EqualTo("not pointed at"));
        }
    }

    private static bool Contains(ScreenRect rect, (int X, int Y) point) =>
        point.X >= rect.Left && point.X <= rect.Right && point.Y >= rect.Top && point.Y <= rect.Bottom;

    [Test]
    public void Physical_click_and_typing_reach_the_app()
    {
        var input = MainForm.InputBox;

        input.PhysicalClick();
        Eventually(() => input.HasKeyboardFocus, "a physical click focuses the text box");

        WinFormsApp.Context.Input.SendText("typed");
        Eventually(() => input.ValuePattern.Value == "typed", "SendText types into the focused text box");
    }
}
