using System.Diagnostics;
using WinUia.Core.Exceptions;
using WinUia.Examples.Winforms.UiTests.Application;

namespace WinUia.Examples.Winforms.UiTests.Tests;

[UiTest]
public sealed class DialogTests
{
    private WinFormsApp _app = null!;

    [SetUp]
    public void SetUp() => _app = App.Launch<WinFormsApp>();

    [Test]
    public void Opening_the_dialog_shows_a_modal_window()
    {
        var stopwatch = Stopwatch.StartNew();
        var dialog = _app.OpenDialog();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(5)), "Click must not block while the modal dialog is open");
            Assert.That(dialog.Window.Name, Is.EqualTo("WinUia Dialog"));
            Assert.That(dialog.Window.WindowPattern.IsModal, Is.True);
            Assert.That(dialog.MessageLabel.Name, Is.EqualTo("Enter a value:"));
        }
    }

    [Test]
    public void TryFindWindow_returns_null_for_a_window_that_does_not_exist()
    {
        Assert.That(_app.TryFindWindow("No such window", TimeSpan.FromMilliseconds(100)), Is.Null);
    }

    [Test]
    public void Confirming_the_dialog_reports_the_entered_value()
    {
        var dialog = _app.OpenDialog();

        dialog.InputBox.SetValue("hello");
        dialog.OkButton.Click();

        Eventually(() => _app.ResultLabel.Name == "Dialog: OK (hello)");
    }

    [Test]
    public void Cancelling_the_dialog_reports_cancel()
    {
        var dialog = _app.OpenDialog();

        dialog.CancelButton.Click();

        Eventually(() => _app.ResultLabel.Name == "Dialog: Cancel");
    }

    [Test]
    public void Closing_the_dialog_removes_it()
    {
        var dialog = _app.OpenDialog();

        dialog.CancelButton.Click();

        Eventually(() =>
        {
            try
            {
                _ = dialog.Window.Name;
                return false;
            }
            catch (UiaStaleElementException)
            {
                return true; // Closed: the window is gone and cannot be found again.
            }
        });
    }

    [TearDown]
    public void TearDown() => _app.Dispose();
}
