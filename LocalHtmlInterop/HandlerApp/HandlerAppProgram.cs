using SGrottel;

namespace LocalHtmlInterop.Handler
{
	internal class HandlerAppProgram
	{
		static void Main(string[] args)
		{
			ISimpleLog? log = null;
			try
			{
				log = new SimpleLog();


				log.Write($"Called:\n\t{string.Join("\n\t", args)}");

				CustomUrlProtocol.RegisterAsHandler(Path.Combine(AppContext.BaseDirectory, "LocalHtmlInterop.exe"));


				log.Write("done.");
			}
			catch (Exception ex)
			{
				log?.Write(ISimpleLog.FlagError, $"EXCEPTION: {ex}");
			}
		}
	}
}
