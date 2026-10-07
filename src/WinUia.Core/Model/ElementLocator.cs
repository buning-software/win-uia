using System.Linq.Expressions;
using WinUia.Core.Interop;

namespace WinUia;

// Index is the position among FindAll results (null for FindFirst); it can shift, so re-resolving confirms the match via ElementIdentity.
internal sealed record ElementLocator(ElementHandle? Parent, Expression<Func<Element, bool>> Search, TreeScope Scope, int? Index)
{
    public static readonly ElementLocator Root = new(null, ElementPredicate.MatchAll, TreeScope.Element, Index: null);

    public IUIAutomationElement? ResolveOnce(AutomationContext context, ElementIdentity? identity)
    {
        if (Parent is null)
            return ComExceptionMapper.Invoke(() => context.Automation.GetRootElement());

        var condition = context.CreateCondition(Search);
        if (Index is not { } index)
            return Parent.GetResolvingOnce(p => p.FindFirst(Scope, condition));

        if (identity is null)
            return null; // Never read before it went stale: nothing to confirm a candidate against.

        return Parent.GetResolvingOnce(p =>
        {
            var all = p.FindAll(Scope, condition);
            var length = all.Length;
            if (index < length && all.GetElement(index) is var atIndex && identity.Matches(atIndex))
                return atIndex;

            for (var i = 0; i < length; i++)
            {
                var candidate = all.GetElement(i);
                if (identity.Matches(candidate))
                    return candidate;
            }

            return null;
        });
    }

    public override string ToString() => Parent is null ? "<root>" : $"{Scope} {ElementPredicate.Describe(Search)}" + (Index is { } i ? $" [{i}]" : "");
}
