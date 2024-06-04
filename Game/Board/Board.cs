// A data-type that stores a cell and a position
global using PositionedCell = (MineSweeper.Cell cell, (int X, int Y) position);

namespace MineSweeper;

/// <summary>
/// A board containing minesweepers cells. Allows players to perform the in-game actions of "Mine Sweeper", such as digging and flagging.<br/>
/// Also contains methods to randomly distribute the bombs through the board itself.
/// </summary>
class Board
{
    public Statistics Stats {get; private set;}
    private Cell[,] _cells;

    public delegate void MultipleCellPosDel(PositionedCell[] values);
    public delegate void CellPosDel(int x, int y, Cell cell);
    public event CellPosDel? Discovered;
    public event MultipleCellPosDel? DiscoveredMultiple;
    public event CellPosDel? Flagged;
    public event CellPosDel? Lost;
    public event EventHandler? Won;

    public int Width => Stats.Width;
    public int Height => Stats.Height;

    private Board(Cell[,] cells, Statistics statistics)
    {
        ArgumentNullException.ThrowIfNull(cells);
        ArgumentNullException.ThrowIfNull(statistics);

        if(statistics.Width <= 0 || statistics.Height <= 0)
            throw new ArgumentException($"The board size can't be negative and must be more than 0");

        _cells = cells;
        Stats = statistics;
    }

    public Cell See(Point pos)
        => See(pos.X, pos.Y);

    public Cell See(int x, int y)
        => _cells[x, y].IsDiscovered ? _cells[x, y] : new(Kind.Unknown, _cells[x, y].IsFlagged, _cells[x, y].Extra);

    public Cell Get(int x, int y)
        => _cells[x, y];


    public void Discover(Point position)
        => Discover(position.X, position.Y);
    public void Discover(int x, int y)
    {
        // If the cell is already discovered do nothing
        if (_cells[x, y].IsDiscovered)
            return;

        // If bomb, you lose
        if (_cells[x, y].Kind == Kind.Bomb)
        {
            Lost?.Invoke(x, y, Get(x, y));
            return;
        }
        
        // Empty cells make you discover all empty area around you
        // Discover around this cell and do it for all other empty cells you find
        if(_cells[x, y].Kind == Kind.Empty) 
        {
            List<PositionedCell> discovered = new();
            DiscoverNeighboursRecursive(x, y, discovered);

            for (int i = 0; i < Stats.Height; i++)
                for (int j = 0; j < Stats.Width; j++)
                    _cells[j, i].Extra = false;
            
            DiscoveredMultiple?.Invoke(discovered.ToArray());
            return;
        }

        // If it's only a single cell discover it
        _cells[x, y].IsDiscovered = true;

        if(_cells[x, y].IsFlagged)
        {
            Stats.FlagsPlaced--;
            _cells[x, y].IsFlagged = false;
        }

        Stats.Discovered++;
        Discovered?.Invoke(x, y, See(x, y));
    }

    public void Flag(Point p)
        => Flag(p.X, p.Y);

    public void Flag(int x, int y)
    {
        if (_cells[x, y].IsDiscovered)
            return;

        _cells[x, y].IsFlagged = !_cells[x, y].IsFlagged;
        Stats.FlagsPlaced += _cells[x, y].IsFlagged ? 1 : -1;

        if(_cells[x, y].Kind == Kind.Bomb)
            Stats.BombsFlagged += _cells[x, y].IsFlagged ? 1 : -1;

        if(Stats.BombsFlagged == Stats.TotalBombs)
        {
            Won?.Invoke(this, EventArgs.Empty);
            return;
        }

        Flagged?.Invoke(x, y, See(x, y));
    }

    private void DiscoverNeighboursRecursive(int x, int y, List<PositionedCell> cells)
    {
        if (_cells[x, y].Extra)
            return;

        _cells[x, y].Extra = true;

        for (int j = -1; j <= 1; j++)
            for (int l = -1; l <= 1; l++)
            {
                try
                {
                    if(_cells[x, y].IsFlagged)
                    {
                        Stats.FlagsPlaced--;
                        _cells[x, y].IsFlagged = false;
                    }
                    _cells[x + j, y + l].IsDiscovered = true;
                    Stats.Discovered++;

                    cells.Add((_cells[x + j, y + l], (x + j, y + l)));

                    if (_cells[x + j, y + l].Kind == Kind.Empty)
                        DiscoverNeighboursRecursive(x + j, y + l, cells);
                }
                catch {}
            }
    }

    public void Reset()
    {
        int mines = 0;
        Stats = Generate(_cells, Stats.BombPercentage, Stats.Seed);
    }

    public static Board New(int width, int height, float minePercentage = 0.15f, int? seed = null)
    {
        if(minePercentage <= 0)
            throw new ArgumentException("Mine percentage can't be less than 0");

        if(width <= 0)
            throw new ArgumentException("width can't be less than 0");

        if(height <= 0)
            throw new ArgumentException("height can't be less than 0");

        Cell[,] cells = new Cell[width, height];

        Statistics stats = Generate(cells, minePercentage, seed);
        return new Board(cells, stats);
    }

    private static Statistics Generate(Cell[,] cells, float minePercentage, int? seed)
    {
        int width = cells.GetLength(0);
        int height = cells.GetLength(1);
        
        int resultSeed = seed ?? Guid.NewGuid().GetHashCode();
        Random random = new Random(resultSeed);

        int totalMines = (int)Math.Round(width * height * minePercentage);

        for (int j = 0; j < width; j++)
            for (int l = 0; l < height; l++)
                cells[j, l] = new(Kind.Empty);


        // For the amount of mines to place
        for (int i = 0; i < totalMines; i++)
        {
            // Chose a random place without a mine
            int x, y;
         
            do {
                x = random.Next(width);
                y = random.Next(height);
            }
            while (cells[x, y].Kind == Kind.Bomb);

            // Place a mine
            cells[x, y] = new(Kind.Bomb);

            // Increase the mine number on all neighbours by 1
            for (int j = -1; j <= 1; j++)
                for (int l = -1; l <= 1; l++)
                {
                    if(x + j < 0 || x + j >= width || y + l < 0 || y + l >= height)
                        continue;

                    // Ignore bombs (They dont track nearby bombs)
                    if (cells[x + j, y + l].Kind == Kind.Bomb)
                        continue;

                    // Kind numbers 0-8 rapresent nearby mines
                    // Adding 1 means one more mine nearby
                    cells[x + j, y + l].Kind += 1;
                }
        }
        int totalEmpty = 0;
        int totalCells = width*height;

        for (int j = 0; j < width; j++)
            for (int l = 0; l < height; l++)
                    if(cells[j, l].Kind == Kind.Empty)
                        totalEmpty++;

        return new()
        {
            Seed = resultSeed,
            Width = width,
            Height = height,
            TotalBombs = totalMines,
            TotalEmpty = totalEmpty,
            TotalNumbered = totalCells - totalEmpty - totalMines,
            BombPercentage = minePercentage,
        };
    }

    public class Statistics
    {
        public int TotalBombs {get;init;}
        public int TotalEmpty {get;init;}
        public int TotalNumbered {get;init;}
        public int Seed {get;init;}
        public int Width {get;init;}
        public int Height {get;init;}
        
        public int FlagsPlaced {get; internal set;}
        public int BombsFlagged {get; internal set;}
        public int Discovered {get; internal set;}
        public float BombPercentage { get; internal set; }
    }
}

class Cell
{
    public Cell() { }
    public Cell(Kind kind) { Kind = kind; }
    public Cell(Kind kind, bool flagged) { Kind = kind; IsFlagged = flagged; }
    public Cell(Kind kind, bool flagged, bool extra) { Kind = kind; IsFlagged = flagged; Extra = extra; }

    public Kind Kind = Kind.Empty;
    public bool IsFlagged = false;
    public bool IsDiscovered = false;
    public bool Extra = false;

    public static Cell Unknown(bool flagged, bool extra)
    {
        return new(Kind.Unknown, flagged, extra);
    }

    public override string ToString()
    {
        if(this.Kind >= Kind.One && this.Kind  <= Kind.Eight)
            return ((int)Kind).ToString();

        return Kind.ToString();
    }
}

public enum Kind
{
    Unknown = -1,
    Empty = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Bomb = 9,
}