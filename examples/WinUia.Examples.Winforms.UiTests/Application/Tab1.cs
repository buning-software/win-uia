using WinUia.Core;

namespace WinUia.Examples.Winforms.UiTests.Application;

/// <summary>The content of the first tab page, found under the main window.</summary>
public sealed class Tab1(Element window)
{
    public Element Content => window.FindByAutomationId("lblTab1");
}
