using System.Diagnostics;
using WinUia.TestFrameworks.Common;

namespace WinUia.TestFrameworks.NUnit.UiTests;

public sealed class BasicFeatureTests
{
    private App _app = null!;

    [SetUp]
    public void Launch()
    {
        _app = App.Launch(ExampleApp.Path);
    }

    [TearDown]
    public void Close()
    {
        _app.Dispose();
    }

    [Test]
    public void Launch_finds_the_main_window()
    {
        Assert.That(_app.MainWindow.Name, Is.EqualTo(ExampleApp.MainWindowName));
    }

    [Test]
    public void Click_changes_the_label()
    {
        Assert.That(_app.Find(ExampleApp.ResultLabelId).Name, Is.EqualTo(ExampleApp.ReadyText));

        _app.Find(ExampleApp.ClickButtonId).Click();

        Assert.That(Poll.Until(() => _app.Find(ExampleApp.ResultLabelId).Name == ExampleApp.ClickedText), Is.True);
    }

    [Test]
    public void Dispose_closes_the_app()
    {
        var processId = _app.Process.Id;

        _app.Dispose();

        Assert.Throws<ArgumentException>(() => Process.GetProcessById(processId));
    }

    [Test]
    public void Tests_run_on_an_MTA_thread()
    {
        Assert.That(Thread.CurrentThread.GetApartmentState(), Is.EqualTo(ApartmentState.MTA));
    }
}
