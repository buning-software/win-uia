namespace WinUia.Recording;

/// <summary>Thrown when ffmpeg cannot be found or fails to start recording.</summary>
public class ScreenRecordingException : Exception
{
    /// <summary>Creates the exception with a message.</summary>
    public ScreenRecordingException(string message) : base(message)
    {
    }

    /// <summary>Creates the exception with a message and the exception that caused it.</summary>
    public ScreenRecordingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
