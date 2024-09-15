using Newtonsoft.Json;
using SGrottel;
using System;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace Redate
{
	class Program
	{
		internal enum RunMode
		{
			Init,
			Run,
			FileReg,
			FileUnreg
		};

		static int Main(string[] args)
		{
			Console.WriteLine("Redate");
			EchoingSimpleLog log = new();

			try
			{
				CmdLineParser cmd = null;
				try
				{
					cmd = new CmdLineParser(args);
				}
				catch
				{
					Console.WriteLine();
					SimpleLog.Error(log, "Error parsing command line.");
					Console.WriteLine();

					CmdLineParser.PrintHelp();

					throw;
				}

				switch (cmd.RunMode)
				{
					case RunMode.Init:
						log.Write("Init");
						{
							log.Write("Collecting Data");
							string targetDir = System.IO.Path.GetDirectoryName(cmd.RedateFile) + '\\';
							FileCollectionInfoData files = new FileCollectionInfoData() { Log = log };
							files.Collect(cmd.SourceDirs);
							files.SourceDirsToRelative(targetDir);

							log.Write("Compute MD5s");
							foreach (var f in files.Files)
							{
								f.ComputeMd5Hash();
								f.PathToRelative(targetDir);
							}

							log.Write("Saving " + System.IO.Path.GetFileName(cmd.RedateFile));
							files.FileDate = DateTime.Now;
							JsonSerializerSettings s = new JsonSerializerSettings();
							s.Culture = CultureInfo.InvariantCulture;
							s.Formatting = Formatting.Indented;
							System.IO.File.WriteAllText(cmd.RedateFile, JsonConvert.SerializeObject(files, s), new UTF8Encoding(false));
						}
						log.Write("Done");
						break;

					case RunMode.Run:
						log.Write("Run");
						{
							log.Write("Loading " + System.IO.Path.GetFileName(cmd.RedateFile));
							FileCollectionInfoData knownFiles = JsonConvert.DeserializeObject<FileCollectionInfoData>(System.IO.File.ReadAllText(cmd.RedateFile));
							knownFiles.Log = log;
							string targetDir = System.IO.Path.GetDirectoryName(cmd.RedateFile) + '\\';
							knownFiles.SourceDirsToAbsolute(targetDir);
							foreach (var f in knownFiles.Files) f.PathToAbsolute(targetDir);

							log.Write("Collecting Data");
							FileCollectionInfoData files = new FileCollectionInfoData() { Log = log };
							files.Collect(knownFiles.SourceDirs);
							log.Write("Compute MD5s");
							foreach (var f in files.Files) f.ComputeMd5Hash();

							log.Write("Updating");
							bool isUpdated = knownFiles.Update(files);

							if (isUpdated || cmd.ForceFileDateUpdate)
							{
								log.Write("Saving " + System.IO.Path.GetFileName(cmd.RedateFile));
								knownFiles.SourceDirsToRelative(targetDir);
								foreach (var f in knownFiles.Files) f.PathToRelative(targetDir);
								knownFiles.FileDate = DateTime.Now;
								JsonSerializerSettings s = new JsonSerializerSettings();
								s.Culture = CultureInfo.InvariantCulture;
								s.Formatting = Formatting.Indented;
								System.IO.File.WriteAllText(cmd.RedateFile, JsonConvert.SerializeObject(knownFiles, s), new UTF8Encoding(false));
							}
							else
							{
								log.Write("Files unchanged - No need to save.");
							}

						}
						log.Write("Done");
						break;
					case RunMode.FileReg:
						log.Write("File Type Registration");
						FileTypeReg.Register();
						log.Write("Done");
						break;
					case RunMode.FileUnreg:
						log.Write("File Type Unregistration");
						FileTypeReg.Unregister();
						log.Write("Done");
						break;
					default:
						throw new NotImplementedException();
				}

			}
			catch (Exception ex)
			{
				SimpleLog.Error(log, "Fatal error: " + ex);
				WaitBeforeClosingConsole();
				return -1;
			}

			WaitBeforeClosingConsole();
			return 0;
		}

		private static void WaitBeforeClosingConsole()
		{
			if (!IsSelfhostedConsole) return;
			Wait(defaultTimeoutSeconds);
		}

		const int defaultTimeoutSeconds = 20;

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		private static extern int GetConsoleProcessList(int[] buffer, int size);

		public static bool IsSelfhostedConsole {
			get {
				return GetConsoleProcessList(new int[2], 2) <= 1;
			}
		}

		public static void Wait(int timeoutSeconds)
		{
			if (timeoutSeconds == 0) return;
			if (Console.IsOutputRedirected) return;

			while (Console.KeyAvailable) Console.ReadKey();

			Console.Write("Hit any key to continue...");

			if (timeoutSeconds < 0)
			{
				Console.ReadKey();
			}
			else
			{
				DateTime start = DateTime.Now;
				while (((int)(DateTime.Now - start).TotalSeconds) < timeoutSeconds)
				{
					Console.Write("\rHit any key to continue... {0} ", timeoutSeconds - (int)(DateTime.Now - start).TotalSeconds);
					if (Console.KeyAvailable) break;
					System.Threading.Thread.Sleep(10);
				}
				Console.Write("\rHit any key to continue...     ");
			}

			while (Console.KeyAvailable) Console.ReadKey();
			Console.WriteLine();
		}

	}
}
