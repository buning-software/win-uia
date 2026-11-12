using WinUia.Models;
using WinUia.Testing.Common;

namespace WinUia.UiTests.Model;

public sealed class LaunchableApp : App
{
    protected override string ExecutablePath => AppPaths.TestApp;

    // A new instance per launch is intended: the options are per-launch values, not shared state.
    #pragma warning disable WFO2000
    protected override AppLaunchOptions DefaultOptions => new() { ShowPointer = true, MainWindowTimeout = TimeSpan.FromSeconds(7) };
    #pragma warning restore WFO2000

    public string Title => MainWindow.Name;
}
