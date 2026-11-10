using WinUia.Models;
using WinUia.Testing.Common;

namespace WinUia.UiTests.Model;

public sealed class LaunchableApp : App
{
    protected override string ExecutablePath => AppPaths.TestApp;

    protected override AppLaunchOptions DefaultOptions => new() { ShowPointer = true, MainWindowTimeout = TimeSpan.FromSeconds(7) };

    public string Title => MainWindow.Name;
}
