using WinUia.Examples.Winforms.UiTests.Fixtures;
using WinUia.Patterns;

namespace WinUia.Examples.Winforms.UiTests.Tests;

public sealed class MainFormTests : WinFormsFixture
{

    [SetUp]
    public void SelectTab() => MainForm.SelectTab1();

    [Test]
    public void Launch_returns_the_main_form()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(MainForm.Window.Name, Is.EqualTo("WinUia Test App"));
            Assert.That(MainForm.Window.ProcessId, Is.EqualTo(WinFormsApp.Process.Id));
        }
    }

    [Test]
    public void Find_falls_back_to_the_name()
    {
        Assert.That(WinFormsApp.Find("Click me").AutomationId, Is.EqualTo("btnClick"));
    }

    [Test]
    public void The_form_starts_in_its_initial_state()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(MainForm.ResultLabel.Name, Is.EqualTo("Ready"));
            Assert.That(MainForm.InputBox.ValuePattern.Value, Is.Empty);
            Assert.That(MainForm.ToggleCheckBox.TogglePattern.State, Is.EqualTo(ToggleState.Off));
            Assert.That(MainForm.VolatileButton.Name, Is.EqualTo("Volatile 1"));
            Assert.That(ItemNames(), Is.EqualTo(["Item A", "Item B", "Item C"]));
            Assert.That(MainForm.ItemsList.FindAll(e => e.ControlType == ControlType.ListItem, TreeScope.Children).Select(i => i.Name), Is.EqualTo(["Alpha", "Beta", "Gamma"]));
        }
    }

    [Test]
    public void Selecting_a_list_item_selects_it()
    {
        var beta = MainForm.ItemsList.Find(e => e.Name == "Beta");

        beta.Select();

        Assert.That(beta.SelectionItemPattern.IsSelected, Is.True);
    }

    [Test]
    public void Recreate_increments_the_volatile_button_generation()
    {
        MainForm.RecreateButton.Click();
        Eventually(() => MainForm.VolatileButton.Name == "Volatile 2");

        MainForm.RecreateButton.Click();
        Eventually(() => MainForm.VolatileButton.Name == "Volatile 3");
    }

    [Test]
    public void Reverse_twice_restores_the_item_order()
    {
        MainForm.ReverseButton.Click();
        Eventually(() => ItemNames().SequenceEqual(["Item C", "Item B", "Item A"]));

        MainForm.ReverseButton.Click();
        Eventually(() => ItemNames().SequenceEqual(["Item A", "Item B", "Item C"]));
    }

    [Test]
    public void Clicking_the_button_updates_the_label()
    {
        MainForm.Button.Click();

        Eventually(() => MainForm.ResultLabel.Name == "Clicked");
    }

    [Test]
    public void SetValue_round_trips()
    {
        var input = MainForm.InputBox;

        input.SetValue("round trip");

        Assert.That(input.ValuePattern.Value, Is.EqualTo("round trip"));
    }

    [Test]
    public void Toggle_changes_the_checkbox()
    {
        var toggle = MainForm.ToggleCheckBox;

        toggle.Toggle();

        Assert.That(toggle.TogglePattern.State, Is.EqualTo(ToggleState.On));
    }

    private IEnumerable<string> ItemNames() => MainForm.ItemsPanel.FindAll(e => e.ControlType == ControlType.Button, TreeScope.Children).Select(i => i.Name);
}
