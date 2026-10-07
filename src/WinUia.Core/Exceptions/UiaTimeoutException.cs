using WinUia.Core.Interop;

namespace WinUia;

/// <summary>A UI Automation call or wait timed out (<c>UIA_E_TIMEOUT</c>).</summary>
public class UiaTimeoutException(string message, int hresult = HResults.UIA_E_TIMEOUT, Exception? innerException = null)
    : UiaException(message, hresult, innerException);
