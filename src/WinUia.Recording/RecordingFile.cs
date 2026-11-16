namespace WinUia.Recording;

internal static class RecordingFile
{
    // MAX_PATH is 260 including the terminating null. Not every tool (ffmpeg, Explorer) handles longer paths, and a video
    // that cannot be written is a missing video, so the name is made to fit the folder instead.
    private const int MaxPathLength = 259;

    // Room for the "-2" ... "-999" that keeps a second recording from overwriting the first.
    private const int NumberReserve = 4;

    private const int MinNameLength = 16;

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

        var budget = MaxPathLength - Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar).Length - 1 - extension.Length - NumberReserve;
        var baseName = Fit(Sanitize(string.IsNullOrWhiteSpace(name) ? DateTime.Now.ToString("yyyyMMdd-HHmmss") : name), Math.Max(budget, MinNameLength));
        var path = Path.Combine(folder, baseName + extension);
        for (var number = 2; exists(path); number++)
            path = Path.Combine(folder, $"{baseName}-{number}{extension}");

        return path;
    }

    public static string Sanitize(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }

    // Keep the tail: the test name and its parameters come last and tell recordings apart.
    // Start at a segment boundary, so the name begins with a whole namespace or class name, not half of one.
    private static string Fit(string name, int maxLength)
    {
        if (name.Length <= maxLength)
            return name;

        var tail = name[^maxLength..];
        var boundary = tail.IndexOf('.');
        return boundary >= 0 && boundary < tail.Length - 1 ? tail[(boundary + 1)..] : tail;
    }
}
