using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

[ComImport, Guid("4042c624-389c-4afc-a630-9df854a541fc"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationTreeWalker
{
    IUIAutomationElement? GetParentElement(IUIAutomationElement element);
}
