using System.Runtime.InteropServices;

namespace WinUia.Core.UnitTests.Exceptions;

public class ComExceptionMapperTests
{
    private static readonly object[] Mappings =
    [
        new object[] { 0x80040201u, typeof(UiaStaleElementException) },       // UIA_E_ELEMENTNOTAVAILABLE
        new object[] { 0x80010108u, typeof(UiaStaleElementException) },       // RPC_E_DISCONNECTED
        new object[] { 0x800706BAu, typeof(UiaStaleElementException) },       // RPC_S_SERVER_UNAVAILABLE
        new object[] { 0x800706BEu, typeof(UiaStaleElementException) },       // RPC_S_CALL_FAILED
        new object[] { 0x80040200u, typeof(UiaException)        },  // UIA_E_ELEMENTNOTENABLED
        new object[] { 0x80040202u, typeof(UiaException)        },   // UIA_E_NOCLICKABLEPOINT
        new object[] { 0x80040204u, typeof(UiaPatternNotSupportedException) },// UIA_E_NOTSUPPORTED
        new object[] { 0x80131505u, typeof(UiaTimeoutException) },            // UIA_E_TIMEOUT
        new object[] { 0x80004005u, typeof(UiaException) },                   // E_FAIL is not "not found"
        new object[] { 0x80070057u, typeof(UiaException) },                   // E_INVALIDARG
    ];

    [TestCaseSource(nameof(Mappings))]
    public void Map_returns_expected_exception_type(uint hresult, Type expected)
    {
        var com = new COMException("boom", unchecked((int)hresult));

        var mapped = ComExceptionMapper.Map(com);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(mapped, Is.TypeOf(expected));
            Assert.That(mapped.HResult, Is.EqualTo(unchecked((int)hresult)));
            Assert.That(mapped.InnerException, Is.SameAs(com));
        }
    }

    [Test]
    public void All_mapped_exceptions_derive_from_UiaException()
    {
        foreach (object[] row in Mappings)
        {
            var mapped = ComExceptionMapper.Map(new COMException("x", unchecked((int)(uint)row[0])));
            Assert.That(mapped, Is.InstanceOf<UiaException>());
        }
    }

    [Test]
    public void Invoke_maps_HRESULTs_the_runtime_converts_to_other_exception_types()
    {
        // The COM interop layer throws TimeoutException for UIA_E_TIMEOUT (COR_E_TIMEOUT) and
        // NotImplementedException for E_NOTIMPL instead of COMException.
        Assert.Throws<UiaTimeoutException>(() => ComExceptionMapper.Invoke<int>(() => throw new TimeoutException()));
        Assert.Throws<UiaPatternNotSupportedException>(() => ComExceptionMapper.Invoke<int>(() => throw new NotImplementedException()));
    }

    [TestCase(0x80040201u, true)]
    [TestCase(0x80010108u, true)]
    [TestCase(0x80040200u, false)]
    [TestCase(0x80004005u, false)]
    public void IsStale_matches_stale_hresults(uint hresult, bool expected)
    {
        Assert.That(ComExceptionMapper.IsStale(unchecked((int)hresult)), Is.EqualTo(expected));
    }
}
