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
						// continue with implementation below this switch...
						break;

					case CmdLineArgs.Operation.RegisterHandler:
						RunOperationAndReportViaPipe(
							cmdLine.AppOperation.ToString(),
							cmdLine.CallbackId,
							(output) =>
							{
								output.WriteLine("Writing entry to windows registry.");

								string path = Path.Combine(AppContext.BaseDirectory, "LocalHtmlInterop.Handler.exe");

								string? rbPath = CustomUrlProtocol.GetRegisteredHandlerExe();
								if (rbPath != null)
								{
									if (!path.Equals(rbPath, StringComparison.CurrentCultureIgnoreCase))
									{
										output.WriteLine("WARNING: another handler is already registered. Registration will be overwritten:");
										output.WriteLine($"\told value: {rbPath}");
									}
								}

								CustomUrlProtocol.RegisterAsHandler(path);
								rbPath = CustomUrlProtocol.GetRegisteredHandlerExe();

								if (rbPath == null)
								{
									output.WriteLine("FAILED: handler was not written to windows registry.");
									return;
								}
								if (!path.Equals(rbPath, StringComparison.CurrentCultureIgnoreCase))
								{
									output.WriteLine("FAILED: registered handler is different from stored and read-back handler:");
									output.WriteLine($"\tselected: {path}");
									output.WriteLine($"\tstored: {rbPath}");
									return;
								}

								output.WriteLine("Complete");
							});
						return;

					case CmdLineArgs.Operation.UnregisterHandler:
						RunOperationAndReportViaPipe(
							cmdLine.AppOperation.ToString(),
							cmdLine.CallbackId,
							(output) =>
							{
								output.WriteLine("Removing entry from windows registry.");

								string path = Path.Combine(AppContext.BaseDirectory, "LocalHtmlInterop.Handler.exe");

								string? rbPath = CustomUrlProtocol.GetRegisteredHandlerExe();
								if (rbPath != null)
								{
									if (!path.Equals(rbPath, StringComparison.CurrentCultureIgnoreCase))
									{
										output.WriteLine("WARNING: registered handler mismatches this handler path:");
										output.WriteLine($"\told value: {rbPath}");
									}
								}

								CustomUrlProtocol.UnregisterHandler();
								rbPath = CustomUrlProtocol.GetRegisteredHandlerExe();

								if (rbPath != null)
								{
									output.WriteLine("FAILED: handler is still registered");
									output.WriteLine($"\tvalue: {rbPath}");
									return;
								}

								output.WriteLine("Complete");
							});
						return;

					default:
						throw new Exception($"\n\nHandler application invoke parameters invalid:\n\"{string.Join("\" \"", args)}\"\n");
				}

				// main operation
				Debug.Assert(cmdLine.AppOperation == CmdLineArgs.Operation.InteropCall);
#if DEBUG
				log = new DebugEchoLog<SimpleLog>();
#else
				log = new SimpleLog();
#endif

				SingleInstance singleInstance = new();
				if (singleInstance.Failed)
				{
					// Another handler instance exist.
					// Try handing off the task.

					// TODO: Implement
					throw new NotImplementedException();

				}

				log.Write($"Called:\n\t{string.Join("\n\t", args)}");

				// TODO: Hand off to job manager

				// Opening server, regardless of whether or not the request contains a callback id.
				// Additional requests from secondary handler instance will come in via the same server socket.
				ServerPortConfig port = new();
				Server server = new() { Port = port.GetValue(), Log = log };

				server.OnNewClient += Server_OnNewClient;
				server.OnClientClosed += Server_OnClientClosed;

				server.Running = true;

				TimeSpan noClientCloseTimeout = TimeSpan.FromSeconds(10);
				DateTime disconnectTime = DateTime.Now + noClientCloseTimeout;
				while (DateTime.Now < disconnectTime)
				{
					Thread.Sleep(TimeSpan.FromSeconds(1));
					lock (clientCountLock)
					{
						if (clientCount > 0)
						{
							disconnectTime = DateTime.Now + noClientCloseTimeout;
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
					MessageBox.Show(IntPtr.Zero, ex.ToString(), AppDomain.CurrentDomain.FriendlyName, MessageBox.MB_OK | MessageBox.MB_ICONERROR);
				}
			}
		}

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
