namespace WinUia.Recording;

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
