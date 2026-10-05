using System.Reflection;
using WinUia.Core;

namespace WinUia.Examples.Winforms.UiTests.Application;

/// <summary>
/// Page object for the example WinForms app: the <see cref="App"/> itself, with the controls of its main window.
/// It knows its own executable and defaults, so tests launch it with <c>App.Launch&lt;WinFormsApp&gt;()</c>, or
/// <c>App.Launch&lt;WinFormsApp&gt;(new AppLaunchOptions { ShowPointer = false })</c> where nobody watches.
/// The tab pages and the dialog are page objects of their own, built on the elements they cover.
/// </summary>
public class WinFormsApp : App
{
    /// <inheritdoc />
    protected override string ExecutablePath => typeof(WinFormsApp).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(a => a.Key == "ApplicationPath").Value!;

    /// <summary>The cursor moves to every element a test interacts with, so you can follow a run.</summary>
    protected override AppLaunchOptions DefaultOptions => new() { ShowPointer = true };

    public Element Window => MainWindow;
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
        return new DialogForm(FindWindow("WinUia Dialog"));
    }

    // The tab header (TabItem) and its page (Pane) share the tab's text, so ask for the TabItem explicitly.
    public Element TabHeader(string text) =>
        TabControl.Find(e => e.Name == text && e.ControlType == ControlType.TabItem, TreeScope.Children);

    public Tab1 SelectTab1()
    {
        TabHeader("Tab 1").Select();
        return new Tab1(Window);
    }

    public Tab2 SelectTab2()
    {
        TabHeader("Tab 2").Select();
        return new Tab2(Window);
    }
}
