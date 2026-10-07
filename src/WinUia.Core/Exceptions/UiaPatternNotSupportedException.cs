using WinUia.Core.Interop;

namespace WinUia;

/// <summary>The element does not support the requested control pattern (<c>UIA_E_NOTSUPPORTED</c>).</summary>
public class UiaPatternNotSupportedException(string message, int hresult = HResults.UIA_E_NOTSUPPORTED, Exception? innerException = null)
    : UiaException(message, hresult, innerException);
