using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace FolderSummary
{
	public static class Summary
	{

		public class FileData
		{
			[JsonPropertyName("s")]
			public ulong Size { get; set; } = 0;
			[JsonPropertyName("t")]
			public DateTime Date { get; set; } = DateTime.MinValue;
		}

		public class DirectoryData
		{
			[JsonPropertyName("d")]
			public Dictionary<string, DirectoryData>? Directories { get; set; } = null;
			[JsonPropertyName("f")]
			public Dictionary<string, FileData>? Files { get; set; } = null;
		}

		internal static void ScanViaFileSystem(DirectoryData data, DirectoryInfo dir)
		{
			if (data.Files == null) data.Files = new();
			data.Files.Clear();
			foreach (var info in dir.GetFiles())
			{
				if (info.Attributes.HasFlag(FileAttributes.System)
					|| info.Attributes.HasFlag(FileAttributes.Temporary)
					|| info.Attributes.HasFlag(FileAttributes.Hidden))
				{
					continue;
				}

				data.Files[info.Name] = new FileData() { Size = (ulong)info.Length, Date = info.LastWriteTimeUtc };
			}
			if (!data.Files.Any()) data.Files = null;

			if (data.Directories == null) data.Directories = new();
			data.Directories.Clear();
			foreach (var info in dir.GetDirectories())
			{
				if (info.Attributes.HasFlag(FileAttributes.System)
					|| info.Attributes.HasFlag(FileAttributes.Temporary)
					|| info.Attributes.HasFlag(FileAttributes.Hidden))
				{
					continue;
				}

				DirectoryData dd = new();
				ScanViaFileSystem(dd, info);
				data.Directories[info.Name] = dd;
			}
			if (!data.Directories.Any()) data.Directories = null;

		}

		[SupportedOSPlatform("windows")]
		private static void ScanViaEverything(DirectoryData d, DirectoryInfo dir)
		{
			DirectoryData root = d;
			for (DirectoryInfo? di = dir;di != null; di = di.Parent)
			{
				DirectoryData dd = root;
				root = new() { Directories = new() };
				root.Directories[di.Name.Trim(Path.DirectorySeparatorChar)] = dd;
			}

			EverythingSearchClient.SearchClient everything = new();
			var res = everything.Search($"{dir.FullName}\\ ");

			foreach (EverythingSearchClient.Result.Item file in res.Items)
			{
				var attr = file.FileAttributes ?? EverythingSearchClient.Result.ItemFileAttributes.None;
				if (attr.HasFlag(EverythingSearchClient.Result.ItemFileAttributes.Hidden)
					|| attr.HasFlag(EverythingSearchClient.Result.ItemFileAttributes.System))
				{
					continue;
				}

				DirectoryData dd = root;
				DirectoryData? ndd = null;
				foreach (string p in file.Path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
				{
					if (!(dd.Directories?.TryGetValue(p, out ndd) ?? false))
					{
						ndd = new DirectoryData();
						if (dd.Directories == null) dd.Directories = new();
						dd.Directories[p] = ndd;
					}
					dd = ndd;
				}

				if (file.Flags.HasFlag(EverythingSearchClient.Result.ItemFlags.Folder))
				{
					if (!(dd.Directories?.TryGetValue(file.Name, out ndd) ?? false))
					{
						ndd = new DirectoryData();
						if (dd.Directories == null) dd.Directories = new();
						dd.Directories[file.Name] = ndd;
					}
				}
				else
				{
					if (dd.Files == null) dd.Files = new();
					FileData newFile = new() { Size = file.Size ?? 0, Date = file.LastWriteTime ?? DateTime.MinValue };
					dd.Files[file.Name] = newFile;
				}
			}

		}

		public static DirectoryData Scan(DirectoryInfo dir)
		{
			DirectoryData d = new();

			if (OperatingSystem.IsWindows() &&
				EverythingSearchClient.SearchClient.IsEverythingAvailable())
			{
				ScanViaEverything(d, dir);
			}
			else
			{
				ScanViaFileSystem(d, dir);
			}

			return d;
		}

		public static void SaveJson(DirectoryData data, FileInfo filename)
		{
			JsonSerializerOptions opt = new() {
				//WriteIndented = true,
				MaxDepth = 200,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // ok, because file is meant to be only read by this application again
			};
			using (FileStream file = new(filename.FullName, FileMode.Create, FileAccess.Write))
			JsonSerializer.Serialize(file, data, opt);
		}

		internal static DirectoryData LoadJson(FileInfo filename)
		{
			using (FileStream file = new(filename.FullName, FileMode.Open, FileAccess.Read))
			return JsonSerializer.Deserialize<DirectoryData>(file) ?? throw new Exception("Json file did not contain valid data");
		}
	}
}
