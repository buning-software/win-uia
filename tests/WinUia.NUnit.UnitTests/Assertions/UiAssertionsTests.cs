using NUnit.Framework.Internal;

namespace WinUia.NUnit.UnitTests.Assertions;

public class UiAssertionsTests
{
    [Test]
    public void Eventually_passes_as_soon_as_the_condition_holds()
    {
        var calls = 0;

        Eventually(() => ++calls == 3);

        Assert.That(calls, Is.EqualTo(3));
    }

    [Test]
    public void Eventually_fails_with_the_reason_when_the_condition_never_holds()
    {
        string? message;
        using (new TestExecutionContext.IsolatedContext())
        {
            var ex = Assert.Throws<AssertionException>(() => Eventually(() => false, "the label never changes"));
            message = ex.Message;
        }

        Assert.That(message, Does.Contain("the label never changes"));
    }
}
