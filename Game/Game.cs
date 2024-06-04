global using PointTuple = (int X, int Y);
global using RectTuple = (int X, int Y, int Width, int Height);
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography;

namespace MineSweeper;

/// <summary>
/// Main "Mine Sweeper" game class. Works as a bridge between game logic classes such as <see cref="Board"/> and the player.<br/>
/// Allows the game to <see cref="Start"/>, <see cref="Pause"/>, <see cref="Resume"/>. Keeps track of score and time.
/// </summary>
public class Game : IDisposable
{
    public enum State
    {
        NotInitialized,
        Initializing,
        Starting,
        Running,
        Paused,
        Won,
        Lost,
    }

    // ....     Properties      ....

    public State CurrentState => _state;


    // ....     Systems     ....

    private Board _board;
    private CancellationTokenSource _cancellationSource;
    private Timer _headerTimer;


    // ....     Variables    ....
    private GameStyle _style;
    private GameKeybinds _keybinds;
    private State _state;
    private TimeSpan _pauseStart;
    private bool _displayClear = false;
    private bool _disposed = false;
    private Guid _guid;
    private TimeSpan _startTime;

    #region ....     Layout      ....

    private bool _running = true;    
    private Point _currentCell;
    private Point _lastCell;
    private Point _boardPos;
    private Point _headerPos;
    private Point _headerSize;
    private Point _seedPos;
    private Rect _borderArea;
    private TimeSpan _pauseTotalTime;

    #endregion


    public Game(Rect area, GameSettings? settings = null)
    {
        _state = State.Initializing;

        settings ??= GameSettings.Default;
        _board = Board.New(area.Width, area.Height, settings.MinePercentage, settings.Seed);
        
        _style = settings?.Style ?? GameStyle.DefaultStyle;
        _keybinds = settings?.Keybinds ?? GameKeybinds.WASD;
        _guid = Guid.NewGuid();

        _board.Discovered += Board_Discovered;
        _board.DiscoveredMultiple += Board_DiscoveredMultiple;
        _board.Lost += Board_Lost;
        _board.Won += Board_Won;
        //Handler.Instance.WindowResized += Handler_WindowResized;

        _borderArea = (area.Left, area.Top, _board.Width + 1, _board.Height + 3);

        _boardPos = (_borderArea.Left + 1, area.Top + 3);
        _headerPos = (_borderArea.Left + 1, _borderArea.Top + 1);
        _headerSize = (_borderArea.Right - 1 - _headerPos.X, 1);

        _seedPos = (_borderArea.Left, _borderArea.Bottom+1);
        
        _cancellationSource = new();
        _headerTimer = new Timer((_) => DisplayHeader(), null, Timeout.Infinite, 1000);
    }


    // ....     Public Methods      ....

    public void Start()
    {
        _state = State.Starting;
        InputSystem.KeyDown += HandleInput;
        DisplayAll();

        //InputSystem.ReadKey(_cancellationSource.Token);

        bool found = false;
        for(int y = 0; y < _board.Height && !found; y++)
        {
            for (int x = 0; y < _board.Width && !found; x++)
                if(_board.Get(x, y).Kind == Kind.Empty)
                {
                    _board.Discover(x, y);
                    found = true;
                }
        }

        // Wait for keypress

        _headerTimer.Change(1000, 1000);
        _state = State.Running;
        _startTime = DateTime.Now.TimeOfDay;
    }

    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _disposed = true;
        InputSystem.KeyDown -= HandleInput;
        _board.Discovered -= Board_Discovered;
        _board.DiscoveredMultiple -= Board_DiscoveredMultiple;
        _board.Lost -= Board_Lost;
        _board.Won -= Board_Won;
        
        _cancellationSource.Dispose();
        _headerTimer.Dispose();
    }  

    public void Pause()
    {
        if(_state != State.Running)
            return;

        _state = State.Paused;
        _pauseStart = DateTime.Now.TimeOfDay;
        InputSystem.KeyDown += HandleInput;
        _headerTimer.Change(0, -1);
    }

    public void Resume()
    {
        if(_state != State.Paused)
            return;

        _pauseTotalTime += DateTime.Now.TimeOfDay - _pauseStart;
        _state = State.Running;
    }

    public void DisplayAll()
    {
        OutputSystem.Write($"{_guid}_DisplayAll", QueueOptimization.Newest, (x) => {
            DisplayBorder();
            DisplayHeader();
            DisplayBoard();
            DisplayCursor();
            DisplaySeed();
        });
    }


    // ....     Board / Input Events    ....

    void Board_Won(object? sender, EventArgs e)
    {
        _running = false;
        _state = State.Won;

        DisplayHeader();
        _displayClear = true;
        DisplayBoard();

        _cancellationSource.Cancel();
    }

    void Board_Discovered(int x, int y, Cell cell)
    {
        Point point = CellToScreen(x, y);
        Pixel pixel = CellToPixel(cell, point);
    
        OutputSystem.Write($"{_guid}_DisplayCell_{point.X}_{point.Y}", pixel);
    }

    void Board_DiscoveredMultiple(PositionedCell[] values)
    {
        //int outLength = Pixel.TOTAL_LENGTH * values.Length;

        OutputSystem.Write((DrawInterface e) => {

            for(int i = 0; i < values.Length; i++)
            {
                Point pos = CellToScreen(values[i].position);
                Pixel pixel = CellToPixel(values[i].cell, pos);
                e.Write(pixel);
            }
        });
    }

    void Board_Lost(int x, int y, Cell cell)
    {
        _running = false;
        _state = State.Lost;

        DisplayHeader();
        _displayClear = true;
        DisplayBoard();

        _cancellationSource.Cancel();
    }

    void Handler_WindowResized(object? sender, Point newSize)
    {
        DisplayAll();
    }

    void HandleInput(object? e, ConsoleKeyInfo input)
    {
        if(_disposed)
            return;

        if (_cancellationSource.Token.IsCancellationRequested)
        {
            Dispose();
            return;
        }


        if (input.Key == _keybinds.MoveLeft)
        {
            _lastCell = _currentCell;

            if (_currentCell.X > 0)
                _currentCell.X--;
        }
        else if (input.Key == _keybinds.MoveUp)
        {
            _lastCell = _currentCell;

            if(_currentCell.Y > 0)
            _currentCell.Y--;
        }
        else if (input.Key == _keybinds.MoveRight)
        {
            _lastCell = _currentCell;

            if (_currentCell.X < _board.Width - 1)
                _currentCell.X++;
        }
        else if (input.Key == _keybinds.MoveDown)
        { 
            _lastCell = _currentCell;

            if (_currentCell.Y < _board.Height - 1)
                _currentCell.Y++;
        }
        else if (input.Key == _keybinds.Dig)
            _board.Discover(_currentCell);
        else if (input.Key == _keybinds.Flag)
            _board.Flag(_currentCell);
        else if (input.Key == _keybinds.XRay)
        {
            _displayClear = !_displayClear;
            DisplayBoard();
        }

        DisplayCursor();
    }

    
    
    // ....     Logic Methods       ....


    /// <summary>
    /// Returns a pixel rapresenting the cell. It may be transparent
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="screenPos"></param>
    /// <param name="showUnchecked"></param>
    /// <returns></returns>
    Pixel CellToPixel(Cell cell, Point? screenPos, bool showUnchecked = false)
    {
        if(!cell.IsDiscovered && !showUnchecked)
            cell = Cell.Unknown(cell.IsFlagged, cell.Extra);

        CellStyle style = _style.CellStyles[cell.Kind];

        if(cell.IsFlagged)
            style = CellStyle.Overlay(style, _style.Flag);

        return style.GetPixel(screenPos, _style);
    }

    Point CellToScreen(int x, int y)
        => (_boardPos.X + x + x * _style.CellSpacingX, _boardPos.Y + y + y * _style.CellSpacingY);

    Point CellToScreen(Point point)
        => CellToScreen(point.X, point.Y);


    // ....     Display     ....

    void DisplayCursor()
    {
        // Get new cursor
        Cell cell = _board.See(_currentCell);
        Point cellPos = CellToScreen(_currentCell);
        
        CellStyle cellStyle = _style.CellStyles[cell.Kind];
    
        if(cell.IsFlagged)
            cellStyle = CellStyle.Overlay(cellStyle, _style.Flag);

        cellStyle = CellStyle.Overlay(cellStyle, _style.Cursor);

        /* I keep this code to show how ugly it was
        if (_style.Cursor.Background is GameStyle.COLOR_TRANSPARENT)
            background = cell.IsFlagged && (_style.Flag.Background != GameStyle.COLOR_TRANSPARENT)
                                                        ? _style.Flag.Background : cellStyle.Background;
        else
            background = _style.Cursor.Background;

        if (_style.Cursor.Foreground is GameStyle.COLOR_TRANSPARENT)
            foreground = cell.IsFlagged && (_style.Flag.Foreground != GameStyle.COLOR_TRANSPARENT)
                                                        ? _style.Flag.Foreground : cellStyle.Foreground;
        else
            foreground = _style.Cursor.Foreground;*/



        OutputSystem.Write($"{_guid}_DisplayCell_{_lastCell.X}_{_lastCell.Y}", QueueOptimization.Newest, (e) =>
        {
            Pixel lastCell = CellToPixel(_board.Get(_lastCell.X, _lastCell.Y), CellToScreen(_lastCell.X, _lastCell.Y));
            e.Write(lastCell);
        });

        // Display cursor
        OutputSystem.Write($"{_guid}_DisplayCursor", QueueOptimization.Newest, (DrawInterface e) =>
        { 
            Pixel cursor = cellStyle.GetPixel(cellPos, _style);
            e.Write(cursor);
        });
    }

    void DisplaySeed()
        => OutputSystem.Write($"{_guid}_SeedHeader", _seedPos, $"SEED: {_board.Stats.Seed}", _style.DefaultForeground, _style.DefaultBackground);

    void DisplayHeader()
    {
        switch(_state)
        {
            case State.Initializing:
            case State.Starting:
                OutputSystem.Write($"{_guid}_DisplayHeader", _headerPos.X, _headerPos.Y, $"Welcome to MINESWEEPvPr", ConsoleColor.Blue, _style.DefaultBackground);
                break;

            case State.Running:
                TimeSpan gameDuration = DateTime.Now.TimeOfDay - _startTime;
                OutputSystem.Write($"{_guid}_DisplayHeader", _headerPos.X, _headerPos.Y, $"MSpvp  ♦ {_board.Stats.FlagsPlaced}/{_board.Stats.TotalBombs}  ◄ {gameDuration.ToString("mm\\:ss")} ►", _style.DefaultForeground, _style.DefaultBackground);
                break;

            case State.Won:
                OutputSystem.Write($"{_guid}_DisplayHeader", _headerPos.X, _headerPos.Y, $"☺ YOU WON ☺", ConsoleColor.Green, _style.DefaultBackground);
                break;

            case State.Lost:
                OutputSystem.Write($"{_guid}_DisplayHeader", _headerPos.X, _headerPos.Y, $"☻ YOU LOST ☻", ConsoleColor.DarkRed, _style.DefaultBackground);
                break;
        }
    }

    void DisplayBorder()
    {
        // Top horizontal
        OutputSystem.Write($"{_guid}_DisplayBorder", QueueOptimization.Newest, (DrawInterface e) =>
        {
            e.SetCursorPosition(_borderArea.Left, _borderArea.Top);
            e.SetBackground(_style.DefaultBackground);
            e.SetForeground(_style.DefaultForeground);
    
            e.Write('╔');

            for(int i = _borderArea.Left + 1; i < _borderArea.Right; i++)
                e.Write('═');    

            e.Write('╗');


            // Header horizontal
            e.SetCursorPosition(_borderArea.Left, _headerPos.Y+1);
            e.Write('╠');

            for(int i = _borderArea.Left + 1; i < _borderArea.Right; i++)
                e.Write('═');   

            e.Write('╢');


            // Bottom horizontal
            e.SetCursorPosition(_borderArea.Left, _borderArea.Bottom);
            e.Write('╚');
    
            for(int i = _borderArea.Left + 1; i < _borderArea.Right; i++)
                e.Write('═');    

            e.Write('╝');


            // Left Vertical
            for(int y = _borderArea.Top + 1; y < _borderArea.Bottom; y++)
            {
                e.SetCursorPosition(_borderArea.Left, y);
                e.Write('║');
            }

            // Right vertical
            for(int y = _borderArea.Top + 1; y < _borderArea.Bottom; y++)
            {
                e.SetCursorPosition(_borderArea.Right, y);
                e.Write('║');
            }
        });
    }

    void DisplayBoard()
    {
        OutputSystem.Write($"{_guid}_DisplayBoard", QueueOptimization.Newest, (e) =>
        {
            for (int y = 0; y < _board.Height; y++)
            {
                for (int x = 0; x < _board.Width; x++)
                {
                    Cell cell = _board.Get(x, y);
             
                    Point position = CellToScreen(x, y);
                    Pixel pixel = CellToPixel(cell, position, _displayClear);
                    e.Write(pixel);   
                }
            }
        });
    }
}