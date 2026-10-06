using System.Linq.Expressions;

namespace WinUia.Core.UnitTests.Helpers;

public class ElementPredicateTests
{
    private static string Describe(Expression<Func<Element, bool>> predicate) =>
        ElementPredicate.Describe(ElementPredicate.Canonicalize(predicate));

    [Test]
    public void Translates_equality_combined_with_and_or_and_not()
    {
        var description = Describe(e =>
            (e.ControlType == ControlType.Button && e.Name == "OK") || !(e.AutomationId == "a") || e.ClassName != "c");

        Assert.That(description, Is.EqualTo("(((ControlType=50000 AND Name=OK) OR NOT AutomationId=a) OR NOT ClassName=c)"));
    }

    [Test]
    public void Reads_captured_values_on_either_side_when_canonicalized()
    {
        var name = "Tab 1";
        var processId = 42;

        // ReSharper disable once AccessToModifiedClosure (the point of the test: the change after canonicalizing is not seen)
        var search = ElementPredicate.Canonicalize(e => name == e.Name && e.ProcessId == processId);
        name = "changed";

        Assert.That(ElementPredicate.Describe(search), Is.EqualTo("(Name=Tab 1 AND ProcessId=42)"));
    }

    [Test]
    public void Evaluates_computed_values()
    {
        var names = new[] { "first", "second" };

        Assert.That(Describe(e => e.Name == names[1].ToUpperInvariant()), Is.EqualTo("Name=SECOND"));
    }

    [Test]
    public void Reads_instance_members_through_this()
    {
        var holder = new ProcessHolder(1234);

        Assert.That(Describe(holder.Predicate()), Is.EqualTo("ProcessId=1234"));
    }

    [Test]
    public void Applies_user_defined_conversions_on_the_value_side()
    {
        var id = new AutomationIdValue("btnOk");

        Assert.That(Describe(e => e.AutomationId == id), Is.EqualTo("AutomationId=btnOk"));
    }

    [Test]
    public void Applies_explicit_numeric_casts_the_way_CSharp_does()
    {
        var processId = 1234.7;

        Assert.That(Describe(e => e.ProcessId == (int)processId), Is.EqualTo("ProcessId=1234"));
    }

    [Test]
    public void Reads_a_captured_enum_as_its_control_type_id()
    {
        var controlType = ControlType.TabItem;

        Assert.That(Describe(e => e.ControlType == controlType), Is.EqualTo("ControlType=50019"));
    }

    [Test]
    public void True_matches_everything()
    {
        Assert.That(Describe(e => true), Is.EqualTo("True"));
    }

    [Test]
    public void Rejects_what_UIA_cannot_search_on()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.Throws<NotSupportedException>(() => ElementPredicate.Canonicalize(e => e.IsEnabled));
            Assert.Throws<NotSupportedException>(() => ElementPredicate.Canonicalize(e => e.Name.StartsWith("x")));
            Assert.Throws<NotSupportedException>(() => ElementPredicate.Canonicalize(e => e.Name == e.AutomationId));
            Assert.Throws<NotSupportedException>(() => ElementPredicate.Canonicalize(e => e.Name == null!));
        }
    }
}
