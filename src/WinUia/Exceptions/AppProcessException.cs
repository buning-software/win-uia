namespace WinUia;

/// <summary>
/// Starting, activating or attaching to the application's process failed, or the process exited before its main window
/// appeared. Failures of UI Automation itself are <see cref="UiaException"/>s.
/// </summary>
public class AppProcessException(string message, Exception? innerException = null)
    : Exception(message, innerException);
