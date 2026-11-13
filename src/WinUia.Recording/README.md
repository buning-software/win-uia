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

Recording is on exactly when a `RecordingOptions` is configured, and then every run is recorded, locally too (which needs ffmpeg).
Every video is written to `TestResults/Recordings/All`. There is no keep policy and no environment switch in WinUia:
the pipeline decides what to keep or upload from the directories.

To name the video after the test and to get the videos of failed tests in a directory of their own, tell the recording how,
with your test framework. For NUnit:

```csharp
Recording = new RecordingOptions
{
    FileName = () => TestContext.CurrentContext.Test.FullName,
    Failed = () => TestContext.CurrentContext.Result.Outcome.Status is TestStatus.Failed or TestStatus.Inconclusive,
}
```

`Failed` is asked when the app is disposed, so dispose the app in `[TearDown]`. When it says the test failed, the video is
also copied to `TestResults/Recordings/Failed`; a passing test leaves its video in `All` only. `Failed` never deletes a video.
When a name is longer than 100 characters its end is kept.

The default directories are relative to the current directory, which for `dotnet test --results-directory <dir>` is not `<dir>`:
set `AllDirectory` and `FailedDirectory` when the pipeline looks elsewhere.

### Environment variables

| Variable | |
|---|---|
| `WINUIA_FFMPEG` | Full path to `ffmpeg.exe`, for `FfmpegRecorder`. |

## Options

`RecordingOptions` decides where the videos go. Its properties are `init`-only: set them where you create it.

| Option | Default | |
|---|---|---|
| `AllDirectory` | `TestResults/Recordings/All` | Where every video is written. |
| `FailedDirectory` | `TestResults/Recordings/Failed` | Where the video of a failed test is copied to. |
| `Area` | primary monitor | A `ScreenRect` in physical pixels. Use `ScreenArea.PrimaryMonitor()` or `ScreenArea.MonitorContaining(rect)` to pick a monitor. |
| `Recorder` | `new FfmpegRecorder()` | How the screen is captured, see below. |
| `FileName` | app and time | A `Func<string?>` that gives the video's file name, for example the running test's name. |
| `Failed` | none | A `Func<bool>` asked when the app is disposed: true copies the video to `FailedDirectory`. |

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
`Recorder`. Naming, the directories and the app lifecycle are unchanged.

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
