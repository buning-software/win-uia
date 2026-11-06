# WinUia.Core

The automation core of WinUia: `AutomationContext`, `Element` with lambda searches and self-healing locators,
control-pattern wrappers (`WinUia.Patterns`) and physical mouse and keyboard input (`WinUia.Input`), on top of the
Windows UI Automation (UIA3) COM API with no other dependencies.

Most users install `WinUia` (which brings this package). For tests of any framework it also offers `DesktopLock`
(one desktop at a time, across processes; `App` takes it for you) and `Poll.Until` (wait for asynchronous UI state). Use `WinUia.Core` directly
to automate elements without launching or attaching to an application through `App`:

```
dotnet add package WinUia.Core
```

```csharp
using WinUia;

using var context = new AutomationContext();
var window = context.FromHandle(hwnd);
window.FindByAutomationId("btnOk").Click();
```

Documentation: https://github.com/buning-software/win-uia
