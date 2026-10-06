using WinUia.Examples.Winforms.UiTests.Application;

namespace WinUia.Examples.Winforms.UiTests.Tests;

[UiTest]
public class WinFormsAppTests
{
    [Test]
    public void Application_launches_and_closes_successfully()
    {
        using var app = App.Launch<WinFormsApp>();
        app.Close();
    }
}