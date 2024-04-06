using Microsoft.Win32;
using SGrottel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using YamlDotNet.Core.Tokens;

namespace LocalHtmlInterop.Handler
{

	internal class HandlerAppProgram
	{

		private static ISimpleLog? log = null;
		private static CommandManager cmdMan = new();
		private static readonly TimeSpan noClientCloseTimeout = TimeSpan.FromSeconds(10);

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

					case CmdLineArgs.Operation.AddFileToRegister:
						RunOperationAndReportViaPipe(
							cmdLine.AppOperation.ToString(),
							cmdLine.CallbackId,
							(output) =>
							{
								if (cmdLine.CommandParameters == null || cmdLine.CommandParameters.Count <= 0)
								{
									output.WriteLine("No files specified");
									return;
								}

								if (OperatingSystem.IsWindows()) {
									AddFilesToRegistry(
										output,
										(cmdLine.Command == "lm")
											? Registry.LocalMachine
											: Registry.CurrentUser,
										cmdLine.CommandParameters.Keys);
								}
							});
						return;

					case CmdLineArgs.Operation.RemoveFileFromRegister:
						RunOperationAndReportViaPipe(
							cmdLine.AppOperation.ToString(),
							cmdLine.CallbackId,
							(output) =>
							{
								if (cmdLine.CommandParameters == null || cmdLine.CommandParameters.Count <= 0)
								{
									output.WriteLine("No files specified");
									return;
								}

								if (OperatingSystem.IsWindows())
								{
									RemoveFilesFromRegistry(
										output,
										cmdLine.CommandParameters.Keys);
								}
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
				cmdMan.Log = log;

				log.Write($"Called:\n\t{string.Join("\n\t", args)}");

				SingleInstance singleInstance = new();
				if (singleInstance.Failed)
				{
					// Another handler instance exist.
					// Try handing off the task.
					if (HandoffTask(cmdLine))
					{
						log.Write("Handed off task to single instance");
						return;
					}

					// Handoff failed.
					// The main instance might have shut down by now.
					Thread.Sleep(TimeSpan.FromSeconds(1));
					singleInstance = new();
					if (singleInstance.Failed)
					{
						// still not good. giving up
						log.Write(ISimpleLog.FlagError, "Failed to hand off task to single instance");
						throw new Exception("Critical Error");
					}
					// else, we now are the main instance. continue.
				}

				cmdMan.Push(new()
				{
					Command = cmdLine.Command,
					CallbackId = cmdLine.CallbackId,
					CommandParameters = cmdLine.CommandParameters
				});

				// Opening server, regardless of whether or not the request contains a callback id.
				// Additional requests from secondary handler instance will come in via the same server socket.
				ServerPortConfig port = new();
				Server server = new() { Port = port.GetValue(), Log = log };

				server.OnNewClient += Server_OnNewClient;
				server.OnClientClosed += Server_OnClientClosed;

				server.Running = true;

				DateTime disconnectTime = DateTime.Now + noClientCloseTimeout;
				while (DateTime.Now < disconnectTime)
				{
					Thread.Sleep(TimeSpan.FromSeconds(1));

					bool active = false;

					lock (clientCountLock)
					{
						if (clientCount > 0)
						{
							active = true;
						}
					}

					if (cmdMan.CoundRunningCommands() > 0)
					{
						active = true;
					}

					if (active)
					{
						disconnectTime = DateTime.Now + noClientCloseTimeout;
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

				if (client!.GetType() == typeof(AppTcpClient))
				{
					client.OnDataMessageReceived += (c, d) =>
					{
						CommandInfo? data = JsonSerializer.Deserialize<CommandInfo>(Encoding.UTF8.GetString(d));
						if (data != null)
						{
							cmdMan.Push(data);
						}
					};
				}
				else
				{
#if DEBUG
					client.OnDataMessageReceived += (c, d) =>
					{
						log?.Write($"Data message of {d.Length} bytes received");
					};
#endif

					client.OnTextMessageReceived += (c, t) =>
					{
						if (t.StartsWith("reqCallback:", StringComparison.CurrentCultureIgnoreCase))
						{
							string id = t[12..].Trim();
							string resp = cmdMan.GetCallbackResponse(id);
							((Server.Client?)c)!.SendText(resp);
							return;
						}

#if DEBUG
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
#endif
					};
				}
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

		private static bool HandoffTask(CmdLineArgs cmdLine)
		{
			try
			{
				ServerPortConfig port = new();
				using (TcpClient tcp = new TcpClient("localhost", port.GetValue()))
				{
					var stream = tcp.GetStream();
					stream.Write(Encoding.UTF8.GetBytes("SGR"));

					CommandInfo data = new()
					{
						Command = cmdLine.Command,
						CommandParameters = cmdLine.CommandParameters,
						CallbackId = cmdLine.CallbackId
					};

					byte[] rawData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
					byte[] rawDataLen = BitConverter.GetBytes((uint)rawData.Length);
					Debug.Assert(rawDataLen.Length == 4);

					stream.Write(rawDataLen);
					stream.Write(rawData);

					int conf = stream.ReadByte();
					if (conf == 1)
					{
						// confirmation byte signaled success
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				log?.Write(ISimpleLog.FlagError, $"EXCEPTION trying to connect for task handoff: {ex}");
			}
			return false;
		}

		private static void AddFilesToRegistry(StreamWriter output, RegistryKey registryKey, IReadOnlyCollection<string> files)
		{
			bool someReg = false;
			using (var regkey = registryKey.CreateSubKey(RegistryUtility.SubKeyName))
			{
				foreach (string file in files)
				{
					output.WriteLine($"Reg: {file}");

					IEnumerable<string> knownFiles
						= regkey
						.GetValueNames()
						.Where((s) => s.StartsWith("file-"))
						.Select((s) => regkey.GetValue(s)?.ToString() ?? string.Empty);

					bool known = false;
					foreach (string knownFile in knownFiles)
					{
						if (knownFile.Equals(file, StringComparison.InvariantCultureIgnoreCase))
						{
							known = true;
							break;
						}
					}
					if (known)
					{
						output.WriteLine("  file already registered. Skipping.");
						continue;
					}

					string sn = $"file-1";
					for (int n = 1; knownFiles.Contains(sn); n++, sn = $"file-{n}") ;

					regkey.SetValue(sn, file);
					output.WriteLine("  registered.");
					someReg = true;
				}
			}

			if (someReg)
			{
				output.Write("The newly registered command definition files will only be loaded when the handler process restarts. ");
				output.WriteLine($"Wait {noClientCloseTimeout} seconds, for any current process to exit after the last connection is closed.");
			}
			else
			{
				output.WriteLine("ok.");
			}
		}

		private static void RemoveFilesFromRegistry(StreamWriter output, IReadOnlyCollection<string> files)
		{
			RegistryKey?[] regkey = new RegistryKey?[2];
			{
				Exception? ex = null;
				try
				{
					regkey[0] = Registry.LocalMachine.OpenSubKey(RegistryUtility.SubKeyName, true);
				}
				catch (Exception ex1) { ex = ex1; }
				try
				{
					regkey[1] = Registry.CurrentUser.OpenSubKey(RegistryUtility.SubKeyName, true);
				}
				catch (Exception ex2) { ex = ex2; }
				if (regkey[0] == null && regkey[1] == null)
				{
					throw new InvalidOperationException($"Failed to open any application registry keys: {ex}");
				}
			}

			try
			{
				foreach (string file in files)
				{
					output.WriteLine($"{file}");
					foreach (RegistryKey? rk in regkey)
					{
						if (rk == null) continue;
						IEnumerable<string> knownFilesNames
							= rk
								.GetValueNames()
								.Where((s) => s.StartsWith("file-"));
						foreach (var vn in knownFilesNames)
						{
							string? kf = rk.GetValue(vn) as string;
							if (kf?.Equals(file, StringComparison.InvariantCultureIgnoreCase) ?? false)
							{
								output.WriteLine($"Found {rk.Name}..{vn}");

								try
								{
									rk.DeleteValue(vn);
									if (rk.GetValueNames().Contains(vn))
									{
										output.WriteLine("\tFailed to delete: entry silently remains");
									}
									else
									{
										output.WriteLine("\tdeleted");
									}
								}
								catch(Exception ex)
								{
									output.WriteLine($"\tFailed to delete: {ex}");
								}
							}
						}
					}
				}

			}
			finally
			{
				for (int i = 0; i < 2; i++)
				{
					try
					{
						regkey[i]?.Close();
						regkey[i]?.Dispose();
						regkey[i] = null;
					}
					catch { }
				}
			}
		}

	}
}
