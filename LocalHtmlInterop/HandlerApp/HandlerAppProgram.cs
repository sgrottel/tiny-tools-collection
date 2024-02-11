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

				ServerPortConfig port = new();
				port.SetValue(ServerPortConfig.DefaultPort);


				log.Write($"Called:\n\t{string.Join("\n\t", args)}");


				Server server = new() { Port = port.GetValue(), Log = log };
				server.OnNewClient += (_, c) => {

					c.OnClosed += (_, _) => { log.Write("Client closed"); };

					c.OnDataMessageReceived += (_, d) => { log.Write($"Data message of {d.Length} bytes received"); };

					c.OnTextMessageReceived += (c, t) =>
					{
						log.Write($"Text message received: {t}");
						((Server.Client?)c)!.SendText($"Got: \"{t}\" with a smile");
					};

				};

				server.Running = true;

				Thread.Sleep(TimeSpan.FromSeconds(30));

				server.Running = false;
				server.CloseAllClient();

				// CustomUrlProtocol.RegisterAsHandler(Path.Combine(AppContext.BaseDirectory, "LocalHtmlInterop.exe"));

				log.Write("done.");
			}
			catch (Exception ex)
			{
				log?.Write(ISimpleLog.FlagError, $"EXCEPTION: {ex}");
			}
		}

	}
}
