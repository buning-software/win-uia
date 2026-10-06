using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace WinUia.NUnit;

/// <summary>
/// Marks UI tests: tests that launch an application and drive it with WinUia.
/// <list type="bullet">
/// <item>Each test gets the desktop to itself. The lock is taken before the test's <c>[SetUp]</c> and released after
/// its <c>[TearDown]</c>, and it holds across test processes, so parallel test assemblies do not fight over the
/// mouse, keyboard focus or foreground window.</item>
/// <item>Each test is put in the <c>UI</c> category, so <c>dotnet test --filter "Category!=UI"</c> skips them on a
/// machine without an interactive desktop.</item>
/// </list>
/// Put it on a test class, a test method or the assembly (<c>[assembly: UiTest]</c>).
/// <code>
/// [UiTest]
/// public class MainPageTests
/// {
///     private MainPage _page = null!;
///
///     [SetUp] public void SetUp() => _page = App.Launch&lt;MainPage&gt;();
///     [TearDown] public void TearDown() => _page.Dispose();
/// }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
public sealed class UiTestAttribute : NUnitAttribute, ITestAction, IApplyToTest
{
    /// <summary>The category UI tests are put in.</summary>
    public const string Category = "UI";

    // Tracked per test and shared by all attribute instances: [UiTest] on both the assembly and a class makes two
    // instances call BeforeTest for the same test, and the second must not wait for the lock the first one holds.
    private static readonly Dictionary<string, (DesktopLock Lock, int Count)> Held = [];
    private static readonly Lock HeldGate = new();

    /// <summary>How long a test waits for another process to give the desktop back. Default 5 minutes.</summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <inheritdoc />
    public ActionTargets Targets => ActionTargets.Test;

    /// <inheritdoc />
    public void ApplyToTest(Test test)
    {
        if (!test.Properties[PropertyNames.Category].Contains(Category))
            test.Properties.Add(PropertyNames.Category, Category);
    }

    /// <inheritdoc />
    public void BeforeTest(ITest test)
    {
        lock (HeldGate)
        {
            if (Held.TryGetValue(test.Id, out var held))
            {
                Held[test.Id] = (held.Lock, held.Count + 1);
                return;
            }
        }

        // Wait outside the gate: other tests (in this process or another) may hold the desktop for a while.
        var desktop = DesktopLock.Acquire(TimeSpan.FromSeconds(TimeoutSeconds));
        lock (HeldGate)
            Held[test.Id] = (desktop, 1);
    }

    /// <inheritdoc />
    public void AfterTest(ITest test)
    {
        DesktopLock? release = null;
        lock (HeldGate)
        {
            if (!Held.TryGetValue(test.Id, out var held))
                return;

            if (held.Count > 1)
            {
                Held[test.Id] = (held.Lock, held.Count - 1);
            }
            else
            {
                Held.Remove(test.Id);
                release = held.Lock;
            }
        }

        release?.Dispose();
    }
}
