using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

[ComImport, Guid("619be086-1f4e-4ee4-bafa-210128738730"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationExpandCollapsePattern
{
    void Expand();
    void Collapse();
    int CurrentExpandCollapseState { get; }
}
