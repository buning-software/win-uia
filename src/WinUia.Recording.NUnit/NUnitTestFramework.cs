using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace WinUia.Recording;

/// <summary>
/// NUnit as the <see cref="ITestFramework"/> of a recording: the video is named after the running test, kept when the test
/// failed, and attached to its result.
/// </summary>
public sealed class NUnitTestFramework : ITestFramework
{
    /// <inheritdoc />
    public string? CurrentTestName => $"{TestContext.CurrentContext.Test.ClassName?.Split('.')[^1]}.{TestContext.CurrentContext.Test.Name}";

    /// <inheritdoc />
    public bool? CurrentTestFailed => TestContext.CurrentContext.Result.Outcome.Status switch
    {
        TestStatus.Failed => true,
        TestStatus.Passed => false,
        // Skipped and warning-only outcomes are not failures.
        TestStatus.Skipped or TestStatus.Warning => false,
        // Inconclusive: NUnit has not recorded the outcome yet (the test body or SetUp is still unwinding), so keep the video.
        _ => null,
    };

    /// <inheritdoc />
    public void Attach(string path, string? description) => TestContext.AddTestAttachment(path, description);
}
