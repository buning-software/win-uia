using System.Diagnostics;
using WinUia.TestFrameworks.Common;

namespace WinUia.TestFrameworks.XUnit.UiTests;

public sealed class BasicFeatureTests : IDisposable
{
    private readonly App _app = App.Launch(ExampleApp.Path);

    public void Dispose()
    {
        _app.Dispose();
    }

    [Fact]
    public void Launch_finds_the_main_window()
    {
        Assert.Equal(ExampleApp.MainWindowName, _app.MainWindow.Name);
    }

    [Fact]
    public void Click_changes_the_label()
    {
        Assert.Equal(ExampleApp.ReadyText, _app.Find(ExampleApp.ResultLabelId).Name);

        _app.Find(ExampleApp.ClickButtonId).Click();

        Assert.True(Poll.Until(() => _app.Find(ExampleApp.ResultLabelId).Name == ExampleApp.ClickedText));
    }

    [Fact]
    public void Dispose_closes_the_app()
    {
        var processId = _app.Process.Id;

        _app.Dispose();

        Assert.Throws<ArgumentException>(() => Process.GetProcessById(processId));
    }

    [Fact]
    public void Tests_run_on_an_MTA_thread()
    {
        Assert.Equal(ApartmentState.MTA, Thread.CurrentThread.GetApartmentState());
    }
}
