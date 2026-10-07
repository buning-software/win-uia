using System.Diagnostics;
using WinUia.Examples.Winforms.UiTests.Fixtures;

namespace WinUia.Examples.Winforms.UiTests.Tests;

public sealed class DialogTests : WinFormsFixture
{

    [Test]
    public void Opening_the_dialog_shows_a_modal_window()
    {
        var stopwatch = Stopwatch.StartNew();
        var dialog = MainForm.OpenDialog();

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
        Assert.That(WinFormsApp.TryFindWindow("No such window", TimeSpan.FromMilliseconds(100)), Is.Null);
    }

    [Test]
    public void Confirming_the_dialog_reports_the_entered_value()
    {
        var dialog = MainForm.OpenDialog();

        dialog.InputBox.SetValue("hello");
        dialog.OkButton.Click();

        Eventually(() => MainForm.ResultLabel.Name == "Dialog: OK (hello)");
    }

    [Test]
    public void Cancelling_the_dialog_reports_cancel()
    {
        var dialog = MainForm.OpenDialog();

        dialog.CancelButton.Click();

        Eventually(() => MainForm.ResultLabel.Name == "Dialog: Cancel");
    }

    [Test]
    public void Closing_the_dialog_removes_it()
    {
        var dialog = MainForm.OpenDialog();

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
                return true;
            }
        });
    }
}
