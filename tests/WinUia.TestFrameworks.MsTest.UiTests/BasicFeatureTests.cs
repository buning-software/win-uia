using System.Diagnostics;
using WinUia.TestFrameworks.Common;

namespace WinUia.TestFrameworks.MsTest.UiTests;

[TestClass]
public sealed class BasicFeatureTests
{
    private App _app = null!;

    [TestInitialize]
    public void Launch()
    {
        _app = App.Launch(ExampleApp.Path);
    }

    [TestCleanup]
    public void Close()
    {
        _app.Dispose();
    }

    [TestMethod]
    public void Launch_finds_the_main_window()
    {
        Assert.AreEqual(ExampleApp.MainWindowName, _app.MainWindow.Name);
    }

    [TestMethod]
    public void Click_changes_the_label()
    {
        Assert.AreEqual(ExampleApp.ReadyText, _app.Find(ExampleApp.ResultLabelId).Name);

        _app.Find(ExampleApp.ClickButtonId).Click();

        Assert.IsTrue(Poll.Until(() => _app.Find(ExampleApp.ResultLabelId).Name == ExampleApp.ClickedText));
    }

    [TestMethod]
    public void Dispose_closes_the_app()
    {
        var processId = _app.Process.Id;

        _app.Dispose();

        Assert.ThrowsExactly<ArgumentException>(() => Process.GetProcessById(processId));
    }

    [TestMethod]
    public void Tests_run_on_an_MTA_thread()
    {
        Assert.AreEqual(ApartmentState.MTA, Thread.CurrentThread.GetApartmentState());
    }
}
