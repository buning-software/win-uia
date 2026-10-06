using System.Diagnostics;
using System.Linq.Expressions;
using WinUia.Models;

namespace WinUia;

/// <summary>
/// An application under automation: its process, its own <see cref="AutomationContext"/> and its windows.
/// <code>
/// using var app = App.Launch(@"C:\path\to\MyApp.exe");
/// app.Find("btnOk").Click();
/// </code>
/// An application can have its own class that derives from <see cref="App"/> and says which executable it is and how it
/// starts by default; the generic factories create it already connected: <c>App.Launch&lt;MyApp&gt;()</c>. What is on
/// screen is described by page objects: plain classes built on the app or an <see cref="Element"/>, for example
/// <c>new SettingsDialog(app.FindWindow("Settings"))</c>.
/// <para>
/// Must be used from an MTA thread (see <see cref="AutomationContext"/>); creating it on an STA thread throws.
/// Disposing an app that was launched closes it; disposing an attached app leaves it running.
/// </para>
/// </summary>
public class App : IDisposable
{
    private Process? _process;
    private AutomationContext? _context;
    private bool _ownsProcess;
    private bool _disposed;

    /// <summary>
    /// For derived types, which are created by the generic factories such as
    /// <see cref="Launch{TApp}()"/>; those connect the instance to the application. The constructor
    /// runs before the application starts, so it cannot use <see cref="MainWindow"/> or <see cref="Process"/>.
    /// </summary>
    protected App() { }

    /// <summary>
    /// The executable a page object launches when <see cref="Launch{TApp}()"/> is called without a
    /// path. Null (the default) means the page object does not know its executable, so a path must be passed.
    /// </summary>
    protected virtual string? ExecutablePath => null;

    /// <summary>
    /// How a page object is launched or attached unless the caller says otherwise: the options passed to a factory
    /// override these property by property, and an option left empty in both keeps WinUia's default. Attaching uses
    /// only <see cref="AppLaunchOptions.ShowPointer"/> and <see cref="AppLaunchOptions.MainWindowTimeout"/>.
    /// </summary>
    protected virtual AppLaunchOptions DefaultOptions => new();

    /// <summary>Starts a classic executable. Throws <see cref="AppProcessException"/> when it cannot be started.</summary>
    public static App Launch(string path, AppLaunchOptions? options = null) =>
        Start(new App(), effective => AppLauncher.LaunchExe(path, effective), options);

    /// <summary>
    /// Starts a classic executable as <typeparamref name="TApp"/>, a page object that derives from <see cref="App"/>:
    /// <c>App.Launch&lt;MainPage&gt;(path)</c>. Throws <see cref="AppProcessException"/> when it cannot be started.
    /// </summary>
    public static TApp Launch<TApp>(string path, AppLaunchOptions? options = null) where TApp : App, new() =>
        Start(new TApp(), effective => AppLauncher.LaunchExe(path, effective), options);

    /// <summary>
    /// Starts the executable <typeparamref name="TApp"/> declares (<see cref="ExecutablePath"/>) as that page object:
    /// <c>App.Launch&lt;MainPage&gt;()</c>. Throws <see cref="InvalidOperationException"/>, before anything starts,
    /// when <typeparamref name="TApp"/> does not declare one, and <see cref="AppProcessException"/> when it cannot be started.
    /// </summary>
    public static TApp Launch<TApp>() where TApp : App, new() =>
        Launch<TApp>(options: null);

    /// <summary>Like <see cref="Launch{TApp}()"/>, with <paramref name="options"/> overriding the page object's <c>DefaultOptions</c> one by one.</summary>
    public static TApp Launch<TApp>(AppLaunchOptions? options) where TApp : App, new()
    {
        var app = new TApp();
        App definition = app; // Protected members are reachable only through the base type.
        var path = definition.ExecutablePath ?? throw new InvalidOperationException(
            $"{typeof(TApp).Name} does not say which executable it launches. Override {nameof(ExecutablePath)} in " +
            $"{typeof(TApp).Name}, or pass the path: App.Launch<{typeof(TApp).Name}>(path).");

        return Start(app, effective => AppLauncher.LaunchExe(path, effective), options);
    }

    /// <summary>
    /// Starts a packaged app by its AppUserModelID. Throws <see cref="AppProcessException"/> when it cannot be activated,
    /// and <see cref="ArgumentException"/> for <see cref="AppLaunchOptions.WorkingDirectory"/> or
    /// <see cref="AppLaunchOptions.Environment"/>, which Windows decides for packaged apps.
    /// </summary>
    public static App LaunchPackaged(string appUserModelId, AppLaunchOptions? options = null) =>
        Start(new App(), effective => AppLauncher.LaunchPackaged(appUserModelId, effective), options);

    /// <summary>Attaches to a running process by id. Throws <see cref="AppProcessException"/> when it is not running.</summary>
    public static App Attach(int processId) =>
        Start(new App(), _ => AppLauncher.Attach(processId), options: null, ownsProcess: false);

    /// <summary>Like <see cref="Attach(int)"/>, as <typeparamref name="TApp"/>.</summary>
    public static TApp Attach<TApp>(int processId) where TApp : App, new() =>
        Start(new TApp(), _ => AppLauncher.Attach(processId), options: null, ownsProcess: false);

    /// <summary>The automation context used for this app.</summary>
    public AutomationContext Context => _context ?? throw NotConnected();

    /// <summary>The application's process.</summary>
    public Process Process => _process ?? throw NotConnected();

    /// <summary>How long <see cref="MainWindow"/> waits for the window to appear the first time. Default 20 seconds.</summary>
    public TimeSpan MainWindowTimeout { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>The application's main window, waited for on first use.</summary>
    public Element MainWindow => field ??= MainWindowLocator.Find(Context, Process, MainWindowTimeout);

    /// <summary>
    /// Waits for a descendant of the main window whose AutomationId, or failing that Name, equals
    /// <paramref name="automationIdOrName"/> (<see cref="Element.FindByAutomationIdOrName"/>).
    /// Throws <see cref="UiaElementNotFoundException"/> on timeout.
    /// </summary>
    public Element Find(string automationIdOrName, TimeSpan? timeout = null) =>
        MainWindow.FindByAutomationIdOrName(automationIdOrName, timeout);

    /// <summary>Like <see cref="Find"/>, but returns null on timeout.</summary>
    public Element? TryFind(string automationIdOrName, TimeSpan? timeout = null) =>
        MainWindow.TryFindByAutomationIdOrName(automationIdOrName, timeout);

    /// <summary>
    /// Waits for a window of this application titled <paramref name="title"/>, such as a dialog. Owned windows are
    /// listed under their owner or under the desktop depending on the UI framework, so both are searched.
    /// Throws <see cref="UiaElementNotFoundException"/> on timeout.
    /// </summary>
    public Element FindWindow(string title, TimeSpan? timeout = null) =>
        Context.WaitForOrThrow(WindowProbe(title), () => $"window titled '{title}' of process {Process.Id}", timeout);

    /// <summary>Like <see cref="FindWindow"/>, but returns null on timeout.</summary>
    public Element? TryFindWindow(string title, TimeSpan? timeout = null) =>
        Context.WaitFor(WindowProbe(title), timeout);

    private Func<Element?> WindowProbe(string title)
    {
        var processId = Process.Id;
        Expression<Func<Element, bool>> isWindow = e => e.Name == title && e.ControlType == ControlType.Window && e.ProcessId == processId;
        var owner = MainWindow;
        var desktop = Context.GetRootElement();
        return () => owner.TryFind(isWindow, TreeScope.Children, TimeSpan.Zero)
                     ?? desktop.TryFind(isWindow, TreeScope.Children, TimeSpan.Zero);
    }

    /// <summary>Closes the application: Window pattern first, then kill after <paramref name="timeout"/>.</summary>
    public void Close(TimeSpan? timeout = null) => AppCloser.Close(Context, Process, timeout);

    /// <summary>Closes the app if it was launched by this instance, then releases the process and context.</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases the app. Derived types override this to release their own resources, then call the base.</summary>
    /// <param name="disposing">True when called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
            return;
        _disposed = true;

        // Not created through a factory: not connected, nothing to release.
        if (_process is null || _context is null)
            return;

        try
        {
            if (_ownsProcess)
                Close();
        }
        finally
        {
            _process.Dispose();
            _context.Dispose();
        }
    }

    // The context is created before the process so an STA thread fails before anything is started.
    private static TApp Start<TApp>(TApp app, Func<AppLaunchOptions, Process> connect, AppLaunchOptions? options,
        bool ownsProcess = true) where TApp : App
    {
        App connected = app; // Private and protected members are reachable only through the base type.
        var effective = connected.DefaultOptions.OverriddenBy(options);

        var context = new AutomationContext();
        try
        {
            if (effective.ShowPointer is { } showPointer)
                context.ShowPointer = showPointer;
            if (effective.MainWindowTimeout is { } mainWindowTimeout)
                connected.MainWindowTimeout = mainWindowTimeout;

            connected._process = connect(effective);
            connected._context = context;
            connected._ownsProcess = ownsProcess;
            return app;
        }
        catch
        {
            context.Dispose();
            throw;
        }
    }

    private static InvalidOperationException NotConnected() =>
        new("This app is not connected to an application. Create it with App.Launch<TApp>(...) or App.Attach<TApp>(...).");
}
