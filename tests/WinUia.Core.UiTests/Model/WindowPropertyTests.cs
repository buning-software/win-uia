using WinUia.Testing.Shared;

namespace WinUia.Core.UiTests.Model;

public class WindowPropertyTests
{
    [Test]
    public void A_window_reports_the_same_values_through_typed_properties_and_raw_UIA_properties()
    {
        using var app = App.Launch(AppPaths.TestApp);
        var window = app.MainWindow;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(window.Name, Is.EqualTo(window.Handle.Raw.GetCurrentPropertyValue(30005) ?? ""));
            Assert.That(window.AutomationId, Is.EqualTo(window.Handle.Raw.GetCurrentPropertyValue(30011) ?? ""));
            Assert.That(window.ClassName, Is.EqualTo(window.Handle.Raw.GetCurrentPropertyValue(30012) ?? ""));
            Assert.That(window.FrameworkId, Is.EqualTo(window.Handle.Raw.GetCurrentPropertyValue(30024) ?? ""));
            Assert.That((int)window.ControlType, Is.EqualTo(window.Handle.Raw.GetCurrentPropertyValue(30003)));
            Assert.That(window.ProcessId, Is.EqualTo(window.Handle.Raw.GetCurrentPropertyValue(30002)));
            Assert.That(window.IsEnabled, Is.EqualTo(window.Handle.Raw.GetCurrentPropertyValue(30010)));
            Assert.That(window.IsOffscreen, Is.EqualTo(window.Handle.Raw.GetCurrentPropertyValue(30022)));
            Assert.That((long)window.NativeWindowHandle, Is.EqualTo(Convert.ToInt64(window.Handle.Raw.GetCurrentPropertyValue(30020))));

            var bounds = (double[])window.Handle.Raw.GetCurrentPropertyValue(30001)!; // l, t, w, h
            var rect = window.BoundingRectangle;
            Assert.That(rect.Left, Is.EqualTo((int)bounds[0]));
            Assert.That(rect.Top, Is.EqualTo((int)bounds[1]));
            Assert.That(rect.Right, Is.EqualTo((int)(bounds[0] + bounds[2])));
            Assert.That(rect.Bottom, Is.EqualTo((int)(bounds[1] + bounds[3])));
        }
    }
}
