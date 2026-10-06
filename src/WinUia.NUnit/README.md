# WinUia.NUnit

NUnit integration for WinUia: `[UiTest]` gives each UI test the desktop to itself (also across parallel test
processes) and puts it in the `UI` category; `Eventually(...)` waits for UI state the app updates asynchronously.
The package adds `using WinUia.NUnit;` and `using static WinUia.NUnit.UiAssertions;` to your project
(turn off with `<WinUiaImplicitUsings>false</WinUiaImplicitUsings>`).

```
dotnet add package WinUia.NUnit
```

```csharp
using NUnit.Framework;
using WinUia;

[assembly: Apartment(System.Threading.ApartmentState.MTA)]   // UI Automation needs MTA threads

// MyApp: your App subclass (executable path and defaults), see the WinUia README
[UiTest]
public class MainFormTests
{
    private MyApp _app = null!;

    [SetUp] public void SetUp() => _app = App.Launch<MyApp>();
    [TearDown] public void TearDown() => _app.Dispose();

    [Test]
    public void Clicking_shows_the_result()
    {
        _app.Find("btnClick").Click();
        Eventually(() => _app.Find("lblResult").Name == "Clicked");
    }
}
```

Run everything except UI tests with `dotnet test --filter "Category!=UI"`.
Documentation: https://github.com/buning-software/win-uia
