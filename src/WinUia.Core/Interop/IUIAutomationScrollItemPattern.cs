using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

[ComImport, Guid("b488300f-d015-4f19-9c29-bb595e3645ef"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationScrollItemPattern
{
    void ScrollIntoView();
}
