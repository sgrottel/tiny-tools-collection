using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FolderSummary
{
	public static class Summary
	{

		public class FileData
		{
			public string Name { get; set; } = string.Empty;
			public ulong Size { get; set; } = 0;
			public DateTime Date { get; set; } = DateTime.MinValue;
		}

		public class DirectoryData
		{
			public string Name { get; set; } = string.Empty;

			[JsonIgnore]
			public List<DirectoryData> Directories { get; set; } = new();
			
			[JsonIgnore]
			public List<FileData> Files { get; set; } = new();

			[JsonPropertyName("Directories")]
			public DirectoryData[]? JsonDirectories
			{
				get
				{
					if (Directories.Count == 0) return null;
					return Directories.ToArray();
				}
				set
				{
					Directories.Clear();
					if (value != null)
					{
						Directories.AddRange(value);
					}
				}
			}

			[JsonPropertyName("Files")]
			public FileData[]? JsonFiles {
				get
				{
					if (Files.Count == 0) return null;
					return Files.ToArray();
				}
				set
				{
					Files.Clear();
					if (value != null)
					{
						Files.AddRange(value);
					}
				}
			}
		}

		public static DirectoryData Scan(DirectoryInfo dir)
		{
			DirectoryData d = new() { Name = dir.Name };
			d.Files.Add(new FileData() { Name = "Dummy", Date = DateTime.Now });

			// TODO: Implement

			return d;
		}

		public static void SaveJson(DirectoryData data, FileInfo filename)
		{
			JsonSerializerOptions opt = new() {
				WriteIndented = true,
				MaxDepth = 200,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
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
