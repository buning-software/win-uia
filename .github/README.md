<div id="top"></div>

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![GNU Affero General Public License v3.0 License][license-shield]][license-url]
[![Tests][tests-shield]][tests-url]
[![Unit tests][unit-tests-shield]][tests-url]

<div align="center">
  <img src="../assets/win-uia-logo.png" alt="WinUia logo" width="120" height="120">
  <h3 align="center">WinUia</h3>
  <p align="center">
    A modern, high-performance C# library for Windows UI Automation, desktop app testing, and accessibility tooling.
    <br />
    <a href="https://github.com/buning-software/win-uia/issues">Report Bug</a>
    ·
    <a href="https://github.com/buning-software/win-uia/issues">Request Feature</a>
  </p>
</div>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
        <li><a href="#features">Features</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#installation">Installation</a></li>
        <li><a href="#building-from-source">Building from source</a></li>
      </ul>
    </li>
    <li>
      <a href="#usage">Usage</a>
      <ul>
        <li><a href="#setting-up-your-application">Setting up your application</a></li>
        <li><a href="#recording">Recording</a></li>
        <li><a href="#limitations">Limitations</a></li>
      </ul>
    </li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>



## About The Project

WinUia is a .NET library for inspecting and automating Windows applications through the Microsoft UI Automation framework.

### Built With

* [.NET 11](https://dotnet.microsoft.com/)
* [Microsoft UI Automation](https://learn.microsoft.com/windows/win32/winauto/entry-uiauto-win32)

### Features

* **No dependencies:** talks to the native UIA3 COM API (`UIAutomationCore.dll`) through hand-written interop. No FlaUI, no interop packages.
* **Works across UI stacks:** WinForms, WPF, WinUI 3 (packaged and unpackaged), UWP and classic Win32 apps, x86 or x64.
* **Self-healing elements:** an element remembers how it was found and re-finds itself when a re-render makes it stale.
* **Waiting built in:** `Find` polls until the element appears (default 5 s), `TryFind` returns `null` instead of throwing.
* **Patterns first, mouse last:** `Click()` and `SetValue()` use UIA patterns (Invoke, Toggle, SelectionItem, ExpandCollapse, Value) and only fall back to `SendInput`.

## Getting Started

WinUia needs the [.NET 11 SDK](https://dotnet.microsoft.com/download) and Windows 10 or 11; tests that drive an app need an interactive, unlocked desktop. Use Visual Studio 2022 or later, or any editor with the `dotnet` CLI. The Windows SDK is not required.

### Installation

```bash
dotnet add package WinUia
dotnet add package WinUia.Recording   # optional: a screen video of the test run, see Recording
```

### Building from source

```bash
git clone https://github.com/buning-software/win-uia.git
cd win-uia
dotnet build WinUia.slnx
dotnet test WinUia.slnx
```

The UI tests (the `*.UiTests` projects) launch an app and need an interactive desktop. Run only the others with `dotnet test WinUia.slnx --filter "FullyQualifiedName!~.UiTests."`.

## Usage

Test a calculator app in a few lines:

```csharp
using WinUia;

using var app = App.Launch(@"C:\path\to\Calculator.exe");   // or App.LaunchPackaged(appId), App.Attach(pid)

app.Find("btn2").Click();
app.Find("btnPlus").Click();
app.Find("btn3").Click();
app.Find("btnEquals").Click();

Console.WriteLine(app.Find("txtResult").GetText());
// Disposing a launched app closes it (Window pattern first, then kill).
```

`Element`'s methods (`Click`, `SetValue`, `GetText`, `Toggle`, `Select`, `Expand`, `Collapse`) are the everyday API: they check the element is enabled and pick the right pattern. The pattern wrappers (`element.TogglePattern.State`, `element.WindowPattern.Close()`, ...) are for pattern state and the members those methods do not cover. Other windows, such as dialogs, are found with `app.FindWindow("Save changes?")`, and any element with a lambda: `window.Find(e => e.ControlType == ControlType.Button && e.Name == "Yes")`.

### Setting up your application

`examples/WinUia.Examples.Winforms.UiTests` is a complete test project for the WinForms app next to it. To do the same for your app:

1.  **Create a test project** that targets Windows and references `WinUia` and your test framework (the example uses NUnit).

2.  **Derive a class from `App`** that says which executable it is and how it starts, so tests launch it without repeating either. The example builds the app first and passes its path as assembly metadata; a fixed path works too.

    ```csharp
    using WinUia;
    using WinUia.Models;
    using WinUia.Recording;

    public sealed class MyApp : App
    {
        protected override string ExecutablePath => @"C:\path\to\MyApp.exe";

        protected override AppLaunchOptions DefaultOptions => new()
        {
            ShowPointer = true,
            Recording = new RecordingOptions   // optional: records the screen while the app runs, see Recording below
            {
                Name = () => TestContext.CurrentContext.Test.FullName,
                Keep = () => TestContext.CurrentContext.Result.Outcome.Status is TestStatus.Failed or TestStatus.Inconclusive,
            },
        };
    }
    ```

    The generic factories return it already connected: `using var app = App.Launch<MyApp>();` (also `App.Attach<T>`). `AppLaunchOptions` sets the arguments, working directory, environment, pointer and main-window timeout; options passed to a factory override these defaults one by one, and a path launches another executable.

3.  **Describe the screen with page objects.** The app owns the process, so it is the one thing you dispose; pages are plain classes that read the app and need no teardown.

    ```csharp
    public class MainForm(MyApp app)
    {
        public Element Window => app.MainWindow;
        public Element SaveButton => Window.FindByAutomationId("btnSave");

        public SettingsTab OpenSettings()
        {
            Window.FindByAutomationId("tabSettings").Select();
            return new SettingsTab(app);
        }
    }
    ```

    A page for part of the window, such as a tab, can derive from the main window's page:

    ```csharp
    public class SettingsTab(MyApp app) : MainForm(app)
    {
        public Element Content => Window.FindByAutomationId("lblSettings");
    }
    ```

4.  **Write a test.** Launch the app and create the page in `[SetUp]`, and dispose the app in `[TearDown]`:

    ```csharp
    public class SettingsTests
    {
        private MyApp _app = null!;
        private MainForm _mainForm = null!;

        [SetUp]
        public void Launch()
        {
            _app = App.Launch<MyApp>();
            _mainForm = new MainForm(_app);
        }

        [TearDown]
        public void Close()
        {
            _app.Dispose();
        }

        [Test]
        public void Saving_the_settings_shows_a_confirmation()
        {
            var settings = _mainForm.OpenSettings();

            settings.SaveButton.Click();

            Assert.That(settings.Content.Name, Is.EqualTo("Saved"));
        }
    }
    ```

### Testing

WinUia works with any test framework and ships no framework package, attribute or convention.

* **The desktop is shared automatically.** UI tests share one mouse, one keyboard focus and one foreground window, and `dotnet test` runs assemblies in parallel. An `App` holds the desktop (`DesktopLock`) from `Launch` or `Attach` until it is disposed, across processes, so tests take turns with no setup. The wait limit is `AppLaunchOptions.DesktopTimeout` (default 5 minutes). Dispose the app as soon as the test is done.
* **`DesktopLock.Acquire()`** holds the desktop longer, for example for a whole test run, or when you use `AutomationContext` without `App`. Locks nest within a process.
* **`Poll.Until(condition, timeout)`** waits for UI state that the application updates asynchronously and returns whether it came true; assert on the result with your framework.

The rest is your framework's own: tag or group UI tests to filter them (a category, a trait, or a separate test project), set timeouts with its timeout, and run on an MTA thread (UI Automation requires it). In NUnit:

```csharp
[assembly: Apartment(ApartmentState.MTA)]

// In a test: Assert.That(Poll.Until(() => page.ResultLabel.Name == "Clicked"), Is.True);
```

`examples/WinUia.Examples.Winforms.UiTests` is the complete NUnit setup. `tests/WinUia.TestFrameworks.XUnit.UiTests` and `tests/WinUia.TestFrameworks.MsTest.UiTests` show the same for xUnit and MSTest.

### Recording

With the optional `WinUia.Recording` package, put a `RecordingOptions` in the app's `DefaultOptions` (see [Setting up your application](#setting-up-your-application)), or in the `AppLaunchOptions` of one launch. `Keep` is asked when the app is disposed, so dispose it in `[TearDown]`; without `Name` and `Keep` every video is kept. Videos go to `TestResults/Recordings`, or to `WINUIA_RECORDING_DIR`. Set `WINUIA_RECORDING` to `all`, `failed` or `off` to change what is recorded without touching code.

[ffmpeg](https://ffmpeg.org) is not bundled: put it on `PATH`, or point `WINUIA_FFMPEG` at `ffmpeg.exe`. Projects that don't use the package are unaffected. All options are in the [package README](../src/WinUia.Recording/README.md).

### Project structure

WinUia is a modular monolith: one project and up to three test projects per module. Dependencies point one way, from the application layer down to the platform:

| Module | Layer | Contents | Depends on |
|---|---|---|---|
| `WinUia` | Application | `App` (launch, attach, find, close), `AppLaunchOptions`, `AppProcessException`, `IAppRecorder` (the hook a recording plugs into); internal launchers | `WinUia.Core` (public API only) |
| `WinUia.Core` | Automation | `AutomationContext`, `Element` (lambda searches, self-healing locators), `WinUia.Patterns`, the `Uia*Exception` types, physical input (`WinUia.Input`: `IInputSimulator` and `Win32InputSimulator` via `SendInput`; one per `AutomationContext` as `Input`, replaceable in tests), `Poll` and `DesktopLock`, the UIA COM and Win32 interop | — |
| `WinUia.Recording` | Application (optional) | `RecordingOptions` (an `IAppRecorder`), `IRecorder` and `FfmpegRecorder`, `ScreenRecording`, `ScreenArea`, `RecordingMode` | `WinUia` (public API only) |

Module rules:

* A module's `Interop/` folder is private to it. Other modules use what it offers (`IInputSimulator`), never its P/Invoke or COM declarations.
* Modules use each other's public API only, the same API an application gets. Internals are shared only through `InternalsVisibleTo`, and only with the module's own test projects.
* Folders group files by role (models, enumerations, exceptions, patterns, interop); a folder exists only when it has files, and test projects mirror them. One type per file. Namespaces do not follow folders: public and internal types alike use `WinUia`, `WinUia.Patterns` and `WinUia.Input`, except `Interop/` (keeps its project's `.Interop` namespace as a fence) and the launch-option types `AppLaunchOptions`, `IAppRecorder` and `AppRecordingContext` (`WinUia.Models`); tests follow project and folder.
* Package versions are set once, in `Directory.Packages.props`.
* Every public member is listed in the package's `PublicAPI.Unshipped.txt` (moved to `PublicAPI.Shipped.txt` at release); the build fails on an unlisted change, so a public API change always shows in review.

Test projects under `tests/`, by what the tests need:

| Project | Tests | Needs |
|---|---|---|
| `<Module>.UnitTests` | The module's code on its own | Nothing |
| `<Module>.IntegrationTests` | Against the real UI Automation client, Win32 and other OS objects, without showing a window | Windows |
| `<Module>.UiTests` | Launch an app or move the real mouse and keyboard | An interactive, unlocked desktop |

Tests are selected by project name, so `dotnet test --filter "FullyQualifiedName!~.UiTests."` runs the unit and integration tests on any Windows machine. Every `.UiTests` project holds a `DesktopLock` for the whole assembly (`tests/Directory.Build.props`, `tests/DesktopLockSetUp.cs`), because some tests move the real cursor without an `App`. UI tests launch `tests/WinUia.Testing.Common/TestApp`, an empty window, located through `AppPaths`. Tests of control behaviour (patterns, searches, self-healing elements, dialogs, input) run against the WinForms example in `examples/WinUia.Examples.Winforms.UiTests`. Tests use NUnit, except `tests/WinUia.TestFrameworks.*`: the same basic checks and recording tests run under NUnit, xUnit and MSTest (shared code in `WinUia.TestFrameworks.Common`), so a release is blocked if WinUia stops working under one of them.

`examples/` shows WinUia as a user would use it: `WinUia.Examples.Winforms` is a WinForms app, tested by `WinUia.Examples.Winforms.UiTests` with page objects, using only the public API of WinUia and WinUia.Recording, and NUnit. Examples and tests do not reference each other, except that `WinUia.TestFrameworks.Common` runs the WinForms example.

### Limitations

* **MTA only:** UI Automation must be called from an MTA thread. Creating an `AutomationContext` (or launching an `App`) on an STA thread (a WinForms/WPF UI thread, an `[STAThread]` `Main`) throws. Use `Task.Run` or a thread-pool thread.
* **Physical input needs a real desktop:** the mouse and keyboard fallbacks use `SendInput`, which does nothing on a locked workstation or a disconnected RDP session, and goes to whatever window is on top.
* **Elevation (UIPI):** a non-elevated process cannot send input to, and has limited UIA access to, an elevated app. Run the automation elevated when the target is.
* **DPI:** creating an `AutomationContext` makes the process per-monitor DPI aware (if it is not already) so UIA coordinates and `SendInput` both use physical pixels.

<!-- CONTRIBUTING -->
## Contributing

Contributions are greatly appreciated. Fork the repo and open a pull request, or open an issue with the tag "enhancement". Don't forget to give the project a star!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b features/feature-title`)
3. Commit your Changes (`git commit -m 'Added feature'`)
4. Push to the Branch (`git push origin features/feature-title`)
5. Open a Pull Request


<!-- LICENSE -->
## License
Distributed under the GNU Affero General Public License v3.0 License. See `LICENSE` for more information.


<p align="right">(<a href="#top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/buning-software/win-uia.svg?style=for-the-badge
[contributors-url]: https://github.com/buning-software/win-uia/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/buning-software/win-uia.svg?style=for-the-badge
[forks-url]: https://github.com/buning-software/win-uia/network/members
[stars-shield]: https://img.shields.io/github/stars/buning-software/win-uia.svg?style=for-the-badge
[stars-url]: https://github.com/buning-software/win-uia/stargazers
[issues-shield]: https://img.shields.io/github/issues/buning-software/win-uia.svg?style=for-the-badge
[issues-url]: https://github.com/buning-software/win-uia/issues
[license-shield]: https://img.shields.io/github/license/buning-software/win-uia.svg?style=for-the-badge
[license-url]: https://github.com/buning-software/win-uia/blob/main/LICENSE
[tests-shield]: https://img.shields.io/github/actions/workflow/status/buning-software/win-uia/build-and-test.yaml?branch=main&label=tests&logo=dotnet&style=for-the-badge
[unit-tests-shield]: https://img.shields.io/badge/unit%20tests-82-brightgreen?logo=dotnet&style=for-the-badge
[tests-url]: https://github.com/buning-software/win-uia/actions/workflows/build-and-test.yaml
