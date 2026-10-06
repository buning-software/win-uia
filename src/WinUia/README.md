# WinUia

Launch, attach to and drive Windows desktop applications (WinForms, WPF, WinUI, Win32) through UI Automation.
Elements re-find themselves when the UI recreates them, and searches are plain C# lambdas.

```
dotnet add package WinUia
```

```csharp
using WinUia;

using var app = App.Launch(@"C:\path\to\MyApp.exe");
app.Find("txtName").SetValue("Ada");                       // by AutomationId, then by Name
app.MainWindow.Find(e => e.ControlType == ControlType.Button && e.Name == "Save").Click();
var dialog = app.FindWindow("Save changes?");
dialog.FindByAutomationIdOrName("Yes").Click();
```

UI Automation must be called from an MTA thread. For NUnit tests, add `WinUia.NUnit`.
Documentation: https://github.com/buning-software/win-uia
