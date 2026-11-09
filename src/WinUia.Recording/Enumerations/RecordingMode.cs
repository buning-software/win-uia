namespace WinUia.Recording;

/// <summary>Whether <see cref="RecordingOptions"/> records, and which recordings are kept.</summary>
public enum RecordingMode
{
    /// <summary>Record, and keep only the recordings of failed tests. A passing test leaves no file. The default.</summary>
    Failed,

    /// <summary>Record, and keep every recording.</summary>
    All,

    /// <summary>Record nothing.</summary>
    Off,
}

internal static class RecordingModes
{
    public const string EnvironmentVariable = "WINUIA_RECORDING";

    public static RecordingMode? Parse(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "all" => RecordingMode.All,
        "failed" => RecordingMode.Failed,
        "off" => RecordingMode.Off,
        _ => null,
    };

    public static RecordingMode Resolve(RecordingMode mode, string? environmentValue) => Parse(environmentValue) ?? mode;
}
