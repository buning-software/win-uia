using System.Runtime.InteropServices;

namespace WinUia.Core.Interop;

[ComImport, Guid("d22108aa-8ac5-49a5-837b-37bbb3d7591e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IUIAutomationElement
{
    void SetFocus();
    [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)]
    int[]? GetRuntimeId();
    IUIAutomationElement? FindFirst(TreeScope scope, IUIAutomationCondition condition);
    IUIAutomationElementArray FindAll(TreeScope scope, IUIAutomationCondition condition);
    void Reserved_FindFirstBuildCache();
    void Reserved_FindAllBuildCache();
    void Reserved_BuildUpdatedCache();
    [return: MarshalAs(UnmanagedType.Struct)]
    object? GetCurrentPropertyValue(int propertyId);
    void Reserved_GetCurrentPropertyValueEx();
    void Reserved_GetCachedPropertyValue();
    void Reserved_GetCachedPropertyValueEx();
    void Reserved_GetCurrentPatternAs();
    void Reserved_GetCachedPatternAs();
    [return: MarshalAs(UnmanagedType.IUnknown)]
    object? GetCurrentPattern(int patternId);
    void Reserved_GetCachedPattern();
    void Reserved_GetCachedParent();
    void Reserved_GetCachedChildren();
    int CurrentProcessId { get; }
    int CurrentControlType { get; }
    string? CurrentLocalizedControlType { [return: MarshalAs(UnmanagedType.BStr)] get; }
    string? CurrentName { [return: MarshalAs(UnmanagedType.BStr)] get; }
    string? CurrentAcceleratorKey { [return: MarshalAs(UnmanagedType.BStr)] get; }
    string? CurrentAccessKey { [return: MarshalAs(UnmanagedType.BStr)] get; }
    bool CurrentHasKeyboardFocus { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentIsKeyboardFocusable { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentIsEnabled { [return: MarshalAs(UnmanagedType.Bool)] get; }
    string? CurrentAutomationId { [return: MarshalAs(UnmanagedType.BStr)] get; }
    string? CurrentClassName { [return: MarshalAs(UnmanagedType.BStr)] get; }
    string? CurrentHelpText { [return: MarshalAs(UnmanagedType.BStr)] get; }
    int CurrentCulture { get; }
    bool CurrentIsControlElement { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentIsContentElement { [return: MarshalAs(UnmanagedType.Bool)] get; }
    bool CurrentIsPassword { [return: MarshalAs(UnmanagedType.Bool)] get; }
    nint CurrentNativeWindowHandle { get; }
    string? CurrentItemType { [return: MarshalAs(UnmanagedType.BStr)] get; }
    bool CurrentIsOffscreen { [return: MarshalAs(UnmanagedType.Bool)] get; }
    int CurrentOrientation { get; }
    string? CurrentFrameworkId { [return: MarshalAs(UnmanagedType.BStr)] get; }
    bool CurrentIsRequiredForForm { [return: MarshalAs(UnmanagedType.Bool)] get; }
    string? CurrentItemStatus { [return: MarshalAs(UnmanagedType.BStr)] get; }
    tagRECT CurrentBoundingRectangle { get; }
    void Reserved_get_CurrentLabeledBy();
    void Reserved_get_CurrentAriaRole();
    void Reserved_get_CurrentAriaProperties();
    void Reserved_get_CurrentIsDataValidForForm();
    void Reserved_get_CurrentControllerFor();
    void Reserved_get_CurrentDescribedBy();
    void Reserved_get_CurrentFlowsTo();
    void Reserved_get_CurrentProviderDescription();
    void Reserved_get_CachedProcessId();
    void Reserved_get_CachedControlType();
    void Reserved_get_CachedLocalizedControlType();
    void Reserved_get_CachedName();
    void Reserved_get_CachedAcceleratorKey();
    void Reserved_get_CachedAccessKey();
    void Reserved_get_CachedHasKeyboardFocus();
    void Reserved_get_CachedIsKeyboardFocusable();
    void Reserved_get_CachedIsEnabled();
    void Reserved_get_CachedAutomationId();
    void Reserved_get_CachedClassName();
    void Reserved_get_CachedHelpText();
    void Reserved_get_CachedCulture();
    void Reserved_get_CachedIsControlElement();
    void Reserved_get_CachedIsContentElement();
    void Reserved_get_CachedIsPassword();
    void Reserved_get_CachedNativeWindowHandle();
    void Reserved_get_CachedItemType();
    void Reserved_get_CachedIsOffscreen();
    void Reserved_get_CachedOrientation();
    void Reserved_get_CachedFrameworkId();
    void Reserved_get_CachedIsRequiredForForm();
    void Reserved_get_CachedItemStatus();
    void Reserved_get_CachedBoundingRectangle();
    void Reserved_get_CachedLabeledBy();
    void Reserved_get_CachedAriaRole();
    void Reserved_get_CachedAriaProperties();
    void Reserved_get_CachedIsDataValidForForm();
    void Reserved_get_CachedControllerFor();
    void Reserved_get_CachedDescribedBy();
    void Reserved_get_CachedFlowsTo();
    void Reserved_get_CachedProviderDescription();
    [return: MarshalAs(UnmanagedType.Bool)]
    bool GetClickablePoint(out tagPOINT clickable);
}
