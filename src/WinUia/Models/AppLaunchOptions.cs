namespace WinUia.Models;

/// <summary>
/// How <see cref="App.Launch(string, AppLaunchOptions?)"/> and <see cref="App.LaunchPackaged(string, AppLaunchOptions?)"/>
/// start an application. Every option is optional: one left out keeps the page object's default (its
/// <c>DefaultOptions</c>), and otherwise WinUia's.
/// <code>
/// using var app = App.Launch&lt;MainPage&gt;(new AppLaunchOptions { Arguments = "--demo", ShowPointer = true });
/// </code>
/// </summary>
public sealed record AppLaunchOptions
{
    /// <summary>Command-line arguments for the application.</summary>
    public string? Arguments { get; init; }

    /// <summary>The directory the application starts in. Executables only: packaged apps start where Windows starts them.</summary>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    /// Environment variables to set for the application, on top of this process's own; a null value removes a variable.
    /// Executables only: packaged apps get their environment from Windows.
    /// </summary>
    public IReadOnlyDictionary<string, string?>? Environment { get; init; }

    /// <summary>
    /// Whether interactions move the cursor to each element first (<see cref="AutomationContext.ShowPointer"/>).
    /// Null keeps the default, which the <c>WINUIA_SHOW_POINTER</c> environment variable can switch on.
    /// </summary>
    public bool? ShowPointer { get; init; }

    /// <summary>How long <see cref="App.MainWindow"/> waits for the window to appear. Null keeps the default (20 seconds).</summary>
    public TimeSpan? MainWindowTimeout { get; init; }

    internal AppLaunchOptions OverriddenBy(AppLaunchOptions? overrides) => overrides is null ? this : new AppLaunchOptions
    {
        Arguments = overrides.Arguments ?? Arguments,
        WorkingDirectory = overrides.WorkingDirectory ?? WorkingDirectory,
        Environment = overrides.Environment ?? Environment,
        ShowPointer = overrides.ShowPointer ?? ShowPointer,
        MainWindowTimeout = overrides.MainWindowTimeout ?? MainWindowTimeout,
    };
}
