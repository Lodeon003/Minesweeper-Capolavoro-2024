using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper;

/// <summary>
/// A multi-threading output system to use instead of blocking single-thread <see cref="Console"/>'s methods.<br/>
/// Optimized to reduce the overhead of frequently-drawn elements.
/// </summary>
public class OutputSystem
{
    private static OptimizedQueue<string, DrawCallback>? _events;
    private static CancellationTokenSource? _cts;
    private static ManualResetEvent? _handle;
    private static bool _running = false;

    public static ConsoleColor Foreground { get; private set; }
    public static ConsoleColor Background { get; private set; }

    public static void Start(ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        if (_running)
            return;

        try {
            Console.CursorVisible = false;
        }
        finally{
            
        }

        try {
            Foreground = foreground ?? Console.ForegroundColor;
            Background = background ?? Console.BackgroundColor;
        }
        catch {
            Foreground = ConsoleColor.White;
            Foreground = ConsoleColor.Black;
        }

        _running = true;
        _events = new();

        _cts = new();
        _handle = new(false);

        Thread thread = new Thread(Loop)
        {
            Name = "Output Thread"
        };

        thread.Start();
    }

    public static void Stop()
    {
        if (!_running)
            return;

        _running = false;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _events = null;

        _handle?.Dispose();
        _handle = null;
    }

    private static DrawInterface _drawInterface = new();
    
    private static List<char> _buffer = new(20);
    private static void Loop()
    {
        _drawInterface.ActionRequested += ActionRequested;
        _drawInterface.StringRequested += StringRequested;

        while (_running && !_cts.IsCancellationRequested)
        {
            _drawInterface.BeginDrawCall();

            _handle.WaitOne();
            _handle.Reset();

            while (_events.TryPeek(out DrawCallback? drawCall, out Guid uniqueID, out string? label))
            {
                if (drawCall is null)
                    continue;

                Debug.WriteLine($"[{nameof(OutputSystem)}] Processed call with name \'{label}\'");

                drawCall.Invoke(_drawInterface);
                Console.Out.Write(CollectionsMarshal.AsSpan(_buffer));
                _buffer.Clear();

                // Wait until the value was processed to remove it, to prevent
                // adding another one as soon as it starts being processed (might take some time and we dont want it to run again)
                _events.TryRemove(uniqueID);
            }

            _drawInterface.EndDrawCall();
        }

        _drawInterface.ActionRequested -= ActionRequested;
        _drawInterface.StringRequested -= StringRequested;
    }

    private static void StringRequested(object? sender, ReadOnlyMemory<char> s)
        => _buffer.AddRange(s.Span);

    private static void ActionRequested(object? sender, Pixel e)
    {
        if(e.Position is Point position)
        {
            Span<char> code = stackalloc char[Pixel.POSITION_CODE_LENGTH];
            Pixel.SetPositionCode(code, position);
            _buffer.AddRange(code);
        }

        if(e.Foreground is ConsoleColor foreground)
        {
            Span<char> code = stackalloc char[Pixel.COLOR_CODE_LENGTH];
            Pixel.SetForegroundCode(code, foreground);
            _buffer.AddRange(code);
        }

        if (e.Background is ConsoleColor background)
        {
            Span<char> code = stackalloc char[Pixel.COLOR_CODE_LENGTH];
            Pixel.SetBackgroundCode(code, background);
            _buffer.AddRange(code);
        }

        if (e.Character is char character)
            _buffer.Add(character);
    }

    private static void ThrowIfNotStarted()
    {
        if(!_running)
            throw new InvalidOperationException($"You must call {nameof(Start)} before executing output system calls");
    }

    public static void Write(string label, QueueOptimization behaviour, DrawCallback action)
    {
        ThrowIfNotStarted();

        if (action is null)
            return;

        _events.Enqueue(label, action, behaviour);
        _handle.Set();
    }

    public static void Write(DrawCallback action)
    {
        ThrowIfNotStarted();

        if (action is null)
            return;

        _events.Enqueue(action);
        _handle.Set();
    }

    public static void Write(string label, string text, QueueOptimization behaviour = QueueOptimization.Newest)
    {
        ThrowIfNotStarted();

        _events.Enqueue(label, (x) => x.Write(text), behaviour);
        _handle.Set();
    }

    public static void Write(string label, Pixel pixel, QueueOptimization behaviour = QueueOptimization.Newest)
    {
        ThrowIfNotStarted();

        _events.Enqueue(label, (x) => x.Write(pixel), behaviour);
        _handle.Set();
    }

    public static void Write(string label, Point position, string text, ConsoleColor? foreground = null, ConsoleColor? background = null, QueueOptimization behaviour = QueueOptimization.Newest)
        => Write(label, position.X, position.Y, text, foreground, background, behaviour);

    public static void Write(string label, Point position, ReadOnlySpan<char> text, ConsoleColor? foreground = null, ConsoleColor? background = null, QueueOptimization behaviour = QueueOptimization.Newest)
    {
        ThrowIfNotStarted();

        /// Rented buffer will stay in memory until freed in the callback,
        // aka when the draw call is processed. Draw calls shouldn't take more than a few seconds
        // so keeping it for a while shouldn't be a problem.
        char[] buffer = ArrayPool<char>.Shared.Rent(text.Length);
        
        // Length stored separately because "Rent" may return an array bigger than "text.length".
        // A pool buffer is used to avoid allocations. This because a "span<char>" cant be stored in a callback procedure,
        // as it is invoked in another context, with another stack.
        text.CopyTo(buffer);
        
        int length = text.Length;
        ReadOnlyMemory<char> output = buffer.AsMemory()[..length];

        DrawCallback action = (e) =>
        {
            Pixel formatting = new()
            {
                Position = position,
                Foreground = foreground ?? OutputSystem.Foreground,
                Background = background  ?? OutputSystem.Background,
            };

            e.Write(formatting);
            e.Write(output);

            // Here return the array *inside of callback*
            // It will remain rented until draw call is processed
            ArrayPool<char>.Shared.Return(buffer);
        };

        _events.Enqueue(label, action, behaviour);
        _handle.Set();
    }
    
    public static void Write(string label, int x, int y, string text, ConsoleColor? foreground = null, ConsoleColor? background = null, QueueOptimization behaviour = QueueOptimization.Newest)
    {
        ThrowIfNotStarted();

        DrawCallback action = (e) =>
        {
            Pixel formatting = new()
            {
                Position = new(x, y),
                Foreground = foreground ?? OutputSystem.Foreground,
                Background = background ?? OutputSystem.Background,
            };

            e.Write(formatting);
            e.Write(text);
        };

        _events.Enqueue(label, action, behaviour);
        _handle.Set();
    }
}
