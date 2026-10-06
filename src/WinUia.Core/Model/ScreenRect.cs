namespace WinUia;

/// <summary>A screen rectangle in physical pixels.</summary>
public readonly record struct ScreenRect(int Left, int Top, int Right, int Bottom)
{
    /// <summary>Width in pixels.</summary>
    public int Width => Right - Left;

    /// <summary>Height in pixels.</summary>
    public int Height => Bottom - Top;

    /// <summary>True when the rectangle has no area.</summary>
    public bool IsEmpty => Width <= 0 || Height <= 0;

    /// <summary>The centre point.</summary>
    public ScreenPoint Center => new(Left + Width / 2, Top + Height / 2);
}
