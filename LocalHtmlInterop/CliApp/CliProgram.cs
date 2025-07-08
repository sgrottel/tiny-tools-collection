using SGrottel;
using System.CommandLine;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;

namespace LocalHtmlInterop
{
	internal class CliProgram
	{
		static void PrintGreeting()
		{
			Console.WriteLine("SGR Local Html Interop");
		}

		private static string GetVersion(Assembly? asm, string? asmFile)
		{
			if (string.IsNullOrEmpty(asmFile))
			{
				if (asm != null)
				{
					var fvae = asm.GetCustomAttributes<AssemblyFileVersionAttribute>();
					if (fvae.Any())
					{
						return fvae.First().Version;
					}
				}

				return string.Empty;
			}

			try
			{
				return AssemblyName.GetAssemblyName(asmFile).Version?.ToString() ?? string.Empty;
			}
			catch { }

			try
			{
				FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asmFile);
				return fvi.FileVersion ?? string.Empty;
			}
			catch { }

			return string.Empty;
		}

		static void PrintVersions()
		{
			Console.WriteLine("  CLI");
			Console.WriteLine($"  Ver.: {GetVersion(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().Location)}");
			if (!string.IsNullOrEmpty(HandlerPath) && File.Exists(HandlerPath))
			{
				Console.WriteLine($"  Handler: {HandlerPath}\n  Ver.: {GetVersion(null, HandlerPath)}");
			}
			else
			{
				Console.Error.WriteLine("  ERROR: Handler app not found! Most operations will fail!");
			}
		}

		static string HandlerPath { get; } = FindHandlerAssembly();

		private static string FindHandlerAssembly()
		{
			// not trusting search path env variables
			// need to find the full path manually
			string handlerFilename = "LocalHtmlInterop.Handler.exe";

			string p = Path.Combine(AppContext.BaseDirectory, handlerFilename);
			if (File.Exists(p)) return p;

			p = Path.Combine(Environment.CurrentDirectory, handlerFilename);
			if (File.Exists(p)) return p;

			// assume dev paths:
			p = AppContext.BaseDirectory;
			string dn = Path.GetFileName(p);
			string? tp = Path.GetDirectoryName(p);
			if (tp == null) return string.Empty;
			if (string.IsNullOrEmpty(dn))
			{
				dn = Path.GetFileName(tp);
				tp = Path.GetDirectoryName(tp);
				if (tp == null) return string.Empty;
			}
			if (!dn.EndsWith("-windows"))
			{
				dn += "-windows";
			}

			string c = Path.GetFileName(tp);
			tp = Path.GetDirectoryName(tp);
			if (tp == null) return string.Empty;

			if (!Path.GetFileName(tp).Equals("bin", StringComparison.InvariantCultureIgnoreCase)) return string.Empty;
			tp = Path.GetDirectoryName(tp);
			if (tp == null) return string.Empty;

			if (!Path.GetFileName(tp).Equals("CliApp", StringComparison.InvariantCultureIgnoreCase)) return string.Empty;
			tp = Path.GetDirectoryName(tp);
			if (tp == null) return string.Empty;

			p = Path.Combine(tp, $"HandlerApp\\bin\\{c}\\{dn}\\{handlerFilename}");
			if (File.Exists(p)) return p;

			return string.Empty;
		}

		private static ISimpleLog log =
#if DEBUG
			new DebugOutputEchoingSimpleLog
#endif
				(new EchoingSimpleLog(new SimpleLog()));

		private static readonly Option<bool> nologoOption
			= new Option<bool>(name: "--nologo")
			{
				Description = "Hide the application's startup logo",
				Recursive = true
			};

		private static readonly Option<bool> forceOption
			= new Option<bool>(name: "--force")
			{
				Description = "Forces an operation",
				Aliases = { "-f" },
				Recursive = true
			};

		private static bool printBye = true;

		private static void PrintLogoIfNotSuppressed(ParseResult parseResult)
		{
			if (!parseResult.GetValue(nologoOption))
			{
				PrintGreeting();
				PrintVersions();
			}
		}

		static int Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			int rv = 0;

			try
			{
				var registerCommand = new Command(
					name: "register",
					description: "Registers the handler application matching this CLI application for the custom protocol")
				{
					Aliases = { "reg" }
				};
				registerCommand.SetAction(RunRegisterCommand);

				var unregisterCommand = new Command(
					name: "unregister",
					description: "Unregisters the handler application matching this CLI application for the custom protocol")
				{
					Aliases = { "unreg" }
				};
				unregisterCommand.SetAction(RunUnregisterCommand);

				var getPortCommand = new Command(
					name: "getport",
					description: "Prints the WS port used by the handler application")
				{
					Aliases = { "port" }
				};
				var getDefaultPortOption = new Option<bool>(name: "--default")
				{
					Description = "Returns the default port configured by the application, instead of the currently configured port"
				};
				getPortCommand.Add(getDefaultPortOption);
				getPortCommand.SetAction((c) =>
				{
					RunGetPortCommand(c, getDefaultPortOption);
				});

				var setPortCommand = new Command(
					name: "setport",
					description: "Sets the WS port number to be used by the handler application");
				var newPortValueArgument = new Argument<ushort>(name: "port")
				{
					Description = "The new port value to be used"
				};
				setPortCommand.Add(newPortValueArgument);
				setPortCommand.SetAction((c) =>
				{
					RunSetPortCommand(c, newPortValueArgument);
				});

				var getJSCodeCommand = new Command(
					name: "getjscode",
					description: "Prints the JavaScript code of the 'CallbackReceiver' class (cf. demo.html)");
				var getJSMinifiedCodeOption = new Option<bool>(name: "--mini")
				{
					Description = "Returns the minified version of the JavaScript code"
				};
				getJSCodeCommand.Add(getJSMinifiedCodeOption);
				getJSCodeCommand.SetAction((c) =>
				{
					RunGetJavaScriptCode(c, getJSMinifiedCodeOption);
				});

				var commandsFileArgument = new Argument<IEnumerable<string>>("file")
				{
					Description = "The commands file to be processed",
					Arity = ArgumentArity.OneOrMore
				};

				var commandsFileExistingArgument = new Argument<IEnumerable<FileInfo>>("file")
				{
					Description = "The commands file to be processed",
					Arity = ArgumentArity.OneOrMore
				}.AcceptExistingOnly();

				var listCommandsCommand = new Command(
					name: "list",
					description: "Lists all registered commands")
				{
					Aliases = { "ls" }
				};
				listCommandsCommand.SetAction(RunListCommandsCommand);

				var validateCommandsFileCommand = new Command(
					name: "validate",
					description: "Validates a commands file")
				{
					commandsFileExistingArgument
				};
				validateCommandsFileCommand.SetAction((c) =>
				{
					RunValidateCommandsFileCommand(c, commandsFileExistingArgument);
				});

				var addCommandsFileToLocalMaschineOption = new Option<bool>(name: "--localmachine")
				{
					Description = "Adds the command file ot the 'local machine' registry, instead of the 'current user' registry (default)",
					Aliases = { "-m" }
				};
				var addCommandsFileCommand = new Command(
					name: "add",
					description: "Adds a commands file to the registered commands")
				{
					addCommandsFileToLocalMaschineOption,
					commandsFileExistingArgument
				};
				addCommandsFileCommand.SetAction((c) =>
				{
					RunAddCommandsFileCommand(c, commandsFileExistingArgument, addCommandsFileToLocalMaschineOption);
				});

				var removeCommandsFileCommand = new Command(
					name: "remove",
					description: "Removes a commands file from the registered commands")
				{
					commandsFileArgument
				};
				removeCommandsFileCommand.Aliases.Add("rm");
				removeCommandsFileCommand.SetAction((c) =>
				{
					RunRemoveCommandsFileCommand(c, commandsFileArgument);
				});

				var commandsCommand = new Command(
					name: "command",
					description: "Query and manage commands registered to the application")
				{
					listCommandsCommand,
					validateCommandsFileCommand,
					addCommandsFileCommand,
					removeCommandsFileCommand
				};
				commandsCommand.Aliases.Add("cmd");

				var rootCommand = new RootCommand(description: "SGR Local Html Interop CLI Application")
				{
					registerCommand,
					unregisterCommand,
					getPortCommand,
					setPortCommand,
					getJSCodeCommand,
					commandsCommand,
					nologoOption,
					forceOption
				};

				var parseResult = rootCommand.Parse(args);
				parseResult.Configuration.EnableDefaultExceptionHandler = false;
				rv = parseResult.Invoke();

				if (printBye)
				{
					log.Write("done.");
				}
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
					log.Error($"EXCEPTION: {ex}");
				}
			}
			return rv;
		}

		[SupportedOSPlatform("windows")]
		static void CallHandler(string argument, bool elevated = true, Action<System.Collections.ObjectModel.Collection<string>>? addArgs = null)
		{
			string pipename = "SGR-LocalHtmlInterop-" + Guid.NewGuid().ToString();
			log.Write(EchoingSimpleLog.FlagDontEcho, $"Opening IPC named pipe {pipename}");

			using (NamedPipeServerStream pipeServer = new(pipename, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
			using (Process p = new())
			{
				log.Write($"Calling handler app with \"{argument}\"");

				p.StartInfo.FileName = HandlerPath;
				p.StartInfo.ArgumentList.Clear();
				p.StartInfo.ArgumentList.Add(argument);
				p.StartInfo.ArgumentList.Add(pipename);

				addArgs?.Invoke(p.StartInfo.ArgumentList);

				if (elevated)
				{
					if (!ElevationRightsUtils.IsAdministrator())
					{
						p.StartInfo.UseShellExecute = true;
						p.StartInfo.Verb = "runas";
					}
				}

				p.Start();

				CancellationTokenSource cancelConnect = new CancellationTokenSource(); 
				var waitConnectionTask = pipeServer.WaitForConnectionAsync(cancelConnect.Token);

				while (!waitConnectionTask.IsCompleted && !p.HasExited)
				{
					Thread.Sleep(10);
				}

				if (!waitConnectionTask.IsCompleted)
				{
					cancelConnect.Cancel();
					log.Error("Did not connect to handler process before it exited.");
					return;
				}

				using (StreamReader reader = new(pipeServer, Encoding.UTF8))
				{
					while (true)
					{
						string? line = reader.ReadLine();
						if (line == null) break;
						log.Write(line);
					}
				}

				log.Detail(EchoingSimpleLog.FlagDontEcho, "Pipe stream drained. Waiting for process exit.");
				p.WaitForExit();
			}
		}

		private static void RunRegisterCommand(ParseResult parseResult)
		{
			PrintLogoIfNotSuppressed(parseResult);
			bool force = parseResult.GetValue(forceOption);

			string? regHanExe = CustomUrlProtocol.GetRegisteredHandlerExe();
			if (regHanExe != null)
			{
				if (!HandlerPath.Equals(regHanExe, StringComparison.CurrentCultureIgnoreCase))
				{
					log.Write(
						force ? ISimpleLog.FlagLevelWarning : ISimpleLog.FlagLevelError,
						$"Registering your handler application will overwrite an existing registration:\n\told: {regHanExe}\n\tnew: {HandlerPath}");
					if (!force)
					{
						log.Write("You must add '--force' option to continue");
						return;
					}
				}
			}

			if (OperatingSystem.IsWindows())
			{
				CallHandler("-register");
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private static void RunUnregisterCommand(ParseResult parseResult)
		{
			PrintLogoIfNotSuppressed(parseResult);
			bool force = parseResult.GetValue(forceOption);

			string? regHanExe = CustomUrlProtocol.GetRegisteredHandlerExe();
			if (regHanExe != null)
			{
				if (!HandlerPath.Equals(regHanExe, StringComparison.CurrentCultureIgnoreCase))
				{
					log.Write(
						force ? ISimpleLog.FlagLevelWarning : ISimpleLog.FlagLevelError,
						$"To be unregistered handler application does not match your handler application:\n\tregistered: {regHanExe}\n\tyour: {HandlerPath}");
					if (!force)
					{
						log.Write("You must add '--force' option to continue");
						return;
					}
				}
			}

			if (OperatingSystem.IsWindows())
			{
				CallHandler("-unregister");
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private static void RunGetPortCommand(ParseResult parseResult, Option<bool> getDefaultPortOption)
		{
			PrintLogoIfNotSuppressed(parseResult);
			ServerPortConfig portCfg = new();
			Console.WriteLine(
				parseResult.GetValue(getDefaultPortOption)
				? ServerPortConfig.DefaultPort
				: portCfg.GetValue());
			printBye = false;
		}

		private static void RunSetPortCommand(ParseResult parseResult, Argument<ushort> newPortValueArgument)
		{
			PrintLogoIfNotSuppressed(parseResult);
			bool force = parseResult.GetValue(forceOption);

			ushort newPortValue = parseResult.GetRequiredValue(newPortValueArgument);
			if (newPortValue < 1024)
			{
				log.Write(
					force ? ISimpleLog.FlagLevelWarning : ISimpleLog.FlagLevelError,
					$"Ports {newPortValue} < 1024 are reserved for system services");
				if (!force)
				{
					log.Write("You must add '--force' option to continue");
					return;
				}
			}
			if (newPortValue > 49151)
			{
				log.Write(
					force ? ISimpleLog.FlagLevelWarning : ISimpleLog.FlagLevelError,
					$"Ports {newPortValue} > 49151 are used for dynamic connections");
				if (!force)
				{
					log.Write("You must add '--force' option to continue");
					return;
				}
			}

			if (OperatingSystem.IsWindows())
			{
				ServerPortConfig portCfg = new();
				portCfg.SetValue(newPortValue);
			}
			else
			{
				throw new InvalidOperationException();
			}

			{
				ServerPortConfig portCfg = new();
				if (portCfg.GetValue() != newPortValue)
				{
					log.Error($"FAILED to set port to {newPortValue}. Value remains: {portCfg.GetValue()}");
				}
			}
		}

		private static void RunListCommandsCommand(ParseResult parseResult)
		{
			PrintLogoIfNotSuppressed(parseResult);

			var files = RegistryUtility.LoadFilePaths();
			log.Write($"Registry contains {files.Count} files:");
			log.Write("");
			foreach (var file in files)
			{
				log.Write(file);
			}
			log.Write("");
		}

		private static void RunValidateCommandsFileCommand(ParseResult parseResult, Argument<IEnumerable<FileInfo>> fileArgument)
		{
			PrintLogoIfNotSuppressed(parseResult);

			var files = parseResult.GetValue(fileArgument);
			if (files == null)
			{
				log.Error($"No files specified");
				return;
			}

			foreach (var file in files)
			{
				log.Write($"Validating: {file.FullName}");
				try
				{
					CommandDefinitionFile cdf = CommandDefinitionFile.Load(file.FullName);
					if ((cdf.commands?.Length ?? 0) <= 0)
					{
						log.Warning("No commands defined in file");
					}

					foreach (var cmd in cdf.commands!)
					{
						if (cmd.ValidationError == null) continue;
						throw new Exception($"Failed to validate command {cmd.name}: {cmd.ValidationError}");
					}

					log.Write("ok.");
				}
				catch (Exception ex)
				{
					log.Error($"FAILED: {ex}");
					if (ex.InnerException != null)
					{
						log.Error($"\t{ex.InnerException}");
					}
				}
			}
		}

		private static void RunAddCommandsFileCommand(ParseResult parseResult, Argument<IEnumerable<FileInfo>> fileArgument, Option<bool> addToLocalMachineOption)
		{
			PrintLogoIfNotSuppressed(parseResult);
			bool addToLocalMachine = parseResult.GetValue(addToLocalMachineOption);

			var files = parseResult.GetValue(fileArgument);
			if (files == null)
			{
				log.Error($"No files specified");
				return;
			}

			HashSet<FileInfo> cdfs = new();
			foreach (var fi in files)
			{
				try
				{
					var cdf = CommandDefinitionFile.Load(fi.FullName);
					if ((cdf.commands?.Length ?? 0) <= 0)
					{
						throw new ArgumentException("No commands defined in file");
					}

					foreach (var cmd in cdf.commands!)
					{
						if (cmd.ValidationError == null) continue;
						throw new Exception($"Invalidate command {cmd.name}: {cmd.ValidationError}");
					}

					cdfs.Add(fi);
				}
				catch(Exception ex)
				{
					log.Error($"{fi.FullName}\nFAILED: {ex}");
				}
			}

			if (OperatingSystem.IsWindows())
			{
				CallHandler(
					"-addfile",
					elevated: addToLocalMachine,
					addArgs: (c) =>
					{
						if (addToLocalMachine) c.Add("lm");
						foreach (var fi in cdfs)
						{
							c.Add(fi.FullName);
						}
					});
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private static void RunRemoveCommandsFileCommand(ParseResult parseResult, Argument<IEnumerable<string>> fileArgument)
		{
			PrintLogoIfNotSuppressed(parseResult);

			var files = parseResult.GetValue(fileArgument);
			if (files == null)
			{
				log.Error($"No files specified");
				return;
			}

			HashSet<string> regFilesLM = new(RegistryUtility.LoadFilePathsFromLocalMachine().Select((s) => s.ToLowerInvariant()));
			// if one of the `files` entries is in here, admin rights will be required

			HashSet<string> regFiles = new(RegistryUtility.LoadFilePaths().Select((s) => s.ToLowerInvariant()));
			// if one of the `files` entries is not in here, it's not registered at all

			HashSet<string> toUnreg = new();
			bool asAdmin = false;

			foreach (var file in files)
			{
				if (!regFiles.Contains(file.ToLowerInvariant()))
				{
					log.Warning($"Skipping file {file} -- is not registered");
				}

				toUnreg.Add(file);

				asAdmin |= regFilesLM.Contains(file.ToLowerInvariant());
			}

			if (OperatingSystem.IsWindows())
			{
				CallHandler(
					"-rmfile",
					elevated: asAdmin,
					addArgs: (c) =>
					{
						foreach (var fi in toUnreg)
						{
							c.Add(fi);
						}
					});
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private static void RunGetJavaScriptCode(ParseResult parseResult, Option<bool> getJSMinifiedCodeOption)
		{
			PrintLogoIfNotSuppressed(parseResult);

			bool getMini = parseResult.GetValue(getJSMinifiedCodeOption);

			string str = ResourceLoader.LoadString(
				getMini
				? ResourceLoader.Id.CallbackReceiver_Mini_JavaScript
				: ResourceLoader.Id.CallbackReceiver_JavaScript);

			Console.WriteLine(str);

			printBye = false;
		}

	}
}
