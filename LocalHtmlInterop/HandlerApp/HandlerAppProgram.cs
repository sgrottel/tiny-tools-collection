using SGrottel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;

namespace LocalHtmlInterop.Handler
{

	internal class HandlerAppProgram
	{

		private static ISimpleLog? log = null;

		internal static void Main(string[] args)
		{
			try
			{
				CmdLineArgs cmdLine = new();
				if (!cmdLine.Parse(args))
				{
					throw new Exception($"\n\nThe Handler application \"LocalHtmlInterop.Handler.exe\" should not be called directly.\n"
						+ "Call the command line utility \"LocalHtmlInterop.exe\" instead.\n");
				}

				switch (cmdLine.AppOperation)
				{
					case CmdLineArgs.Operation.None:
						throw new Exception($"\n\nHandler application invoke parameters invalid:\n\"{string.Join("\" \"", args)}\"\n");

					case CmdLineArgs.Operation.InteropCall:
						break;

					case CmdLineArgs.Operation.RegisterHandler:
						RunOperationAndReportViaPipe(
							cmdLine.AppOperation.ToString(),
							cmdLine.CallbackId,
							(output) =>
							{
								output.WriteLine("Writing entry to windows registry.");
								CustomUrlProtocol.RegisterAsHandler(Path.Combine(AppContext.BaseDirectory, "LocalHtmlInterop.Handler.exe"));
								// todo: read back registry to check
								output.WriteLine("Complete");
							});
						return;

					case CmdLineArgs.Operation.UnregisterHandler:
						RunOperationAndReportViaPipe(
							cmdLine.AppOperation.ToString(),
							cmdLine.CallbackId,
							(output) =>
							{
								// todo: implement
								throw new NotImplementedException();
							});
						return;

					default:
						throw new Exception($"\n\nHandler application invoke parameters invalid:\n\"{string.Join("\" \"", args)}\"\n");
				}

				// main operation
				Debug.Assert(cmdLine.AppOperation == CmdLineArgs.Operation.InteropCall);
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



				log.Write("done.");
			}
			catch (Exception ex)
			{
				if (log != null)
				{
					log.Write(ISimpleLog.FlagError, $"EXCEPTION: {ex}");
				}
				else
				{
					MessageBox(IntPtr.Zero, ex.ToString(), AppDomain.CurrentDomain.FriendlyName, MB_OK | MB_ICONERROR);
				}
			}
		}

		private const uint MB_OK = 0x00000000;
		private const uint MB_ICONERROR = 0x00000010;
		private const uint MB_ICONQUESTION = 0x00000020;
		private const uint MB_ICONWARNING = 0x00000030;
		private const uint MB_ICONINFORMATION = 0x00000040;
		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

		private static int clientCount = 0;
		private static object clientCountLock = new();

		private static void Server_OnNewClient(object? server, Server.Client client)
		{
			log?.Write($"OnNewClient({client.Port})");
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
			log?.Write($"OnClientClosed({e.Port})");
			lock (clientCountLock)
			{
				if (clientCount > 0) clientCount--;
			}
		}

		private static void RunOperationAndReportViaPipe(string operationName, string? pipeName, Action<StreamWriter> operation)
		{
			try
			{
				if (string.IsNullOrEmpty(pipeName)) throw new ArgumentNullException(paramName: nameof(pipeName));

				NamedPipeClientStream pipeClient = new(".", pipeName, PipeDirection.Out);
				pipeClient.Connect();
				using (StreamWriter writer = new(pipeClient, Encoding.UTF8))
				{
					writer.AutoFlush = true;
					try
					{
						operation(writer);
					}
					catch (Exception iex)
					{
						writer.WriteLine($"ERROR: failed to run {operationName}\nEXCEPTION: {iex}");
					}
				}
			}
			catch (Exception ex)
			{
				log = new SimpleLog();
				log.Write($"Called {operationName} with pipe {pipeName}");
				log.Write(ISimpleLog.FlagError, $"EXCEPTION: {ex}");
			}
		}

	}
}
