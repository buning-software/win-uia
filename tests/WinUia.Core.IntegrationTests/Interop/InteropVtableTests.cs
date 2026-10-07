namespace WinUia.Core.IntegrationTests.Interop;

public class InteropVtableTests
{
    private static void AssertTypedGettersMatchPropertyValues(Element element)
    {
        var raw = element.Handle.Raw;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(raw.CurrentName, Is.EqualTo(raw.GetCurrentPropertyValue(30005) ?? ""));
            Assert.That(raw.CurrentAutomationId, Is.EqualTo(raw.GetCurrentPropertyValue(30011) ?? ""));
            Assert.That(raw.CurrentClassName, Is.EqualTo(raw.GetCurrentPropertyValue(30012) ?? ""));
            Assert.That(raw.CurrentFrameworkId, Is.EqualTo(raw.GetCurrentPropertyValue(30024) ?? ""));
            Assert.That(raw.CurrentLocalizedControlType ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30004) ?? ""));
            Assert.That(raw.CurrentControlType, Is.EqualTo(raw.GetCurrentPropertyValue(30003)));
            Assert.That(raw.CurrentProcessId, Is.EqualTo(raw.GetCurrentPropertyValue(30002)));
            Assert.That(raw.CurrentIsEnabled, Is.EqualTo(raw.GetCurrentPropertyValue(30010)));
            Assert.That(raw.CurrentIsOffscreen, Is.EqualTo(raw.GetCurrentPropertyValue(30022)));
            Assert.That(raw.CurrentHasKeyboardFocus, Is.EqualTo(raw.GetCurrentPropertyValue(30008)));
            Assert.That(raw.CurrentIsKeyboardFocusable, Is.EqualTo(raw.GetCurrentPropertyValue(30009)));
            Assert.That(raw.CurrentIsControlElement, Is.EqualTo(raw.GetCurrentPropertyValue(30016)));
            Assert.That(raw.CurrentIsPassword, Is.EqualTo(raw.GetCurrentPropertyValue(30019)));
            Assert.That((long)raw.CurrentNativeWindowHandle, Is.EqualTo(Convert.ToInt64(raw.GetCurrentPropertyValue(30020))));
            Assert.That(raw.CurrentAcceleratorKey ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30006) ?? ""));
            Assert.That(raw.CurrentAccessKey ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30007) ?? ""));
            Assert.That(raw.CurrentHelpText ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30013) ?? ""));
            Assert.That(raw.CurrentCulture, Is.EqualTo(raw.GetCurrentPropertyValue(30015)));
            Assert.That(raw.CurrentIsContentElement, Is.EqualTo(raw.GetCurrentPropertyValue(30017)));
            Assert.That(raw.CurrentItemType ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30021) ?? ""));
            Assert.That(raw.CurrentOrientation, Is.EqualTo(raw.GetCurrentPropertyValue(30023)));
            Assert.That(raw.CurrentIsRequiredForForm, Is.EqualTo(raw.GetCurrentPropertyValue(30025)));
            Assert.That(raw.CurrentItemStatus ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30026) ?? ""));

            var bounds = (double[])raw.GetCurrentPropertyValue(30001)!; // l, t, w, h
            var rect = raw.CurrentBoundingRectangle;
            Assert.That(rect.left, Is.EqualTo((int)bounds[0]));
            Assert.That(rect.top, Is.EqualTo((int)bounds[1]));
            Assert.That(rect.right, Is.EqualTo((int)(bounds[0] + bounds[2])));
            Assert.That(rect.bottom, Is.EqualTo((int)(bounds[1] + bounds[3])));

            Assert.That(raw.GetRuntimeId(), Is.Not.Empty);
        }
    }

    [Test]
    public void Root_element_typed_getters_match_property_values()
    {
        using var context = new AutomationContext();

        AssertTypedGettersMatchPropertyValues(context.GetRootElement());
    }

    [Test]
    public void Root_element_is_same_as_itself_and_has_children()
    {
        using var context = new AutomationContext();
        var root = context.GetRootElement();

        Assert.That(root.IsSameAs(context.GetRootElement()), Is.True);
        Assert.That(root.FindAll(e => true, TreeScope.Children), Is.Not.Empty);
    }
}
