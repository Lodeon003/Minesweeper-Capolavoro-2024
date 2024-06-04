namespace MineSweeper;
using System.Runtime.InteropServices;

/// <summary>
/// A class that handles Windows specific functions contained in <c>Kernel32</c> and <c>User32</c> using interoperability and <see cref="DllImportAttribute"/>
/// </summary>
public static class WindowsNative
{
     private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    /// <summary>
    /// If running on Windows uses <see cref="GetConsoleMode"/> and <see cref="SetConsoleMode"/> to enable "virtual terminal processing"
    /// </summary>
    /// <returns>true if OS is <b>not</b> Windows (no need to enable colors manually) or if OS <b>is</b> Windows the operation succeded. Otherwise, false</returns>
    public static bool EnableColors()
    {
        if(!OperatingSystem.IsWindows())
            return true;

        // Get the handle to the standard output stream
        var handle = GetStdHandle(STD_OUTPUT_HANDLE);

        // Get the current console mode
        uint mode;
        if (!GetConsoleMode(handle, out mode))
        {
            Console.Error.WriteLine("Failed to get console mode");
            return false;
        }

        // Enable the virtual terminal processing mode
        mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
        if (!SetConsoleMode(handle, mode))
        {
            Console.Error.WriteLine("Failed to set console mode");
            return false;
        }

        return true;
    }
}
