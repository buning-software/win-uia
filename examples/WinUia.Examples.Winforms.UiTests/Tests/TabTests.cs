using WinUia.Examples.Winforms.UiTests.Application;

namespace WinUia.Examples.Winforms.UiTests;

[UiTest]
public sealed class TabTests
{
    private WinFormsApp _app = null!;

    [SetUp]
    public void SetUp() => _app = App.Launch<WinFormsApp>();

    [Test]
    public void Tab_1_is_selected_at_start()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_app.TabHeader("Tab 1").SelectionItemPattern.IsSelected, Is.True);
            Assert.That(_app.TabHeader("Tab 2").SelectionItemPattern.IsSelected, Is.False);
        }
    }

    [Test]
    public void SelectTab1_shows_the_tab_1_content()
    {
        var tab1 = _app.SelectTab1();

        Assert.That(tab1.Content.Name, Is.EqualTo("tab1 content"));
    }

    [Test]
    public void SelectTab2_selects_the_tab_and_shows_the_tab_2_content()
    {
        var tab2 = _app.SelectTab2();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_app.TabHeader("Tab 2").SelectionItemPattern.IsSelected, Is.True);
            Assert.That(tab2.Content.Name, Is.EqualTo("tab2 content"));
        }
    }

    [Test]
    public void Switching_tabs_hides_the_other_tabs_content()
    {
        _app.SelectTab2();
        var tab1 = _app.SelectTab1();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tab1.Content.Name, Is.EqualTo("tab1 content"));
            Assert.That(_app.Window.TryFindByAutomationId("lblTab2", TimeSpan.FromMilliseconds(200)), Is.Null);
        }
    }

    [Test]
    public void The_main_form_controls_keep_working_while_another_tab_is_selected()
    {
        var tab2 = _app.SelectTab2();

        _app.Button.Click();

        using (Assert.EnterMultipleScope())
        {
            Eventually(() => _app.ResultLabel.Name == "Clicked");
            Assert.That(tab2.Content.Name, Is.EqualTo("tab2 content"));
        }
    }

    [TearDown]
    public void TearDown() => _app.Dispose();
}
