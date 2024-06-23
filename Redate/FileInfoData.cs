using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Redate
{
	public class FileInfoData
	{
		public string Path { get; set; }
		public long Length { get; set; } = 0;
		public DateTime WriteTime { get; set; } = DateTime.MinValue;
		public FileAttributes Attributes { get; set; } = 0;
		public string Md5Hash { get; set; } = null;

		public FileInfoData(string fn)
		{
			Path = fn;
			if (File.Exists(Path)) Collect();
		}

		public void Collect()
		{
			if (!File.Exists(Path)) throw new FileNotFoundException("Cannot create FileInfoData for non-existing file", Path);
			FileInfo fi = new FileInfo(Path);
			Length = fi.Length;
			WriteTime = fi.LastWriteTimeUtc;
			Attributes = fi.Attributes;
		}

		public void ComputeMd5Hash()
		{
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(Path))
				{
					var hash = md5.ComputeHash(stream);
					Md5Hash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
				}
			}
		}

		internal void PathToRelative(string v)
		{
			if (Path.StartsWith(v, StringComparison.InvariantCultureIgnoreCase))
			{
				Path = Path.Substring(v.Length);
			}
		}

		internal void PathToAbsolute(string targetDir)
		{
			Path = System.IO.Path.Combine(targetDir, Path);
		}
	}
}
