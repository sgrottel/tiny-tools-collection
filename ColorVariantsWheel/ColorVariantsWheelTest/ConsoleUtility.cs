using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorVariantsWheelTest
{

    internal class ConsoleUtility
    {

        #region P/Invoke

        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private const int STD_OUTPUT_HANDLE = -11;

        private const IntPtr INVALID_HANDLE_VALUE = -1;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        #endregion

        /// <summary>
        /// Queries if Virtual Terminal Processing is enabled, tries to enable it if not and queries again.
        /// </summary>
        /// <returns>True if Virtual Terminal Processing is enabled.</returns>
        internal static bool EnableVirtualTerminalProcessing()
        {
            IntPtr hOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hOut == INVALID_HANDLE_VALUE) return false;
            if (hOut == IntPtr.Zero) return false;

            uint mode;
            if (!GetConsoleMode(hOut, out mode)) return false;

            if ((mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) == ENABLE_VIRTUAL_TERMINAL_PROCESSING) return true;

            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            if (!SetConsoleMode(hOut, mode)) return false;

            if (!GetConsoleMode(hOut, out mode)) return false;

            return (mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) == ENABLE_VIRTUAL_TERMINAL_PROCESSING;
        }

        internal static string VtsFC(byte r, byte g, byte b)
        {
            return $"\x1b[38;2;{r};{g};{b}m";
        }

        internal static string VtsBC(byte r, byte g, byte b)
        {
            return $"\x1b[48;2;{r};{g};{b}m";
        }

        internal const string VtsReset = "\x1B[0m";

    }

}
