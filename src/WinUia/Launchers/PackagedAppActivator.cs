using System.Runtime.InteropServices;
using WinUia.Interop;

namespace WinUia;

// IApplicationActivationManager returns the app's real process id; starting explorer.exe shell:AppsFolder\... only yields Explorer's.
internal static class PackagedAppActivator
{
    public static int Activate(string appUserModelId, string? arguments)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global (a COM coclass: the object behind it implements the interface)
        var manager = (IApplicationActivationManager)new ApplicationActivationManager();
        try
        {
            manager.ActivateApplication(appUserModelId, arguments, ActivateOptions.NoErrorUI, out var processId);
            return (int)processId;
        }
        catch (COMException ex)
        {
            throw new AppProcessException($"Could not activate packaged app '{appUserModelId}' (0x{ex.HResult:X8}): {ex.Message}", ex);
        }
        finally
        {
            Marshal.ReleaseComObject(manager);
        }
    }
}
