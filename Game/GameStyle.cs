using System.Collections.ObjectModel;

namespace MineSweeper;

/// <summary>
/// Maps cell types to cell styles (<see cref="CellStyle"/>). Used to determine how each cell type should be displayed.
/// </summary>
public class GameStyle
{
    public const ConsoleColor COLOR_INVERTED = (ConsoleColor)64;

    public ReadOnlyDictionary<Kind, CellStyle> CellStyles {get;set;}
    public CellStyle Flag {get; set;}
    public CellStyle Cursor {get; set;}

    public int CellSpacingX {get; set;}
    public int CellSpacingY {get; set;}
    
    public char DefaultSymbol {get; set;}
    public ConsoleColor? DefaultForeground {get; set;}
    public ConsoleColor? DefaultBackground {get; set;}
    
    public static GameStyle DefaultStyle => new()
    {
        DefaultSymbol = ' ',
        DefaultBackground = ConsoleColor.Black,
        DefaultForeground = ConsoleColor.White,

        CellSpacingX = 2,
        CellSpacingY = 1,

        Flag = new()
        {
            Foreground = ConsoleColor.Red,
            Background = ConsoleColor.DarkRed,
            Symbol = '⚑',
        },

        Cursor = new()
        {
            Foreground = ConsoleColor.White,
            Background = ConsoleColor.Magenta,
            Symbol = null,
        },


        CellStyles = new(new Dictionary<Kind, CellStyle>()
        {
            // Unknown
            {
                Kind.Unknown, new()
                {
                    Foreground = ConsoleColor.Green,
                    Background = ConsoleColor.Green,
                    Symbol = ' ',
                }
            },

            // Empty
            {
                Kind.Empty, new()
                {
                    Foreground = ConsoleColor.DarkYellow,
                    Background = ConsoleColor.DarkYellow,
                    Symbol = ' ',
                }
            },

            // One
            {
                Kind.One, new()
                {
                    Foreground = ConsoleColor.Blue,
                    Background = ConsoleColor.DarkYellow,
                    Symbol = '1',
                }
            },

            // Two
            {
                Kind.Two, new()
                {
                    Foreground = ConsoleColor.DarkGreen,
                    Background = ConsoleColor.DarkYellow,
                    Symbol = '2',
                }
            },


            // Three
            {
                Kind.Three, new()
                {
                    Foreground = ConsoleColor.Red,
                    Background = ConsoleColor.DarkYellow,
                    Symbol = '3',
                }
            },

            // Four
            {
                Kind.Four, new()
                {
                    Foreground = ConsoleColor.Magenta,
                    Background = ConsoleColor.DarkYellow,
                    Symbol = '4',
                }
            },

            // Five
            {
                Kind.Five, new()
                {
                    Foreground = ConsoleColor.Magenta,
                    Background = ConsoleColor.DarkYellow,
                    Symbol = '5',
                }
            },

            // Six
            {
                Kind.Six, new()
                {
                    Foreground = ConsoleColor.White,
                    Background = ConsoleColor.DarkYellow,
                    Symbol = '6',
                }
            },

            // Seven
            {
                Kind.Seven, new()
                {
                    Foreground = ConsoleColor.White,
                    Background = ConsoleColor.DarkYellow,
                    Symbol = '7',
                }
            },

            // Eight
            {
                Kind.Eight, new()
                {
                    Foreground = ConsoleColor.White,
                    Background = ConsoleColor.DarkYellow,
                    Symbol = '8',
                }
            },

            // Bomb
            {
                Kind.Bomb, new()
                {
                    Foreground = ConsoleColor.Red,
                    Background = ConsoleColor.DarkGray,
                    Symbol = '@',
                }
            }
        })
    };

    public ConsoleColor GetBackground(CellStyle style)
        => style.Background ?? DefaultBackground ?? OutputSystem.Background;

        public ConsoleColor GetForeground(CellStyle style)
        => style.Foreground ?? DefaultForeground ?? OutputSystem.Foreground;

        public char GetSymbol(CellStyle style)
        => style.Symbol is char notnull ? notnull : ' ';

        public static ConsoleColor? Invert(ConsoleColor? color)
        {
            if(color is not ConsoleColor clr)
                return null;

            return 16 - clr;
        }
}