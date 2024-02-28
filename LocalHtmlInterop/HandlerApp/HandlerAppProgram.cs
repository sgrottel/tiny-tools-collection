using SGrottel;

namespace LocalHtmlInterop.Handler
{

	internal class HandlerAppProgram
	{

		private static ISimpleLog? log = null;

		internal static void Main(string[] args)
		{
			try
			{
				log = new SimpleLog();

				ServerPortConfig port = new();
				port.SetValue(ServerPortConfig.DefaultPort);


				log.Write($"Called:\n\t{string.Join("\n\t", args)}");


				Server server = new() { Port = port.GetValue(), Log = log };
				server.OnNewClient += Server_OnNewClient;
				server.OnClientClosed += Server_OnClientClosed;
				server.Running = true;
				TimeSpan noClientCloseTimeout = TimeSpan.FromSeconds(10);
				DateTime noClientSince = DateTime.Now;
				while (DateTime.Now - noClientSince <= noClientCloseTimeout)
				{
					Thread.Sleep(TimeSpan.FromSeconds(1));
					lock (clientCountLock)
					{
						if (clientCount > 0)
						{
							noClientSince = DateTime.Now + TimeSpan.FromSeconds(1);
						}
					}
				}
				server.Running = false;
				server.CloseAllClients();

				// CustomUrlProtocol.RegisterAsHandler(Path.Combine(AppContext.BaseDirectory, "LocalHtmlInterop.exe"));

				log.Write("done.");
			}
			catch (Exception ex)
			{
				log?.Write(ISimpleLog.FlagError, $"EXCEPTION: {ex}");
			}
		}

		private static int clientCount = 0;
		private static object clientCountLock = new();

		private static void Server_OnNewClient(object? server, Server.Client client)
		{
			lock (clientCountLock)
			{
				clientCount++;

				client.OnDataMessageReceived += (_, d) =>
				{
					log?.Write($"Data message of {d.Length} bytes received");
				};

				client.OnTextMessageReceived += (c, t) =>
				{
					log?.Write($"Text message received: {t}");
					if (t.Trim().ToLower() == "exit")
					{
						((Server?)server)!.CloseAllClients();
					}
					else
					if (t.Trim().ToLower() == "close")
					{
						((Server.Client?)c)!.Close();
					}
					else
					{
						((Server.Client?)c)!.SendText($"Got: \"{t}\" with a smile");
					}
				};
			}
		}

		private static void Server_OnClientClosed(object? sender, Server.Client e)
		{
			lock (clientCountLock)
			{
				if (clientCount > 0) clientCount--;
			}
		}

	}
}
