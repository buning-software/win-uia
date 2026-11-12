using System.Reflection;
using NUnit.Framework.Interfaces;
using WinUia.Models;
using WinUia.Recording;

namespace WinUia.Examples.Winforms.UiTests.Application;

public sealed class WinFormsApp : App
{
    protected override string ExecutablePath => typeof(WinFormsApp).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(a => a.Key == "ApplicationPath").Value!;

    protected override AppLaunchOptions DefaultOptions => new()
    {
        ShowPointer = true,
        // Other UI test assemblies may hold the desktop for their whole run.
        DesktopTimeout = TimeSpan.FromMinutes(60),
        Recording = new RecordingOptions
        {
            Name = () => TestContext.CurrentContext.Test.FullName,
            Keep = () => TestContext.CurrentContext.Result.Outcome.Status is TestStatus.Failed or TestStatus.Inconclusive,
            OnKept = path => TestContext.AddTestAttachment(path),
        },
    };
}
