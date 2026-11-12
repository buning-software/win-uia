namespace WinUia.Recording;

internal static class FfmpegLocator
{
    public const string EnvironmentVariable = "WINUIA_FFMPEG";

    /// <summary>Finds ffmpeg: explicit path, then WINUIA_FFMPEG, then PATH. Returns null and the places searched when not found.</summary>
    public static string? TryLocate(
        string? explicitPath,
        Func<string, string?> getEnvironmentVariable,
        Func<string, bool> fileExists,
        out IReadOnlyList<string> searched)
    {
        var tried = new List<string>();
        searched = tried;

        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            tried.Add($"FfmpegRecorder.FfmpegPath ({explicitPath})");
            // An explicit choice is never replaced by another ffmpeg: a wrong path must show up, not be papered over.
            return fileExists(explicitPath) ? explicitPath : null;
        }

        var fromEnvironment = getEnvironmentVariable(EnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            tried.Add($"{EnvironmentVariable} ({fromEnvironment})");
            return fileExists(fromEnvironment) ? fromEnvironment : null;
        }

        tried.Add("PATH");
        var path = getEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var candidate = Path.Combine(directory.Trim('"'), "ffmpeg.exe");
            if (fileExists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    public static string? TryLocate(string? explicitPath, out IReadOnlyList<string> searched) =>
        TryLocate(explicitPath, Environment.GetEnvironmentVariable, File.Exists, out searched);

    public static string NotFoundMessage(IReadOnlyList<string> searched) =>
        $"ffmpeg was not found. Searched: {string.Join(", ", searched)}. " +
        $"Install ffmpeg and add it to PATH, set {EnvironmentVariable}, or set FfmpegRecorder.FfmpegPath.";

    public static string Locate(string? explicitPath) =>
        TryLocate(explicitPath, out var searched) ?? throw new ScreenRecordingException(NotFoundMessage(searched));
}
