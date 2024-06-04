namespace MineSweeper;

/// <summary>
/// Rapresents a pixel on the screen.
/// </summary>
public struct Pixel
{
    public Pixel() {}

    public ConsoleColor Foreground = OutputSystem.Foreground;
    public ConsoleColor Background = OutputSystem.Background;
    public char? Character = null;
    public Point? Position = null;

    // [31m   <-- color-changing code
    public const int COLOR_CODE_LENGTH = 6;
    public const int POSITION_CODE_LENGTH = 10;
    //public const int TOTAL_LENGTH = 1 + COLOR_CODE_LENGTH * 2 + POSITION_CODE_LENGTH;

    public static void SetForegroundCode(Span<char> chars, ConsoleColor color)
    {
        chars[0] = '\x1b';
        chars[1] = '[';
        chars[5] = 'm';

        switch (color)
        {
            case ConsoleColor.Black:
                chars[2] = '0';
                chars[3] = '3';
                chars[4] = '0';
                break;

            case ConsoleColor.DarkRed:
                chars[2] = '0';
                chars[3] = '3';
                chars[4] = '1';
                break;

            case ConsoleColor.DarkGreen:
                chars[2] = '0';
                chars[3] = '3';
                chars[4] = '2';
                break;

            case ConsoleColor.DarkYellow:
                chars[2] = '0';
                chars[3] = '3';
                chars[4] = '3';
                break;

            case ConsoleColor.DarkBlue:
                chars[2] = '0';
                chars[3] = '3';
                chars[4] = '4';
                break;

            case ConsoleColor.DarkMagenta:
                chars[2] = '0';
                chars[3] = '3';
                chars[4] = '5';
                break;

            case ConsoleColor.DarkCyan:
                chars[2] = '0';
                chars[3] = '3';
                chars[4] = '6';
                break;

            case ConsoleColor.Gray:
                chars[2] = '0';
                chars[3] = '3';
                chars[4] = '7';
                break;

            case ConsoleColor.DarkGray:
                chars[2] = '0';
                chars[3] = '9';
                chars[4] = '0';
                break;

            case ConsoleColor.Red:
                chars[2] = '0';
                chars[3] = '9';
                chars[4] = '1';
                break;

            case ConsoleColor.Green:
                chars[2] = '0';
                chars[3] = '9';
                chars[4] = '2';
                break;

            case ConsoleColor.Yellow:
                chars[2] = '0';
                chars[3] = '9';
                chars[4] = '3';
                break;

            case ConsoleColor.Blue:
                chars[2] = '0';
                chars[3] = '9';
                chars[4] = '4';
                break;

            case ConsoleColor.Magenta:
                chars[2] = '0';
                chars[3] = '9';
                chars[4] = '5';
                break;

            case ConsoleColor.Cyan:
                chars[2] = '0';
                chars[3] = '9';
                chars[4] = '6';
                break;

            case ConsoleColor.White:
                chars[2] = '0';
                chars[3] = '9';
                chars[4] = '7';
                break;
        }
    }

    public static void SetBackgroundCode(Span<char> chars, ConsoleColor color)
    {
        chars[0] = '\x1b';
        chars[1] = '[';
        chars[5] = 'm';

        switch (color)
        {
            case ConsoleColor.Black:
                chars[2] = '0';
                chars[3] = '4';
                chars[4] = '0';
                break;

            case ConsoleColor.DarkRed:
                chars[2] = '0';
                chars[3] = '4';
                chars[4] = '1';
                break;

            case ConsoleColor.DarkGreen:
                chars[2] = '0';
                chars[3] = '4';
                chars[4] = '2';
                break;

            case ConsoleColor.DarkYellow:
                chars[2] = '0';
                chars[3] = '4';
                chars[4] = '3';
                break;

            case ConsoleColor.DarkBlue:
                chars[2] = '0';
                chars[3] = '4';
                chars[4] = '4';
                break;

            case ConsoleColor.DarkMagenta:
                chars[2] = '0';
                chars[3] = '4';
                chars[4] = '5';
                break;

            case ConsoleColor.DarkCyan:
                chars[2] = '0';
                chars[3] = '4';
                chars[4] = '6';
                break;

            case ConsoleColor.Gray:
                chars[2] = '0';
                chars[3] = '4';
                chars[4] = '7';
                break;

            case ConsoleColor.DarkGray:
                chars[2] = '1';
                chars[3] = '0';
                chars[4] = '0';
                break;

            case ConsoleColor.Red:
                chars[2] = '1';
                chars[3] = '0';
                chars[4] = '1';
                break;

            case ConsoleColor.Green:
                chars[2] = '1';
                chars[3] = '0';
                chars[4] = '2';
                break;

            case ConsoleColor.Yellow:
                chars[2] = '1';
                chars[3] = '0';
                chars[4] = '3';
                break;

            case ConsoleColor.Blue:
                chars[2] = '1';
                chars[3] = '0';
                chars[4] = '4';
                break;

            case ConsoleColor.Magenta:
                chars[2] = '1';
                chars[3] = '0';
                chars[4] = '5';
                break;

            case ConsoleColor.Cyan:
                chars[2] = '1';
                chars[3] = '0';
                chars[4] = '6';
                break;

            case ConsoleColor.White:
                chars[2] = '1';
                chars[3] = '0';
                chars[4] = '7';
                break;
        }
    }

    public static void SetPositionCode(Span<char> chars, Point position)
        => SetPositionCode(chars, position.X, position.Y);

    public static void SetPositionCode(Span<char> chars, int x, int y)
    {
        chars[0] = '\x1b';
        chars[1] = '[';
        chars[5] = ';';
        chars[9] = 'H';

        (y+1).TryFormat(chars.Slice(2, 3), out _, format: "D3");
        (x+1).TryFormat(chars.Slice(6, 3), out _, format: "D3");
    }
}