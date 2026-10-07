using WinUia.Examples.Winforms.UiTests.Application;
using WinUia.Examples.Winforms.UiTests.Fixtures;

namespace WinUia.Examples.Winforms.UiTests.Tests;

public sealed class TabTests : WinFormsFixture
{

    [Test]
    public void Tab_1_is_selected_at_start()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(MainForm.TabHeader("Tab 1").SelectionItemPattern.IsSelected, Is.True);
            Assert.That(MainForm.TabHeader("Tab 2").SelectionItemPattern.IsSelected, Is.False);
        }
    }

    [Test]
    public void SelectTab1_shows_the_tab_1_content()
    {
        var tab1 = MainForm.SelectTab1();

        Assert.That(tab1.Content.Name, Is.EqualTo("tab1 content"));
    }

    [Test]
    public void SelectTab2_selects_the_tab_and_shows_the_tab_2_content()
    {
        var tab2 = MainForm.SelectTab2();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(MainForm.TabHeader("Tab 2").SelectionItemPattern.IsSelected, Is.True);
            Assert.That(tab2.Content.Name, Is.EqualTo("tab2 content"));
        }
    }

    [Test]
    public void Switching_tabs_hides_the_other_tabs_content()
    {
        MainForm.SelectTab2();
        var tab1 = MainForm.SelectTab1();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tab1.Content.Name, Is.EqualTo("tab1 content"));
            Assert.That(MainForm.Window.TryFindByAutomationId("lblTab2", TimeSpan.FromMilliseconds(200)), Is.Null);
        }
    }

    [Test]
    public void The_main_form_controls_keep_working_while_another_tab_is_selected()
    {
        var tab2 = MainForm.SelectTab2();
        MainForm.Button.Click();

        using (Assert.EnterMultipleScope())
        {
            Eventually(() => MainForm.ResultLabel.Name == "Clicked");
            Assert.That(tab2.Content.Name, Is.EqualTo("tab2 content"));
        }
    }

    [Test]
    public void A_tab_can_select_another_tab()
    {
        var tab1 = MainForm.SelectTab2().SelectTab1();

        Assert.That(tab1.Content.Name, Is.EqualTo("tab1 content"));
    }
}
