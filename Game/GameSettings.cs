namespace MineSweeper;

/// <summary>
/// Game settings. Used to customize a <see cref="Game"/> before starting it.<br/>
/// <see langword="null"/> values will be assigned to the default ones when starting the game.
/// </summary>
public class GameSettings
{
    public float MinePercentage { get; init; } = 0.15f;
    public int? Seed { get; init; } = null;
    public GameStyle? Style { get; init; } = null;
    public GameKeybinds? Keybinds { get; init; } = null;

    public static GameSettings Default {get;} = new();
}
