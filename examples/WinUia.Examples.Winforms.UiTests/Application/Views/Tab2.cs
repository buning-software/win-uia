namespace WinUia.Examples.Winforms.UiTests.Application.Views;

public class Tab2(WinFormsApp app) : MainForm(app)
{
    public Element Content => Window.FindByAutomationId("lblTab2");
}
