using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

[ComImport, Guid("a543cc6a-f4ae-494b-8239-c814481187a8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationTextRange
{
    void Reserved_Clone();
    void Reserved_Compare();
    void Reserved_CompareEndpoints();
    void Reserved_ExpandToEnclosingUnit();
    void Reserved_FindAttribute();
    void Reserved_FindText();
    void Reserved_GetAttributeValue();
    void Reserved_GetBoundingRectangles();
    void Reserved_GetEnclosingElement();
    [return: MarshalAs(UnmanagedType.BStr)]
    string? GetText(int maxLength);
}
