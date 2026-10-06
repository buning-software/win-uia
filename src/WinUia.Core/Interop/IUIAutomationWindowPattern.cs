using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

[ComImport, Guid("0faef453-9208-43ef-bbb2-3b485177864f"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationWindowPattern
{
    void Close();
    [return: MarshalAs(UnmanagedType.Bool)]
    bool WaitForInputIdle(int milliseconds);
    void SetWindowVisualState(int state);
    bool CurrentCanMaximize { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentCanMinimize { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentIsModal { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentIsTopmost { [return: MarshalAs(UnmanagedType.Bool)] get; }
    int CurrentWindowVisualState { get; }
}
