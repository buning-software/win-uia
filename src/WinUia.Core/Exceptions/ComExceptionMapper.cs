using System.Runtime.InteropServices;
using static WinUia.Core.Interop.HResults;

namespace WinUia;

internal static class ComExceptionMapper
{
    public static T Invoke<T>(Func<T> call)
    {
        try
        {
            return call();
        }
        // The runtime maps UIA_E_TIMEOUT and E_NOTIMPL to TimeoutException and NotImplementedException.
        catch (Exception ex) when (ex is COMException or TimeoutException or NotImplementedException)
        {
            throw Map(ex);
        }
    }

    public static bool IsStale(int hresult) =>
        hresult is UIA_E_ELEMENTNOTAVAILABLE or RPC_E_DISCONNECTED or RPC_S_SERVER_UNAVAILABLE or RPC_S_CALL_FAILED;

    public static UiaException Map(Exception ex)
    {
        var hr = ex.HResult;
        if (IsStale(hr))
            return new UiaStaleElementException("The element is no longer available.", hr, ex);

        return hr switch
        {
            UIA_E_ELEMENTNOTENABLED => new UiaException("The element is not enabled.", hr, ex),
            UIA_E_NOCLICKABLEPOINT => new UiaException("The element has no clickable point.", hr, ex),
            UIA_E_NOTSUPPORTED => new UiaPatternNotSupportedException("The operation is not supported by the element.", hr, ex),
            UIA_E_TIMEOUT => new UiaTimeoutException("The UI Automation call timed out.", hr, ex),
            E_NOTIMPL => new UiaPatternNotSupportedException("The element's provider does not implement this operation.", hr, ex),
            _ => new UiaException($"UI Automation call failed (0x{hr:X8}): {ex.Message}", hr, ex),
        };
    }
}
