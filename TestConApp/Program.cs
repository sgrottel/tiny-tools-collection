using System.Runtime.InteropServices;
using System.Text;

namespace TestConApp
{
    internal class Program
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

        internal static bool CheckVirtualTerminalProcessing()
        {
            IntPtr hOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hOut == INVALID_HANDLE_VALUE) return false;
            if (hOut == IntPtr.Zero) return false;

            uint mode;
            if (!GetConsoleMode(hOut, out mode)) return false;

            if ((mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) == ENABLE_VIRTUAL_TERMINAL_PROCESSING) return true;

            return false;
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("TestConApp");

            Console.WriteLine("Virtual Terminal Processing: {0}", CheckVirtualTerminalProcessing() ? "on" : "off");

            if (OperatingSystem.IsWindows())
            {
                Console.WriteLine("UAC: {0}", UacHelper.IsUacEnabled ? "on" : "off");
                Console.WriteLine("RunAsAdmin: {0}", UacHelper.IsProcessElevated ? "on" : "off");
            }

            Console.WriteLine("Line 1 on StdOut");
            Console.Error.WriteLine("Line 2 on StdErr");
            Console.WriteLine("Line 3 on StdOut");

            Console.Write($"{args.Length} command line argument");
            if (args.Length != 1) Console.Write("s");
            Console.WriteLine((args.Length > 0) ? ":" : ".");
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine($"[{i}] {args[i]}");
            }

            Console.WriteLine("End.");
        }
    }
}