using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

[ComImport, Guid("14314595-b4bc-4055-95f2-58f2e42c9855"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationElementArray
{
    int Length { get; }
    IUIAutomationElement GetElement(int index);
}
