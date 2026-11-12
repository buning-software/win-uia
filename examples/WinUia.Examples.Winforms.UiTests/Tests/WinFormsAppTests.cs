using WinUia.Examples.Winforms.UiTests.Application;

namespace WinUia.Examples.Winforms.UiTests.Tests;

public class WinFormsAppTests
{
    private WinFormsApp? _app;

    // Disposed in teardown, not in the test: the recording asks whether the test failed when the app is disposed,
    // and the result is only final once the test body has finished.
    [TearDown]
    public void CloseApp() => _app?.Dispose();

    [Test]
    public void Application_launches_and_closes_successfully()
    {
        _app = App.Launch<WinFormsApp>();
        _app.Close();
    }
}
