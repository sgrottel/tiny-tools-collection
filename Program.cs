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
				CmdLineParser cmd = new CmdLineParser(args);

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
							knownFiles.Update(files);

							Console.WriteLine("Saving " + System.IO.Path.GetFileName(cmd.RedateFile));
							knownFiles.SourceDirsToRelative(targetDir);
							foreach (var f in knownFiles.Files) f.PathToRelative(targetDir);
							knownFiles.FileDate = DateTime.Now;
							JsonSerializerSettings s = new JsonSerializerSettings();
							s.Culture = CultureInfo.InvariantCulture;
							s.Formatting = Formatting.Indented;
							System.IO.File.WriteAllText(cmd.RedateFile, JsonConvert.SerializeObject(knownFiles, s), new UTF8Encoding(false));

						}
						Console.WriteLine("Done");
						break;
					case RunMode.FileReg:
						RegisterFileType();
						break;
					case RunMode.FileUnreg:
						UnregisterFileType();
						break;
					default:
						throw new NotImplementedException();
				}

			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("Fatal error: " + ex);
				return -1;
			}
			return 0;
		}

		private static void RegisterFileType()
		{
			throw new NotImplementedException();
		}

		private static void UnregisterFileType()
		{
			throw new NotImplementedException();
		}

	}
}
