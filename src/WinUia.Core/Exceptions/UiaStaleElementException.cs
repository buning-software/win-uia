using WinUia.Core.Interop;

namespace WinUia;

/// <summary>The element is no longer available, typically because its UI was re-rendered or closed (<c>UIA_E_ELEMENTNOTAVAILABLE</c>).</summary>
public class UiaStaleElementException(string message, int hresult = HResults.UIA_E_ELEMENTNOTAVAILABLE, Exception? innerException = null)
    : UiaException(message, hresult, innerException);
