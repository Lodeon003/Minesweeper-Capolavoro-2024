namespace MineSweeper;

/// <summary>
/// An area in two-dimensional space.
/// </summary>
public readonly struct Rect
{
    public Rect(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public int Left => X;
    public int Top => Y;
    public int Bottom => Y + Height;
    public int Right => X + Width;

    public readonly int X;
    public readonly int Y;
    public readonly int Width;
    public readonly int Height;

    public static implicit operator Rect(RectTuple tuple)
        => new(tuple.X, tuple.Y, tuple.Width, tuple.Height);

    public static implicit operator RectTuple(Rect rect)
        => new(rect.X, rect.Y, rect.Width, rect.Height);

    public override string ToString()
    {
        return $"{X}, {Y}, {Width}, {Height}";
    }
}
