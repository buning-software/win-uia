using System.Runtime.InteropServices;
using WinUia.Core.Interop;

namespace WinUia;

internal sealed record ElementIdentity(int ControlType, string AutomationId, string Name)
{
    public static ElementIdentity From(IUIAutomationElement raw) =>
        new(raw.CurrentControlType, raw.CurrentAutomationId ?? "", raw.CurrentName ?? "");

    public bool Matches(IUIAutomationElement candidate)
    {
        try
        {
            return candidate.CurrentControlType == ControlType
                && (candidate.CurrentAutomationId ?? "") == AutomationId
                && (AutomationId.Length > 0 || (candidate.CurrentName ?? "") == Name);
        }
        catch (COMException)
        {
            return false; // The candidate itself vanished; that says nothing about the parent.
        }
    }
}
