using System.Runtime.InteropServices;
using WinUia.Core.Interop;

namespace WinUia.Core.UnitTests.Interop;

public class NativeLayoutTests
{
    [Test]
    public void INPUT_has_the_Win32_size_for_the_process_bitness()
    {
        // sizeof(INPUT) is 40 bytes on 64-bit Windows and 28 bytes on 32-bit Windows.
        Assert.That(Marshal.SizeOf<INPUT>(), Is.EqualTo(Environment.Is64BitProcess ? 40 : 28));
    }

    [Test]
    public void INPUT_union_starts_at_the_pointer_aligned_offset()
    {
        Assert.That((int)Marshal.OffsetOf<INPUT>(nameof(INPUT.u)), Is.EqualTo(Environment.Is64BitProcess ? 8 : 4));
    }
}
