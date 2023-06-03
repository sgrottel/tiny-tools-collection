using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

		internal static void Scan(DirectoryData data, DirectoryInfo dir)
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
				Scan(dd, info);
				data.Directories[info.Name] = dd;
			}
			if (!data.Directories.Any()) data.Directories = null;

		}

		public static DirectoryData Scan(DirectoryInfo dir)
		{
			DirectoryData d = new();

			Scan(d, dir);

			// TODO: Implement

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
