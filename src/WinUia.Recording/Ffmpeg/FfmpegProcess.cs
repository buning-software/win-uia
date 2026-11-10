using System.ComponentModel;
using System.Diagnostics;

namespace WinUia.Recording;

internal sealed class FfmpegProcess : IDisposable
{
    // ffmpeg prints this once the input is open and the output is set up, just before it starts writing frames.
    private const string ReadyMarker = "Output #0";
    private const int KeptLines = 30;

    // Only a ceiling: startup ends as soon as ffmpeg is ready or has exited.
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan StopTimeout = TimeSpan.FromSeconds(15);

    private readonly Process _process;
    private readonly Queue<string> _output = new();
    private readonly ManualResetEventSlim _ready = new();
    private readonly ManualResetEventSlim _exited = new();

    private FfmpegProcess(Process process) => _process = process;

    public static FfmpegProcess Start(string ffmpegPath, IReadOnlyList<string> arguments)
    {
        var startInfo = new ProcessStartInfo(ffmpegPath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        // gdigrab is not DPI-aware; without this, offsets and size (physical pixels) are misread on scaled displays.
        startInfo.Environment.TryGetValue("__COMPAT_LAYER", out var layers);
        startInfo.Environment["__COMPAT_LAYER"] = string.IsNullOrWhiteSpace(layers) ? "HighDpiAware" : $"{layers} HighDpiAware";

        var process = new Process { StartInfo = startInfo };
        var result = new FfmpegProcess(process);
        process.EnableRaisingEvents = true;
        process.Exited += (_, _) => result._exited.Set();
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null)
            {
                return;
            }

            lock (result._output)
            {
                result._output.Enqueue(e.Data);
                while (result._output.Count > KeptLines)
                {
                    result._output.Dequeue();
                }
            }

            if (e.Data.StartsWith(ReadyMarker, StringComparison.Ordinal))
            {
                result._ready.Set();
            }
        };

        try
        {
            process.Start();
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
        {
            process.Dispose();
            throw new ScreenRecordingException($"Failed to start ffmpeg at '{ffmpegPath}': {exception.Message}", exception);
        }

        process.BeginErrorReadLine();

        // Bad arguments or an unusable desktop make ffmpeg exit at once; otherwise it says when it is ready.
        WaitHandle.WaitAny([result._ready.WaitHandle, result._exited.WaitHandle], StartupTimeout);
        if (process.HasExited)
        {
            process.WaitForExit(); // flush the redirected stderr
            var output = result.Errors();
            result.DisposeResources();
            throw new ScreenRecordingException($"ffmpeg exited right after starting. Output: {output}");
        }

        return result;
    }

    public void Dispose()
    {
        try
        {
            if (_process.HasExited)
            {
                if (_process.ExitCode != 0)
                {
                    Console.Error.WriteLine($"WinUia.Recording: ffmpeg stopped unexpectedly (exit code {_process.ExitCode}). Output: {Errors()}");
                }
            }
            else
            {
                try
                {
                    _process.StandardInput.Write('q');
                    _process.StandardInput.Flush();
                    _process.StandardInput.Close();
                }
                catch (IOException)
                {
                    // Already exiting.
                }

                if (!_process.WaitForExit(StopTimeout))
                {
                    try
                    {
                        _process.Kill(entireProcessTree: true);
                        _process.WaitForExit();
                    }
                    catch (Exception exception) when (exception is InvalidOperationException or Win32Exception)
                    {
                        // Exited on its own between the timeout and the kill.
                    }
                }
            }
        }
        finally
        {
            DisposeResources();
        }
    }

    // The events are left to the finalizer: the process's Exited and stderr callbacks may still fire after this.
    private void DisposeResources() => _process.Dispose();

    private string Errors()
    {
        lock (_output)
        {
            return string.Join(Environment.NewLine, _output);
        }
    }
}
