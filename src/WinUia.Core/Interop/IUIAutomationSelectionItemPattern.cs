using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

[ComImport, Guid("a8efa66a-0fda-421a-9194-38021f3578ea"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationSelectionItemPattern
{
    void Select();
    void AddToSelection();
    void RemoveFromSelection();
    bool CurrentIsSelected { [return: MarshalAs(UnmanagedType.Bool)] get; }
}
