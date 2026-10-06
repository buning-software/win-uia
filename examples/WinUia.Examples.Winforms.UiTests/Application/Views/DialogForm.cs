namespace WinUia.Examples.Winforms.UiTests.Application.Views;

public sealed class DialogForm(Element window)
{
    public Element Window { get; } = window;
    public Element MessageLabel => Window.FindByAutomationId("lblDialogMessage");
    public Element InputBox => Window.FindByAutomationId("txtDialogInput");
    public Element OkButton => Window.FindByAutomationId("btnDialogOk");
    public Element CancelButton => Window.FindByAutomationId("btnDialogCancel");
}
