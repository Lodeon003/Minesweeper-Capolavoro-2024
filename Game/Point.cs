namespace MineSweeper;

/// <summary>
/// A point in two-dimensional space.
/// </summary>
public struct Point
{
    public Point() {}

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X;
    public int Y;

    public static implicit operator Point(PointTuple tuple)
        => new(tuple.X, tuple.Y);

    public static implicit operator PointTuple(Point point)
        => (point.X, point.Y);

    internal void Deconstruct(out int screenX, out int screenY)
    {
        screenX = X;
        screenY = Y;
    }
}