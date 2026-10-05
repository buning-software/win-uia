using WinUia.Core;

namespace WinUia.Examples.Winforms.UiTests.Application;

/// <summary>The content of the second tab page, found under the main window.</summary>
public class Tab2(Element window) : WinFormsApp
{
    public Element Content => window.FindByAutomationId("lblTab2");
}
