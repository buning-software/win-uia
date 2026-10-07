using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

[ComImport, Guid("a94cd8b1-0844-4cd6-9d2d-640537ab39e9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationValuePattern
{
    void SetValue([MarshalAs(UnmanagedType.BStr)] string val);
    string? CurrentValue { [return: MarshalAs(UnmanagedType.BStr)] get; }
    bool CurrentIsReadOnly { [return: MarshalAs(UnmanagedType.Bool)] get; }
}
