# WinUia.Recording

Record the screen while a UI test runs, so a failing test on CI leaves a video and not only a log.
Install it, pass a recording config to WinUia, and every launched app is recorded.

```bash
dotnet add package WinUia.Recording
```

## Requirements

[ffmpeg](https://ffmpeg.org) must be installed. It is not bundled. It is found through
`FfmpegRecorder.FfmpegPath`, then the `WINUIA_FFMPEG` environment variable, then `PATH`.
Without ffmpeg an app still launches, unrecorded, and a warning is traced.

## Usage

Put a `RecordingOptions` in the launch options, either for one launch or for all launches of an app class:

```csharp
// All launches of this app:
public sealed class MyApp : App
{
    protected override string ExecutablePath => @"C:\path\to\MyApp.exe";
    protected override AppLaunchOptions DefaultOptions => new() { Recording = new RecordingOptions() };
}

// Or one launch:
using var app = App.Launch(path, new AppLaunchOptions { Recording = new RecordingOptions() });
```

That is all the test code needed. The recording starts when the app is launched, so the video shows it starting,
and stops when the app is disposed, after the app has closed. Attaching to a running process is not recorded.

To name the video after the test and keep only the videos of failed tests, tell the recording how, with your test framework.
For NUnit:

```csharp
Recording = new RecordingOptions
{
    Name = () => TestContext.CurrentContext.Test.FullName,
    Keep = () => TestContext.CurrentContext.Result.Outcome.Status is TestStatus.Failed or TestStatus.Inconclusive,
    OnKept = path => TestContext.AddTestAttachment(path),   // optional: attach the video to the test result
}
```

`Keep` is asked when the app is disposed, so dispose the app in `[TearDown]`. A passing or skipped test then leaves no file.
`OnKept` is called with the video's path once it is kept, so a video can be attached to the test result.
When a name is longer than 100 characters its end is kept. To keep every video, set `Mode = RecordingMode.All`.

### Environment variables

| Variable | |
|---|---|
| `WINUIA_RECORDING` | `failed`, `all` or `off`: overrides `Mode`. |
| `WINUIA_RECORDING_DIR` | Where videos go when `OutputDirectory` is not set. Default `TestResults/Recordings`. |
| `WINUIA_FFMPEG` | Full path to `ffmpeg.exe`, for `FfmpegRecorder`. |

On CI this is how to record everything without changing code: set `WINUIA_RECORDING=all`.

## Options

`RecordingOptions` decides where the video goes and which ones are kept. Its properties are `init`-only: set them where you create it.

| Option | Default | |
|---|---|---|
| `Mode` | `Failed` | `Failed`: only the videos of failed tests. `All`: every video. `Off`: record nothing. |
| `OutputDirectory` | see above | Where videos are written. |
| `Area` | primary monitor | A `ScreenRect` in physical pixels. Use `ScreenArea.PrimaryMonitor()` or `ScreenArea.MonitorContaining(rect)` to pick a monitor. |
| `Recorder` | `new FfmpegRecorder()` | How the screen is captured, see below. |
| `Name` | app and time | A `Func<string?>` that gives the video's file name, for example the running test's name. |
| `Keep` | keep all | A `Func<bool>` asked when the app is disposed: true keeps the video, false deletes it. Ignored when `Mode` is `All`. |

An app that opens on a monitor other than the primary one needs `Area`, since the window does not exist yet when recording starts.

### Recorders

`FfmpegRecorder` records with ffmpeg and is the default:

| Option | Default | |
|---|---|---|
| `FrameRate` | 15 | Frames per second. |
| `DrawMouse` | true | Draw the cursor in the video. |
| `FfmpegPath` | null | Full path to ffmpeg.exe. |

ffmpeg encodes with `libx264 -preset ultrafast` to keep CPU use low while the tests run. On a display with scaling (125%, 150%)
the capture is made DPI-aware, but across monitors with different scaling only the primary monitor's scale is used.

```csharp
Recording = new RecordingOptions { Recorder = new FfmpegRecorder { FrameRate = 30 } }
```

The capture method is a strategy: `IRecorder` has a `FileExtension`, an `UnavailableReason` (null when it can be used) and
`Start(outputPath, area)`, which returns something to dispose to stop. To record another way, implement it and pass it as
`Recorder`. Naming, keeping, the environment variables and the app lifecycle are unchanged.

### Something other than a screen video

What WinUia sees is `IAppRecorder` (in `WinUia`): `Start(AppRecordingContext)` is called when an app launches, with the
executable or AppUserModelID and the launch options, and the result is disposed when the app is disposed. `RecordingOptions`
is the implementation for screen video; implement `IAppRecorder` yourself to collect something else, such as logs or a trace.

## Without an app: `ScreenRecording`

The file lifetime around a recorder can be used on its own:

```csharp
using var recording = ScreenRecording.Start("TestResults/Recordings/MyTest.mp4", new FfmpegRecorder(), area: null);
// ...
recording.Keep = true; // Dispose deletes the file unless Keep is true
```

It does not take the desktop lock, so start it after your test has the desktop and stop it before it is released.
With no `area` it records the whole desktop, all monitors included.
