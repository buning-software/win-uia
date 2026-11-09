using System.Reflection;
using WinUia.Models;
using WinUia.Recording;

namespace WinUia.Examples.Winforms.UiTests.Application;

public sealed class WinFormsApp : App
{
    protected override string ExecutablePath => typeof(WinFormsApp).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(a => a.Key == "ApplicationPath").Value!;

    // Records every launch; the video of a failed test is kept and attached to its result. Needs ffmpeg, without it the tests run unrecorded.
    protected override AppLaunchOptions DefaultOptions => new() { ShowPointer = true, Recording = new RecordingOptions { TestFramework = new NUnitTestFramework() } };
}
