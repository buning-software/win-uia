namespace WinUia.Input;

/// <summary>Virtual-key codes for <see cref="IInputSimulator"/>.</summary>
public enum VirtualKey : ushort
{
#pragma warning disable CS1591 // Standard Win32 virtual-key names.
    Back = 0x08,
    Tab = 0x09,
    Enter = 0x0D,
    Shift = 0x10,
    Control = 0x11,
    Alt = 0x12,
    Escape = 0x1B,
    Space = 0x20,
    PageUp = 0x21,
    PageDown = 0x22,
    End = 0x23,
    Home = 0x24,
    Left = 0x25,
    Up = 0x26,
    Right = 0x27,
    Down = 0x28,
    Delete = 0x2E,
    A = 0x41,
    C = 0x43,
    V = 0x56,
    X = 0x58,
    Z = 0x5A,
    F4 = 0x73,
#pragma warning restore CS1591
}
