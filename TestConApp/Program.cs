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

        private enum ArgParseState
        {
            None,
            Sleep
        };

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("TestConApp 🧫");

            Console.WriteLine("Virtual Terminal Processing: {0}", CheckVirtualTerminalProcessing() ? "on" : "off");

            if (OperatingSystem.IsWindows())
            {
                Console.WriteLine("UAC: {0}", UacHelper.IsUacEnabled ? "on" : "off");
                Console.WriteLine("RunAsAdmin: {0}", UacHelper.IsProcessElevated ? "on" : "off");
            }

            Console.WriteLine("Line 1 on StdOut");
            Console.Error.WriteLine("Line 2 on StdErr");
            Console.WriteLine("Line 3 on StdOut");

            Console.WriteLine("Writing multiple\nLines in one\nCall.");

            Console.Write($"{args.Length} command line argument");
            if (args.Length != 1) Console.Write("s");
            Console.WriteLine((args.Length > 0) ? ":" : ".");

            ArgParseState state = ArgParseState.None;
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine($"[{i}] {args[i]}");
                switch (state)
                {
                    case ArgParseState.None:
                        if (args[i].Equals("-sleep", StringComparison.CurrentCultureIgnoreCase))
                        {
                            state = ArgParseState.Sleep;
                        }
                        break;

                    case ArgParseState.Sleep:
                        {
                            int millis = 0;
                            int.TryParse(args[i], out millis);
                            if (millis > 0)
                            {
                                Console.WriteLine($"Sleeping {millis} ms...💤");
                                Thread.Sleep(millis);
                            }
                            else
                            {
                                Console.WriteLine("'-sleep' requires a number greater zero of milliseconds to work");
                            }
                            state = ArgParseState.None;
                        }
                        break;
                }
            }

            Console.Write("End");
        }
    }
}