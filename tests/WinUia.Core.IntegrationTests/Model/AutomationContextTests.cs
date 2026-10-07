using System.Diagnostics;

namespace WinUia.Core.IntegrationTests.Model;

public class AutomationContextTests
{
    private static Exception? RunOnStaThread(Action action)
    {
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { caught = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        return caught;
    }

    [Test]
    public void Test_threads_are_MTA()
    {
        Assert.That(Thread.CurrentThread.GetApartmentState(), Is.EqualTo(ApartmentState.MTA));
    }

    [Test]
    public void Constructing_on_an_STA_thread_throws()
    {
        var caught = RunOnStaThread(() => { using var _ = new AutomationContext(); });

        Assert.That(caught, Is.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Using_an_element_from_an_STA_thread_throws()
    {
        using var context = new AutomationContext();
        var root = context.GetRootElement();

        var caught = RunOnStaThread(() => _ = root.Name);

        Assert.That(caught, Is.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Root_element_is_the_desktop()
    {
        using var context = new AutomationContext();

        var root = context.GetRootElement();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(root.ClassName, Is.EqualTo("#32769"));
            Assert.That(root.ControlType, Is.EqualTo(ControlType.Pane));
            Assert.That(root.BoundingRectangle.Width, Is.GreaterThan(0));
        }
    }

    [Test]
    public void WaitFor_returns_as_soon_as_the_probe_succeeds()
    {
        using var context = new AutomationContext();
        context.PollingInterval = TimeSpan.FromMilliseconds(10);
        var calls = 0;

        var result = context.WaitFor(() => ++calls == 3 ? "found" : null, TimeSpan.FromSeconds(5));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo("found"));
            Assert.That(calls, Is.EqualTo(3));
        }
    }

    [Test]
    public void WaitFor_returns_null_after_the_timeout()
    {
        using var context = new AutomationContext();
        context.PollingInterval = TimeSpan.FromMilliseconds(20);
        var stopwatch = Stopwatch.StartNew();

        var result = context.WaitFor<string>(() => null, TimeSpan.FromMilliseconds(200));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Null);
            Assert.That(stopwatch.ElapsedMilliseconds, Is.InRange(190, 2000));
        }
    }

    [Test]
    public void WaitFor_with_zero_timeout_probes_once()
    {
        using var context = new AutomationContext();
        var calls = 0;

        Assert.That(context.WaitFor<string>(() => { calls++; return null; }, TimeSpan.Zero), Is.Null);
        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public void ShowPointer_is_off_by_default_and_on_when_WINUIA_SHOW_POINTER_is_set()
    {
        var original = Environment.GetEnvironmentVariable("WINUIA_SHOW_POINTER");
        try
        {
            Environment.SetEnvironmentVariable("WINUIA_SHOW_POINTER", null);
            using var off = new AutomationContext();
            Environment.SetEnvironmentVariable("WINUIA_SHOW_POINTER", "true");
            using var on = new AutomationContext();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(off.ShowPointer, Is.False);
                Assert.That(on.ShowPointer, Is.True);
                Assert.That(on.PointerMoveDuration, Is.EqualTo(TimeSpan.FromMilliseconds(300)));
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("WINUIA_SHOW_POINTER", original);
        }
    }

    [Test]
    public void Using_a_disposed_context_throws()
    {
        var context = new AutomationContext();
        context.Dispose();

        Assert.Throws<ObjectDisposedException>(() => context.GetRootElement());
    }
}
