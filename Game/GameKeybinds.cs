namespace MineSweeper;

/// <summary>
/// Maps keyboard keys to in-game actions. Used by <see cref="Game"/> to determine what action corresponds to the player's keypresses.<br/>
/// Default mappings are <see cref="WASD"/> and <see cref="Numpad"/>. Using them in parallel or creating others allows for multiple games to be played at once.
/// </summary>
public class GameKeybinds
{
    public ConsoleKey MoveLeft { get; set; }
    public ConsoleKey MoveUp { get; set; }
    public ConsoleKey MoveRight { get; set; }
    public ConsoleKey MoveDown { get; set; }
    public ConsoleKey Flag { get; set; }
    public ConsoleKey Dig { get; set; }
    public ConsoleKey Exit { get; set; }
    public ConsoleKey Confirm { get; set; }
    public ConsoleKey XRay { get; set; }

    public static GameKeybinds WASD => new()
    {
        MoveLeft = ConsoleKey.A,
        MoveUp = ConsoleKey.W,
        MoveRight = ConsoleKey.D,
        MoveDown = ConsoleKey.S,
        Flag = ConsoleKey.Q,
        Dig = ConsoleKey.E,
        XRay = ConsoleKey.Tab,
        Exit = ConsoleKey.Escape,
        Confirm = ConsoleKey.Spacebar,
    };

    public static GameKeybinds Numpad => new()
    {
        MoveLeft = ConsoleKey.NumPad4,
        MoveUp = ConsoleKey.NumPad8,
        MoveRight = ConsoleKey.NumPad6,
        MoveDown = ConsoleKey.NumPad5,
        Flag = ConsoleKey.NumPad7,
        Dig = ConsoleKey.NumPad9,
        XRay = ConsoleKey.NumPad0,
        Exit = ConsoleKey.Subtract,
        Confirm = ConsoleKey.Add,
    };
}