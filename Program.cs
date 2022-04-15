using Newtonsoft.Json;
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
					Console.WriteLine("Error parsing command line.");
					Console.WriteLine();

					CmdLineParser.PrintHelp();

					throw;
				}

				switch (cmd.RunMode)
				{
					case RunMode.Init:
						Console.WriteLine("Init");
						{
							Console.WriteLine("Collecting Data");
							string targetDir = System.IO.Path.GetDirectoryName(cmd.RedateFile) + '\\';
							FileCollectionInfoData files = new FileCollectionInfoData();
							files.Collect(cmd.SourceDirs);
							files.SourceDirsToRelative(targetDir);

							Console.WriteLine("Compute MD5s");
							foreach (var f in files.Files)
							{
								f.ComputeMd5Hash();
								f.PathToRelative(targetDir);
							}

							Console.WriteLine("Saving " + System.IO.Path.GetFileName(cmd.RedateFile));
							files.FileDate = DateTime.Now;
							JsonSerializerSettings s = new JsonSerializerSettings();
							s.Culture = CultureInfo.InvariantCulture;
							s.Formatting = Formatting.Indented;
							System.IO.File.WriteAllText(cmd.RedateFile, JsonConvert.SerializeObject(files, s), new UTF8Encoding(false));
						}
						Console.WriteLine("Done");
						break;

					case RunMode.Run:
						Console.WriteLine("Run");
						{
							Console.WriteLine("Loading " + System.IO.Path.GetFileName(cmd.RedateFile));
							FileCollectionInfoData knownFiles = JsonConvert.DeserializeObject<FileCollectionInfoData>(System.IO.File.ReadAllText(cmd.RedateFile));
							string targetDir = System.IO.Path.GetDirectoryName(cmd.RedateFile) + '\\';
							knownFiles.SourceDirsToAbsolute(targetDir);
							foreach (var f in knownFiles.Files) f.PathToAbsolute(targetDir);

							Console.WriteLine("Collecting Data");
							FileCollectionInfoData files = new FileCollectionInfoData();
							files.Collect(knownFiles.SourceDirs);
							Console.WriteLine("Compute MD5s");
							foreach (var f in files.Files) f.ComputeMd5Hash();

							Console.WriteLine("Updating");
							bool isUpdated = knownFiles.Update(files);

							if (isUpdated || cmd.ForceFileDateUpdate)
							{
								Console.WriteLine("Saving " + System.IO.Path.GetFileName(cmd.RedateFile));
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
								Console.WriteLine("Files unchanged - No need to save.");
                            }

						}
						Console.WriteLine("Done");
						break;
					case RunMode.FileReg:
						Console.WriteLine("File Type Registration");
						FileTypeReg.Register();
						Console.WriteLine("Done");
						break;
					case RunMode.FileUnreg:
						Console.WriteLine("File Type Unregistration");
						FileTypeReg.Unregister();
						Console.WriteLine("Done");
						break;
					default:
						throw new NotImplementedException();
				}

			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("Fatal error: " + ex);
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
