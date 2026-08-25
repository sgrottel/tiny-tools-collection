using SGrottel;
using System;
using System.Text;

namespace Redate;

class Program
{
	internal enum RunMode
	{
		None,
		Init,
		Run,
		FileReg,
		FileUnreg
	};

	static int Main(string[] args)
	{
		Console.WriteLine("Redate");
		EchoingSimpleLog log = new(new SimpleLog());

		try
		{
			CmdLineParser cmd = new CmdLineParser(args);

			switch (cmd.RunMode)
			{
				case RunMode.None:
					break;

				case RunMode.Init:
					log.Write("Init");
					{
						log.Write("Collecting Data");
						cmd.AssertRedateFile();
						string targetDir = System.IO.Path.GetDirectoryName(cmd.RedateFile) + '\\';
						FileCollectionInfoData files = new FileCollectionInfoData();
						files.Collect(cmd.SourceDirs ?? []);
						files.SourceDirsToRelative(targetDir);

						log.Write("Compute MD5s");
						foreach (var f in files.Files ?? [])
						{
							f.ComputeMd5Hash();
							f.PathToRelative(targetDir);
						}

						log.Write("Saving " + System.IO.Path.GetFileName(cmd.RedateFile));
						files.FileDate = DateTime.Now;
						System.IO.File.WriteAllText(
							cmd.RedateFile,
							System.Text.Json.JsonSerializer.Serialize(
								files,
								options: new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }
							),
							new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
							);
					}
					log.Write("Done");
					break;

				case RunMode.Run:
					log.Write("Run");
					{
						log.Write("Loading " + System.IO.Path.GetFileName(cmd.RedateFile));
						cmd.AssertRedateFile();
						FileCollectionInfoData knownFiles
							= System.Text.Json.JsonSerializer.Deserialize<FileCollectionInfoData>(
								json: System.IO.File.ReadAllText(cmd.RedateFile))
							?? throw new InvalidOperationException("Failed to parse redate file");
						string targetDir = System.IO.Path.GetDirectoryName(cmd.RedateFile) + '\\';
						knownFiles.SourceDirsToAbsolute(targetDir);
						foreach (var f in knownFiles.Files ?? []) f.PathToAbsolute(targetDir);

						log.Write("Collecting Data");
						FileCollectionInfoData files = new FileCollectionInfoData();
						files.Collect(knownFiles.SourceDirs ?? []);
						log.Write("Compute MD5s");
						foreach (var f in files.Files ?? []) f.ComputeMd5Hash();

						log.Write("Updating");
						bool isUpdated = knownFiles.Update(files, log);

						if (isUpdated || cmd.ForceFileDateUpdate)
						{
							log.Write("Saving " + System.IO.Path.GetFileName(cmd.RedateFile));
							knownFiles.SourceDirsToRelative(targetDir);
							foreach (var f in knownFiles.Files ?? []) f.PathToRelative(targetDir);
							knownFiles.FileDate = DateTime.Now;
							System.IO.File.WriteAllText(
								cmd.RedateFile,
								System.Text.Json.JsonSerializer.Serialize(
									knownFiles,
									options: new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }
								),
								new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
								);
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
			log.Critical("Fatal error: " + ex);
			ConsoleUtil.WaitBeforeClosingConsole();
			return -1;
		}

		ConsoleUtil.WaitBeforeClosingConsole();
		return 0;
	}
}
