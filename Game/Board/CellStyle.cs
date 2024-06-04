namespace MineSweeper;

/// <summary>
/// The style of a "Mine Sweeper" cell. Specifies to <see cref="Game"/> and <see cref="Board"/> how a cell should be displayed, based on it's type
/// </summary>
public struct CellStyle
{
    public ConsoleColor? Foreground {get; init;}
    public ConsoleColor? Background {get; init;}
    public char? Symbol {get; init;}

    public Pixel GetPixel(Point? screenPos, GameStyle style) => new()
    {
        Position = screenPos,
        Foreground = Foreground ?? style.DefaultForeground ?? OutputSystem.Foreground,
        Background = Background ?? style.DefaultBackground  ?? OutputSystem.Background,
        Character = Symbol ?? ' ',
    };

    public static CellStyle Overlay(CellStyle bottom, CellStyle top)
    {
        return new()
        {
            Background = top.Background == GameStyle.COLOR_INVERTED ? GameStyle.Invert(bottom.Background) : top.Background ?? bottom.Background,
            Foreground = top.Foreground == GameStyle.COLOR_INVERTED ? GameStyle.Invert(bottom.Foreground) : top.Foreground ?? bottom.Foreground,
            Symbol = top.Symbol ?? bottom.Symbol,
        };
    }
}