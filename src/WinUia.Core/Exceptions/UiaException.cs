namespace WinUia;

/// <summary>Base class for all UI Automation failures raised by WinUia.</summary>
public class UiaException : Exception
{
    /// <summary>Creates an exception with the given message, HRESULT and inner exception.</summary>
    public UiaException(string message, int hresult = 0, Exception? innerException = null)
        : base(message, innerException)
    {
        HResult = hresult;
    }
}
