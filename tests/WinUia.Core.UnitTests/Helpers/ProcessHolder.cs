using System.Linq.Expressions;

namespace WinUia.Core.UnitTests.Helpers;

internal sealed class ProcessHolder(int id)
{
    public int Id { get; } = id;

    public Expression<Func<Element, bool>> Predicate() => e => e.ProcessId == Id;
}
