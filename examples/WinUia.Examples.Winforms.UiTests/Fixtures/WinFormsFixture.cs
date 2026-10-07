using WinUia.Examples.Winforms.UiTests.Application;
using WinUia.Examples.Winforms.UiTests.Application.Views;

namespace WinUia.Examples.Winforms.UiTests.Fixtures;

public abstract class WinFormsFixture
{
    protected WinFormsApp WinFormsApp { get; private set; } = null!;
    protected MainForm MainForm { get; private set; } = null!;

    [SetUp]
    public void LaunchApp()
    {
        WinFormsApp = App.Launch<WinFormsApp>();
        MainForm = new MainForm(WinFormsApp);
    }

    [TearDown]
    public void CloseApp() => WinFormsApp.Dispose();
}
