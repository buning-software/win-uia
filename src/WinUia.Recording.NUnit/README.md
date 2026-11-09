# WinUia.Recording.NUnit

NUnit support for [`WinUia.Recording`](https://www.nuget.org/packages/WinUia.Recording): the screen video is named after the running test, kept when the test failed, and attached to the test result.

```bash
dotnet add package WinUia.Recording.NUnit
```

Pass `NUnitTestFramework` as the test framework of the recording, for example from your app's `DefaultOptions`:

```csharp
using WinUia.Models;
using WinUia.Recording;

public sealed class MyApp : App
{
    protected override string ExecutablePath => @"C:\path\to\MyApp.exe";

    protected override AppLaunchOptions DefaultOptions => new()
    {
        Recording = new RecordingOptions { TestFramework = new NUnitTestFramework() },
    };
}
```

Dispose the app in `[TearDown]`: whether the test failed is asked when the app is disposed. ffmpeg must be installed, see the `WinUia.Recording` README.
