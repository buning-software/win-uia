namespace WinUia.Recording;

internal static class RecordingFile
{
    private const int MaxNameLength = 100;

    public const string DirectoryEnvironmentVariable = "WINUIA_RECORDING_DIR";

    // Directory: option, then WINUIA_RECORDING_DIR, then TestResults/Recordings.
    public static string ResolveFolder(string? directory, string? fromEnvironment) =>
        !string.IsNullOrWhiteSpace(directory) ? directory
        : !string.IsNullOrEmpty(fromEnvironment) ? fromEnvironment
        : Path.Combine("TestResults", "Recordings");

    // Numbered so a second launch in the same test does not overwrite the first.
    public static string Choose(string folder, string? name, string extension, Func<string, bool>? exists = null)
    {
        exists ??= File.Exists;

        var baseName = Sanitize(string.IsNullOrWhiteSpace(name) ? DateTime.Now.ToString("yyyyMMdd-HHmmss") : name);
        var path = Path.Combine(folder, baseName + extension);
        for (var number = 2; exists(path); number++)
            path = Path.Combine(folder, $"{baseName}-{number}{extension}");

        return path;
    }

    public static string Sanitize(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));

        // Parameterised test names can be very long, and the whole path must stay under MAX_PATH.
        // Keep the tail: the test name and its parameters come last and tell recordings apart.
        return clean.Length > MaxNameLength ? clean[^MaxNameLength..] : clean;
    }
}
