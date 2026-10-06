using System.Linq.Expressions;
using System.Reflection;
using WinUia.Core.Interop;

namespace WinUia;

internal static class ElementPredicate
{
    private const string Supported =
        "Use ==, != on Name, AutomationId, ClassName, ControlType or ProcessId, combined with &&, || and !.";

    private static readonly ParameterExpression ElementParameter = Expression.Parameter(typeof(Element), "e");

    public static Expression<Func<Element, bool>> MatchAll { get; } = FromBody(Expression.Constant(true));

    public static Expression<Func<Element, bool>> Canonicalize(Expression<Func<Element, bool>> predicate) =>
        FromBody(Fold(predicate.Body, () => Expression.Constant(true), Equality, Expression.AndAlso, Expression.OrElse, Expression.Not));

    private static Expression<Func<Element, bool>> FromBody(Expression body) => Expression.Lambda<Func<Element, bool>>(body, ElementParameter);

    private static Expression Equality(string property, object value)
    {
        Expression member = Expression.Property(ElementParameter, property);
        if (member.Type.IsEnum)
            member = Expression.Convert(member, typeof(int)); // Enums compare as their underlying type, as the compiler emits.
        return Expression.Equal(member, Expression.Constant(value, member.Type));
    }

    public static string Describe(Expression<Func<Element, bool>> search) =>
        Fold<string>(search.Body, () => "True", (property, value) => $"{property}={value}",
            (left, right) => $"({left} AND {right})", (left, right) => $"({left} OR {right})", inner => $"NOT {inner}");

    public static IUIAutomationCondition ToUia(Expression<Func<Element, bool>> search, IUIAutomation automation) =>
        Fold<IUIAutomationCondition>(search.Body, automation.CreateTrueCondition,
            (property, value) => automation.CreatePropertyCondition(PropertyId(property), value),
            automation.CreateAndCondition, automation.CreateOrCondition, automation.CreateNotCondition);

    private static T Fold<T>(
        Expression body, Func<T> matchAll, Func<string, object, T> property,
        Func<T, T, T> allOf, Func<T, T, T> anyOf, Func<T, T> negate)
    {
        return Visit(body);

        T Visit(Expression node) => node switch
        {
            ConstantExpression { Value: true } => matchAll(),
            ConstantExpression { Value: false } => negate(matchAll()),
            BinaryExpression { NodeType: ExpressionType.AndAlso } both => allOf(Visit(both.Left), Visit(both.Right)),
            BinaryExpression { NodeType: ExpressionType.OrElse } either => anyOf(Visit(either.Left), Visit(either.Right)),
            UnaryExpression { NodeType: ExpressionType.Not } negation => negate(Visit(negation.Operand)),
            BinaryExpression { NodeType: ExpressionType.Equal } equal => Compare(equal, property),
            BinaryExpression { NodeType: ExpressionType.NotEqual } notEqual => negate(Compare(notEqual, property)),
            _ => throw new NotSupportedException($"'{node}' cannot be turned into a UI Automation condition. {Supported}"),
        };
    }

    private static T Compare<T>(BinaryExpression comparison, Func<string, object, T> property)
    {
        var (member, valueSide) = ElementProperty(comparison.Left) is { } left
            ? (left, comparison.Right)
            : (ElementProperty(comparison.Right), comparison.Left);
        if (member is null)
            throw new NotSupportedException($"'{comparison}' does not compare an element property. {Supported}");

        var value = Evaluate(valueSide)
            ?? throw new NotSupportedException($"'{comparison}' compares with null; element properties are never null.");

        var name = member.Member.Name;
        return name switch
        {
            nameof(Element.Name) or nameof(Element.AutomationId) or nameof(Element.ClassName) => property(name, (string)value),
            nameof(Element.ControlType) or nameof(Element.ProcessId) => property(name, Convert.ToInt32(value)),
            _ => throw new NotSupportedException($"Element.{name} cannot be searched on. {Supported}"),
        };
    }

    private static MemberExpression? ElementProperty(Expression side) =>
        StripConvert(side) is MemberExpression { Expression: ParameterExpression parameter } member && parameter.Type == typeof(Element)
            ? member
            : null;

    private static Expression StripConvert(Expression node) =>
        node is UnaryExpression { NodeType: ExpressionType.Convert } convert ? StripConvert(convert.Operand) : node;

    private static Expression StripLosslessConvert(Expression node) =>
        node is UnaryExpression { NodeType: ExpressionType.Convert, Method: null } convert
        && (convert.Type == typeof(object) || Unwrap(convert.Type) == Unwrap(convert.Operand.Type))
            ? StripLosslessConvert(convert.Operand)
            : node;

    private static Type Unwrap(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsEnum ? Enum.GetUnderlyingType(type) : type;
    }

    private static object? Evaluate(Expression side)
    {
        switch (StripLosslessConvert(side))
        {
            case ConstantExpression constant:
                return constant.Value;
            case MemberExpression { Expression: ConstantExpression closure, Member: FieldInfo field }:
                return field.GetValue(closure.Value);
        }

        try
        {
            return Expression.Lambda<Func<object?>>(Expression.Convert(side, typeof(object))).Compile()();
        }
        catch (InvalidOperationException ex)
        {
            throw new NotSupportedException($"'{side}' must be a value that does not depend on the element. {Supported}", ex);
        }
    }

    private static int PropertyId(string property) => property switch
    {
        nameof(Element.Name) => PropertyIds.Name,
        nameof(Element.AutomationId) => PropertyIds.AutomationId,
        nameof(Element.ClassName) => PropertyIds.ClassName,
        nameof(Element.ControlType) => PropertyIds.ControlType,
        nameof(Element.ProcessId) => PropertyIds.ProcessId,
        _ => throw new ArgumentOutOfRangeException(nameof(property), property, "Not a searchable element property."),
    };
}
