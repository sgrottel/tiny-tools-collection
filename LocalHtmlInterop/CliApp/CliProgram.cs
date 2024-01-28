using SGrottel;
using System.Text;

namespace LocalHtmlInterop
{
	internal class CliProgram
	{
		static void PrintGreeting()
		{
			Console.WriteLine("SGR Local Html Interop");
		}

		static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			PrintGreeting();

			ISimpleLog? log = null;
			try
			{
				log = new EchoingSimpleLog();



				log.Write($"Called:\n\t{string.Join("\n\t", args)}");



				log.Write("done.");
			}
			catch (Exception ex)
			{
				if (log == null)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.BackgroundColor = ConsoleColor.Black;
					Console.WriteLine($"EXCEPTION: {ex}");
					Console.ResetColor();
				}
				else
				{
					log.Write(ISimpleLog.FlagError, $"EXCEPTION: {ex}");
				}
			}
		}
	}
}
