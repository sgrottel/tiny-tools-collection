using SGrottel;
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
			if (!dn.EndsWith("-windows")) {
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

		static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			PrintGreeting();
			PrintVersions();

			ISimpleLog? log = null;
			try
			{
				log = new EchoingSimpleLog();



				// log.Write($"Called:\n\t{string.Join("\n\t", args)}");

				if (OperatingSystem.IsWindows())
				{
					CallHandlerElevated("-register", log);
				}



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

		[SupportedOSPlatform("windows")]
		static void CallHandlerElevated(string argument, ISimpleLog log)
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

	}
}
