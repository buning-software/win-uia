namespace WinUia.Core.UnitTests.Helpers;

internal sealed record AutomationIdValue(string Value)
{
    public static implicit operator string(AutomationIdValue id) => id.Value;
}
