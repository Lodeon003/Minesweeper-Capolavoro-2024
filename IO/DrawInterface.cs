using System.Buffers;

namespace MineSweeper;

/// <summary>
/// Used as an interface to let the users of the graphics library to interact with the <see cref="OutputSystem"/><br/>
/// Provides methods to write text to the screen, change the cursor's position and color.
/// </summary>
public class DrawInterface
{
    internal event EventHandler<Pixel>? ActionRequested;
    internal event EventHandler<ReadOnlyMemory<char>>? StringRequested;

    private List<char[]> _rentedArrays = new();
    private bool _drawCallHappening = false;

    /// <summary>
    /// Event invoked by OutputSystem before invoking a <see cref="DrawCallback"/> using this instance
    /// </summary> 
    internal void BeginDrawCall()
    {
        if(_drawCallHappening)
            throw new InvalidOperationException("A draw call was already started on this object");

        _drawCallHappening = true;
    }

    internal void EndDrawCall()
    {
        if(!_drawCallHappening)
            throw new InvalidOperationException("A draw call was already stopped on this object");
            
        _drawCallHappening = false;

        for(int i = 0; i < _rentedArrays.Count; i++)
            ArrayPool<char>.Shared.Return(_rentedArrays[i]);
    }

    public void SetBackground(ConsoleColor? color)
    {
        Pixel pixel = new Pixel()
        {
            Background = color ?? OutputSystem.Background
        };

        ActionRequested?.Invoke(null, pixel);
    }

    public void SetForeground(ConsoleColor? color)
    {
        Pixel pixel = new Pixel()
        {
            Foreground = color ?? OutputSystem.Foreground
        };

        ActionRequested?.Invoke(null, pixel);
    }

    public void SetCursorPosition(int x, int y)
        => SetCursorPosition(new(x, y));

    public void SetCursorPosition(Point position)
    {
        Pixel pixel = new Pixel()
        {
            Position = position
        };

        ActionRequested?.Invoke(null, pixel);
    }

    public void Write(Pixel pixel)
    {
        ActionRequested?.Invoke(null, pixel);
    }

    public void Write(char c)
    {
        Pixel pixel = new Pixel()
        {
            Character = c
        };
        
        ActionRequested?.Invoke(null, pixel);
    }

    public void Write(ReadOnlyMemory<char> s)
    {

        StringRequested?.Invoke(null, s);
    }

    public void Write(ReadOnlySpan<char> s)
    {
        // Using a pool array because Span<char> can't be passed to events as the stack unwinds.
        // This is added to a list and will be freed after the draw call is finished.
        char[] buffer = ArrayPool<char>.Shared.Rent(s.Length);
        _rentedArrays.Add(buffer);

        s.CopyTo(buffer.AsSpan());
        ReadOnlyMemory<char> output = buffer[..s.Length];

        StringRequested?.Invoke(null, output);
    }

    public void Write(string s)
    {
        ThrowIfCallNotBegun();
        StringRequested?.Invoke(null, s.AsMemory());
    }

    private void ThrowIfCallNotBegun()
    {
        if(!_drawCallHappening)
            throw new InvalidOperationException($"This object was used to write but a draw call wasn't started via \'{nameof(BeginDrawCall)}\'");
    }
}