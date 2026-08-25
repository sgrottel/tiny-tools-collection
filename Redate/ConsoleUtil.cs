using System;

namespace Redate;

internal class ConsoleUtil
{
	public static void WaitBeforeClosingConsole()
	{
		if (!IsSelfhostedConsole) return;
		Wait(defaultTimeoutSeconds);
	}

	private const int defaultTimeoutSeconds = 20;

	[System.Runtime.InteropServices.DllImport("kernel32.dll")]
	private static extern int GetConsoleProcessList(int[] buffer, int size);

	private static bool IsSelfhostedConsole
	{
		get
		{
			return GetConsoleProcessList(new int[2], 2) <= 1;
		}
	}

	private static void Wait(int timeoutSeconds)
	{
		if (timeoutSeconds == 0) return;
		if (Console.IsOutputRedirected) return;

		while (Console.KeyAvailable) Console.ReadKey();

		Console.Write("Hit any key to continue...");

		if (timeoutSeconds < 0)
		{
			Console.ReadKey();
		}
		else
		{
			DateTime start = DateTime.Now;
			while (((int)(DateTime.Now - start).TotalSeconds) < timeoutSeconds)
			{
				Console.Write("\rHit any key to continue... {0} ", timeoutSeconds - (int)(DateTime.Now - start).TotalSeconds);
				if (Console.KeyAvailable) break;
				System.Threading.Thread.Sleep(10);
			}
			Console.Write("\rHit any key to continue...     ");
		}

		while (Console.KeyAvailable) Console.ReadKey();
		Console.WriteLine();
	}

}
