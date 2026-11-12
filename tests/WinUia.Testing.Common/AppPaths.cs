using System.Reflection;

namespace WinUia.Testing.Common;

public static class AppPaths
{
    public static string TestApp { get; } = Get("TestApp");

    private static string Get(string name) =>
        typeof(AppPaths).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(a => a.Key == $"AppPath.{name}").Value!;
}
