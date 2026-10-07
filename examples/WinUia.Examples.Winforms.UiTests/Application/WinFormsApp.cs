using System.Reflection;
using WinUia.Models;

namespace WinUia.Examples.Winforms.UiTests.Application;

public sealed class WinFormsApp : App
{
    protected override string ExecutablePath => typeof(WinFormsApp).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(a => a.Key == "ApplicationPath").Value!;
    protected override AppLaunchOptions DefaultOptions => new() { ShowPointer = true };
}