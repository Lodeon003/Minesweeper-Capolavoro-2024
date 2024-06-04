using System.Security;
using System.Text;
using MineSweeper;

////// Program start //////

OutputSystem.Start();   // Enable grahpics processing (on another thread)

#region Setup & Error Display
string helpLink = "no link";

try {
    Console.OutputEncoding = Encoding.Unicode;
    Console.TreatControlCAsInput = true;
    Console.CursorVisible = false;
    Console.Title = "<< Lodeon's MinesweePvPr >>";
}
catch(SecurityException e)
{
    Console.WriteLine(
    $"""
    A security error occured during setup.
    Your terminal might not have the rights to execute this application.
    
    You may want to:
        • Run this app as Administrator
        • Restart your Computer
    
    If the problem persists contact the programmer at <{helpLink}>

    Error:
      {e.Message}
    """);

    InputSystem.ReadKey();
}
catch(IOException e)
{
    Console.WriteLine(
    $"""
    An unexpected error occured during setup.
    There might be compatibility issues with your terminal.
    
    Try again later, if the problem persists contact the programmer at <{helpLink}>

    Error:
      {e.Message}
    """);

    InputSystem.ReadKey();
}
catch(Exception e) when (e is PlatformNotSupportedException || e is InvalidOperationException)
{
    Console.WriteLine(
    $"""
    An error occured during setup. This can mean many things:
        • Your terminal does not support Unicode Encoding
        • Your terminal does not support hiding it's cursor
        • Your terminal does not support changing it's window title
        • Your terminal does not support treating CTRL+C as normal input

    Try again later, if the problem persists contact the programmer at <{helpLink}>

    Error:
      {e.Message}
    """);
    
    InputSystem.ReadKey();
}
catch(Exception e)
{
    Console.WriteLine(
    $"""
    An error occured during setup.
    Try again later, if the problem persists contact the programmer at <{helpLink}>

    Error:
      {e.Message}

    """);
    
    InputSystem.ReadKey();
}

if(!WindowsNative.EnableColors())
{
    Console.WriteLine(
    $"""
    An error occured while enabling colors
    There might be compatibility issues with your terminal.

    You may try:
        • Run the app again
        • Run the app in another terminal

    If the problem persists contact the programmer at <{helpLink}>

    YOU CAN STILL PLAY THE GAME
    Press [ANY] key to continue...
    """);

    InputSystem.ReadKey();
}
#endregion

// Game setup & settings
GameStyle style = GameStyle.DefaultStyle;
style.CellSpacingX = 0;
style.CellSpacingY = 0;

Game game = new((0, 0, 25, 12), new() { Style = style, Keybinds = GameKeybinds.WASD });
game.Start();

InputSystem.Run(); // Enable input processing (Blocks main thread)