namespace WinUia.Recording;

internal static class RecordingFile
{
    private const int MaxNameLength = 100;

    private static readonly string DefaultRoot = Path.Combine("TestResults", "Recordings");

    // The one place that picks an option over its default, for both directories.
    public static (string All, string Failed) ResolveDirectories(string? allDirectory, string? failedDirectory) =>
        (Resolve(allDirectory, "All"), Resolve(failedDirectory, "Failed"));

    private static string Resolve(string? option, string defaultName) =>
        !string.IsNullOrWhiteSpace(option) ? option : Path.Combine(DefaultRoot, defaultName);

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
