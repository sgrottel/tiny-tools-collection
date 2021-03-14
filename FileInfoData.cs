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
		public long Length { get; set; }
		public DateTime WriteTime { get; set; }
		public FileAttributes Attributes { get; set; }
		public string Md5Hash { get; set; }

		public FileInfoData(string fn)
		{
			if (!File.Exists(fn)) throw new FileNotFoundException("Cannot create FileInfoData for non-existing file", fn);
			Path = fn;
			FileInfo fi = new FileInfo(fn);
			Length = fi.Length;
			WriteTime = fi.LastWriteTimeUtc;
			Attributes = fi.Attributes;
			Md5Hash = null;
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
	}
}
