namespace WinUia.Recording;

/// <summary>Whether <see cref="RecordingOptions"/> records, and which recordings are kept.</summary>
public enum RecordingMode
{
    /// <summary>
    /// Record, and keep only the recordings of failed tests: <see cref="RecordingOptions.Keep"/> says whether a test
    /// failed, so a passing test leaves no file. Without <see cref="RecordingOptions.Keep"/> every recording is kept.
    /// The default.
    /// </summary>
    Failed,

    /// <summary>Record, and keep every recording.</summary>
    All,

    /// <summary>Record nothing.</summary>
    Off,
}
