namespace WinUia.Examples.Winforms.UiTests.Application.Views;

public class Tab1(WinFormsApp app) : MainForm(app)
{
    public Element Content => Window.FindByAutomationId("lblTab1");
}
