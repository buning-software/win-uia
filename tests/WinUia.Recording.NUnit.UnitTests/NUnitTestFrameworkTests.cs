namespace WinUia.Recording.NUnit.UnitTests;

public class NUnitTestFrameworkTests
{
    private readonly NUnitTestFramework _framework = new();

    [Test]
    public void The_running_test_is_named_by_its_class_and_method()
    {
        Assert.That(_framework.CurrentTestName, Is.EqualTo($"{nameof(NUnitTestFrameworkTests)}.{TestContext.CurrentContext.Test.Name}"));
        Assert.That(_framework.CurrentTestName, Does.EndWith(nameof(The_running_test_is_named_by_its_class_and_method)));
    }

    [Test]
    public void A_test_that_is_still_running_is_not_reported_as_failed()
    {
        Assert.That(_framework.CurrentTestFailed, Is.Not.True);
    }

    [Test]
    public void Attaching_adds_the_file_to_the_test_result()
    {
        var file = Path.Combine(Path.GetTempPath(), $"winuia-{Guid.NewGuid():N}.txt");
        File.WriteAllText(file, "attachment");

        try
        {
            Assert.DoesNotThrow(() => _framework.Attach(file, "Screen recording"));
        }
        finally
        {
            File.Delete(file);
        }
    }
}
