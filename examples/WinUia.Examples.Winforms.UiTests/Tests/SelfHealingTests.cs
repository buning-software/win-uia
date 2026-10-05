using WinUia.Core;
using WinUia.Core.Exceptions;
using WinUia.Examples.Winforms.UiTests.Application;

namespace WinUia.Examples.Winforms.UiTests;

/// <summary>
/// Elements re-find themselves when their control is re-created: FindFirst results through their search, FindAll
/// results by identity. The example app's "Recreate" button replaces btnVolatile; "Reverse" re-creates the pnlItems buttons
/// in reverse order.
/// </summary>
[UiTest]
public sealed class SelfHealingTests
{
    private WinFormsApp _app = null!;

    [SetUp]
    public void SetUp() => _app = App.Launch<WinFormsApp>();

    [TearDown]
    public void TearDown() => _app.Dispose();

    private IReadOnlyList<Element> Items() => _app.ItemsPanel.FindAll(e => e.ControlType == ControlType.Button, TreeScope.Children);

    private IEnumerable<string> ItemNames() => Items().Select(i => i.Name);

    private void Recreate()
    {
        _app.RecreateButton.Click();
        Eventually(() => _app.VolatileButton.Name == "Volatile 2", "Recreate replaces the volatile button");
    }

    [Test]
    public void A_found_element_re_resolves_after_its_control_is_recreated()
    {
        var volatileButton = _app.VolatileButton;
        Assert.That(volatileButton.Name, Is.EqualTo("Volatile 1"));

        Recreate();

        Assert.That(volatileButton.Name, Is.EqualTo("Volatile 2"));
    }

    [Test]
    public void FindAll_elements_re_resolve_to_the_same_control_when_siblings_move()
    {
        var items = Items();
        var namesBefore = items.Select(i => i.Name).ToArray();
        Assert.That(namesBefore, Is.EquivalentTo(["Item A", "Item B", "Item C"]));

        _app.ReverseButton.Click();
        Eventually(() => ItemNames().SequenceEqual(namesBefore.Reverse()));

        Assert.That(items.Select(i => i.Name), Is.EqualTo(namesBefore));
    }

    [Test]
    public void A_FindAll_element_never_read_before_going_stale_reports_stale_instead_of_guessing()
    {
        var items = Items();

        _app.ReverseButton.Click();
        Eventually(() => ItemNames().First() == "Item C");

        Assert.Throws<UiaStaleElementException>(() => _ = items[0].Name);
    }

    [Test]
    public void Without_stale_retries_a_recreated_control_reports_stale()
    {
        var volatileButton = _app.VolatileButton;
        Assert.That(volatileButton.Name, Is.EqualTo("Volatile 1"));
        Recreate();

        _app.Context.StaleRetryCount = 0;

        Assert.Throws<UiaStaleElementException>(() => _ = volatileButton.Name);
    }

    [Test]
    public void An_element_without_a_locator_reports_stale_after_its_control_is_recreated()
    {
        var byHandle = _app.Context.FromHandle(_app.VolatileButton.NativeWindowHandle);
        Assert.That(byHandle.Name, Is.EqualTo("Volatile 1"));

        Recreate();

        Assert.Throws<UiaStaleElementException>(() => _ = byHandle.Name);
    }

    [Test]
    public void A_found_child_re_resolves_after_it_and_its_siblings_are_recreated()
    {
        var panel = _app.ItemsPanel;
        var itemA = panel.Find(e => e.Name == "Item A", TreeScope.Children);
        Assert.That(itemA.Name, Is.EqualTo("Item A"));

        _app.ReverseButton.Click();
        Eventually(() => ItemNames().First() == "Item C");

        Assert.That(itemA.Name, Is.EqualTo("Item A"));
    }
}
