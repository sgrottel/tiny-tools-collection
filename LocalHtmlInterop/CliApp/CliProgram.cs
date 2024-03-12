using SGrottel;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;
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

		private static string GetVersion(string asmFile)
		{
			try
			{
				return AssemblyName.GetAssemblyName(asmFile).Version?.ToString() ?? string.Empty;
			}
			catch { }

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asmFile);
			return fvi.FileVersion ?? string.Empty;
		}

		static void PrintVersions()
		{
			Console.WriteLine("  CLI");
			Console.WriteLine($"  Ver.: {GetVersion(Assembly.GetExecutingAssembly().Location)}");
			if (!string.IsNullOrEmpty(HandlerPath) && File.Exists(HandlerPath))
			{
				Console.WriteLine($"  Handler: {HandlerPath}\n  Ver.: {GetVersion(HandlerPath)}");
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

		private static ISimpleLog log
#if DEBUG
			= new DebugEchoLog<EchoingSimpleLog>();
#else
			= new EchoingSimpleLog();
#endif

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static Option<bool> nologoOption;
		private static Option<bool> forceOption;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		private static bool printBye = true;

		static int Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			int rv = 0;

			try
			{

				nologoOption = new Option<bool>("--nologo", "Hide the application's startup logo");

				forceOption = new Option<bool>("--force", "Forces an operation");
				forceOption.AddAlias("-f");

				var registerCommand = new Command("register", "Registers the handler application matching this CLI application for the custom protocol");
				registerCommand.AddAlias("reg");
				registerCommand.SetHandler(RunRegisterCommand);

				var unregisterCommand = new Command("unregister", "Unregisters the handler application matching this CLI application for the custom protocol");
				unregisterCommand.AddAlias("unreg");
				unregisterCommand.SetHandler(RunUnregisterCommand);

				var getPortCommand = new Command("getport", "Prints the WS port used by the handler application");
				var getDefaultPortOption = new Option<bool>("--default", "Returns the default port configured by the application, instead of the currently configured port");
				getPortCommand.AddAlias("port");
				getPortCommand.Add(getDefaultPortOption);
				getPortCommand.SetHandler((c) =>
				{
					RunGetPortCommand(c, getDefaultPortOption);
				});

				var setPortCommand = new Command("setport", "Sets the WS port number to be used by the handler application");
				var newPortValueArgument = new Argument<ushort>("port", "The new port value to be used");
				setPortCommand.Add(newPortValueArgument);
				setPortCommand.SetHandler((c) =>
				{
					RunSetPortCommand(c, newPortValueArgument);
				});

				var rootCommand = new RootCommand("SGR Local Html Interop CLI Application")
				{
					registerCommand,
					unregisterCommand,
					getPortCommand,
					setPortCommand
				};
				rootCommand.AddGlobalOption(nologoOption);
				rootCommand.AddGlobalOption(forceOption);

				rv = rootCommand.Invoke(args);

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
					log.Write(ISimpleLog.FlagError, $"EXCEPTION: {ex}");
				}
			}
			return rv;
		}

		[SupportedOSPlatform("windows")]
		static void CallHandlerElevated(string argument)
		{
			string pipename = "SGR-LocalHtmlInterop-" + Guid.NewGuid().ToString();
			log.Write(EchoingSimpleLog.FlagDontEcho, $"Opening IPC named pipe {pipename}");

			using (NamedPipeServerStream pipeServer = new(pipename, PipeDirection.In, 1))
			using (Process p = new())
			{
				log.Write($"Calling handler app with \"{argument}\"");

				p.StartInfo.FileName = HandlerPath;
				p.StartInfo.Arguments = $"{argument} {pipename}";
				if (!ElevationRightsUtils.IsAdministrator())
				{
					p.StartInfo.UseShellExecute = true;
					p.StartInfo.Verb = "runas";
				}

				p.Start();

				pipeServer.WaitForConnection();
				using (StreamReader reader = new(pipeServer, Encoding.UTF8))
				{
					while (true)
					{
						string? line = reader.ReadLine();
						if (line == null) break;
						log.Write(line);
					}
				}

				log.Write(EchoingSimpleLog.FlagDontEcho, "Pipe stream drained. Waiting for process exit.");
				p.WaitForExit();
			}
		}

		private static void RunRegisterCommand(System.CommandLine.Invocation.InvocationContext context)
		{
			if (!context.ParseResult.GetValueForOption(nologoOption))
			{
				PrintGreeting();
				PrintVersions();
			}
			bool force = context.ParseResult.GetValueForOption(forceOption);

			string? regHanExe = CustomUrlProtocol.GetRegisteredHandlerExe();
			if (regHanExe != null)
			{
				if (!HandlerPath.Equals(regHanExe, StringComparison.CurrentCultureIgnoreCase))
				{
					log.Write(
						force ? ISimpleLog.FlagWarning : ISimpleLog.FlagError,
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
				CallHandlerElevated("-register");
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private static void RunUnregisterCommand(System.CommandLine.Invocation.InvocationContext context)
		{
			if (!context.ParseResult.GetValueForOption(nologoOption))
			{
				PrintGreeting();
				PrintVersions();
			}
			bool force = context.ParseResult.GetValueForOption(forceOption);

			string? regHanExe = CustomUrlProtocol.GetRegisteredHandlerExe();
			if (regHanExe != null)
			{
				if (!HandlerPath.Equals(regHanExe, StringComparison.CurrentCultureIgnoreCase))
				{
					log.Write(
						force ? ISimpleLog.FlagWarning : ISimpleLog.FlagError,
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
				CallHandlerElevated("-unregister");
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private static void RunGetPortCommand(System.CommandLine.Invocation.InvocationContext context, Option<bool> getDefaultPortOption)
		{
			if (!context.ParseResult.GetValueForOption(nologoOption))
			{
				PrintGreeting();
				PrintVersions();
			}
			ServerPortConfig portCfg = new();
			Console.WriteLine(
				context.ParseResult.GetValueForOption(getDefaultPortOption)
				? ServerPortConfig.DefaultPort
				: portCfg.GetValue());
			printBye = false;
		}

		private static void RunSetPortCommand(System.CommandLine.Invocation.InvocationContext context, Argument<ushort> newPortValueArgument)
		{
			if (!context.ParseResult.GetValueForOption(nologoOption))
			{
				PrintGreeting();
				PrintVersions();
			}
			bool force = context.ParseResult.GetValueForOption(forceOption);

			ushort newPortValue = context.ParseResult.GetValueForArgument(newPortValueArgument);
			if (newPortValue < 1024)
			{
				log.Write(
					force ? ISimpleLog.FlagWarning : ISimpleLog.FlagError,
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
					force ? ISimpleLog.FlagWarning : ISimpleLog.FlagError,
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
					log.Write(ISimpleLog.FlagError, $"FAILED to set port to {newPortValue}. Value remains: {portCfg.GetValue()}");
				}
			}
		}

	}
}
