using WinUia.Core;
using WinUia.Core.Patterns;
using WinUia.Examples.Winforms.UiTests.Application;

namespace WinUia.Examples.Winforms.UiTests.Tests;

[UiTest]
public sealed class MainFormTests
{
    private WinFormsApp _app = null!;

    [SetUp]
    public void SetUp() => _app = App.Launch<WinFormsApp>();
    
    [Test]
    public void Launch_returns_the_main_form()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_app.Window.Name, Is.EqualTo("WinUia Test App"));
            Assert.That(_app.Window.ProcessId, Is.EqualTo(_app.Process.Id));
        }
    }

    [Test]
    public void Find_falls_back_to_the_name()
    {
        Assert.That(_app.Find("Click me").AutomationId, Is.EqualTo("btnClick"));
    }

    [Test]
    public void The_form_starts_in_its_initial_state()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_app.ResultLabel.Name, Is.EqualTo("Ready"));
            Assert.That(_app.InputBox.ValuePattern.Value, Is.Empty);
            Assert.That(_app.ToggleCheckBox.TogglePattern.State, Is.EqualTo(ToggleState.Off));
            Assert.That(_app.VolatileButton.Name, Is.EqualTo("Volatile 1"));
            Assert.That(ItemNames(), Is.EqualTo(["Item A", "Item B", "Item C"]));
            Assert.That(_app.ItemsList.FindAll(e => e.ControlType == ControlType.ListItem, TreeScope.Children).Select(i => i.Name), Is.EqualTo(["Alpha", "Beta", "Gamma"]));
        }
    }

    [Test]
    public void Selecting_a_list_item_selects_it()
    {
        var beta = _app.ItemsList.Find(e => e.Name == "Beta");

        beta.Select();

        Assert.That(beta.SelectionItemPattern.IsSelected, Is.True);
    }

    [Test]
    public void Recreate_increments_the_volatile_button_generation()
    {
        _app.RecreateButton.Click();
        Eventually(() => _app.VolatileButton.Name == "Volatile 2");

        _app.RecreateButton.Click();
        Eventually(() => _app.VolatileButton.Name == "Volatile 3");
    }

    [Test]
    public void Reverse_twice_restores_the_item_order()
    {
        _app.ReverseButton.Click();
        Eventually(() => ItemNames().SequenceEqual(["Item C", "Item B", "Item A"]));

        _app.ReverseButton.Click();
        Eventually(() => ItemNames().SequenceEqual(["Item A", "Item B", "Item C"]));
    }

    [Test]
    public void Clicking_the_button_updates_the_label()
    {
        _app.Button.Click();

        Eventually(() => _app.ResultLabel.Name == "Clicked");
    }

    [Test]
    public void SetValue_round_trips()
    {
        var input = _app.InputBox;

        input.SetValue("round trip");

        Assert.That(input.ValuePattern.Value, Is.EqualTo("round trip"));
    }

    [Test]
    public void Toggle_changes_the_checkbox()
    {
        var toggle = _app.ToggleCheckBox;

        toggle.Toggle();

        Assert.That(toggle.TogglePattern.State, Is.EqualTo(ToggleState.On));
    }

    [TearDown]
    public void TearDown() => _app.Dispose();

    private IEnumerable<string> ItemNames() => _app.ItemsPanel.FindAll(e => e.ControlType == ControlType.Button, TreeScope.Children).Select(i => i.Name);
}
