namespace WinUia.Recording;

/// <summary>
/// What a recording needs to know about the running test. <c>WinUia.Recording.NUnit</c> provides NUnit; another framework
/// is passed as <see cref="RecordingOptions.TestFramework"/>.
/// </summary>
public interface ITestFramework
{
    /// <summary>The name of the running test, used as the file name. Null when there is none.</summary>
    string? CurrentTestName { get; }

    /// <summary>
    /// Whether the running test has failed so far. Asked when the app is disposed, so dispose it in the test's teardown.
    /// Null when unknown, which keeps the recording.
    /// </summary>
    bool? CurrentTestFailed { get; }

    /// <summary>Attaches a kept recording to the running test's result.</summary>
    void Attach(string path, string? description);
}
