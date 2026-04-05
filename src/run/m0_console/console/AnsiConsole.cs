using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace m0_console.console
{
    // Helper class for ANSI support - handles both Windows and Linux
    public static class AnsiConsole
    {
        // Windows API - Windows only
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        /// <summary>
        /// Enables ANSI escape sequence support - works on both Windows and Linux
        /// </summary>
        public static void EnableAnsiSupport()
        {
            // On Linux/macOS ANSI is enabled by default
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))            
                return;
            

            // Windows - enable ANSI support
            try
            {
                IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);

                if (stdHandle == IntPtr.Zero)
                    return;

                if (!GetConsoleMode(stdHandle, out uint mode))
                    return;

                mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                SetConsoleMode(stdHandle, mode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to enable ANSI support: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if ANSI is supported on the current platform
        /// </summary>
        public static bool IsAnsiSupported()
        {
            // Linux/macOS - always supported
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return true;

            // Windows - check if it was successfully enabled
            try
            {
                IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                if (stdHandle == IntPtr.Zero)
                    return false;

                if (!GetConsoleMode(stdHandle, out uint mode))
                    return false;

                return (mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) != 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
