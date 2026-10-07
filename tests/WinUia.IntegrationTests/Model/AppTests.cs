namespace WinUia.IntegrationTests.Model;

public class AppTests
{
    [Test]
    public void Attaching_to_a_missing_process_throws_an_AppProcessException()
    {
        Assert.Throws<AppProcessException>(() => App.Attach(int.MaxValue));
    }
}
