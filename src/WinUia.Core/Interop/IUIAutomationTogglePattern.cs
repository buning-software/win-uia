using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

[ComImport, Guid("94cf8058-9b8d-4ab9-8bfd-4cd0a33c8c70"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationTogglePattern
{
    void Toggle();
    int CurrentToggleState { get; }
}
