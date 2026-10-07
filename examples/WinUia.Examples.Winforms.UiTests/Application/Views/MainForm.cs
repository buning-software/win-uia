namespace WinUia.Examples.Winforms.UiTests.Application.Views;

public class MainForm(WinFormsApp app)
{
    public Element Window => app.MainWindow;
    public Element Button => Window.FindByAutomationId("btnClick");
    public Element ResultLabel => Window.FindByAutomationId("lblResult");
    public Element InputBox => Window.FindByAutomationId("txtInput");
    public Element ToggleCheckBox => Window.FindByAutomationId("chkToggle");
    public Element ItemsList => Window.FindByAutomationId("lstItems");
    public Element RecreateButton => Window.FindByAutomationId("btnRecreate");
    public Element VolatileButton => Window.FindByAutomationId("btnVolatile");
    public Element ItemsPanel => Window.FindByAutomationId("pnlItems");
    public Element ReverseButton => Window.FindByAutomationId("btnReverse");
    public Element OpenDialogButton => Window.FindByAutomationId("btnOpenDialog");
    public Element TabControl => Window.FindByAutomationId("tabMain");

    public DialogForm OpenDialog()
    {
        OpenDialogButton.Click();
        return new DialogForm(app.FindWindow("WinUia Dialog"));
    }

    public Element TabHeader(string text) =>
        TabControl.Find(e => e.Name == text && e.ControlType == ControlType.TabItem, TreeScope.Children);

    public Tab1 SelectTab1()
    {
        TabHeader("Tab 1").Select();
        return new Tab1(app);
    }

    public Tab2 SelectTab2()
    {
        TabHeader("Tab 2").Select();
        return new Tab2(app);
    }
}
