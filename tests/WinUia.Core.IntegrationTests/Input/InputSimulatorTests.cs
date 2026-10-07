namespace WinUia.Core.IntegrationTests.Input;

public class InputSimulatorTests
{
    [Test]
    public void PhysicalClick_clicks_the_elements_clickable_point_through_the_contexts_input()
    {
        var input = new RecordingInputSimulator();
        using var context = new AutomationContext();
        context.Input = input;
        var root = context.GetRootElement();
        var point = root.GetClickablePoint();

        root.PhysicalClick();

        Assert.That(input.Sent, Is.EqualTo(new[] { $"ClickAt({point.X}, {point.Y}, Left)" }));
    }

    [Test]
    public void With_ShowPointer_PhysicalClick_first_moves_the_pointer_over_PointerMoveDuration()
    {
        var input = new RecordingInputSimulator();
        using var context = new AutomationContext();
        context.Input = input;
        context.ShowPointer = true;
        context.PointerMoveDuration = TimeSpan.FromMilliseconds(250);
        var root = context.GetRootElement();
        var point = root.GetClickablePoint();

        root.PhysicalClick();

        Assert.That(input.Sent, Is.EqualTo(new[]
        {
            $"MoveTo({point.X}, {point.Y}, 250 ms)",
            $"ClickAt({point.X}, {point.Y}, Left)",
        }));
    }
}
