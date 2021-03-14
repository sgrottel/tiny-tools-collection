using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redate
{

	/// <summary>
	/// The files
	/// </summary>
	public class FileCollectionInfoData
	{
		const string DefaultFileType = "REDATE-1-0";

		public string FileType { get; set; } = DefaultFileType;

		public DateTime FileDate { get; set; } = DateTime.Now;

		public string[] SourceDirs { get; set; } = null;

		public FileInfoData[] Files { get; set; } = null;

		internal void Collect(string[] sourceDirs)
		{
			SourceDirs = sourceDirs;
			List<FileInfoData> files = new List<FileInfoData>();
			foreach (string srcdir in sourceDirs)
			{
				collect(files, srcdir);
			}
			Files = files.ToArray();
		}

		private void collect(List<FileInfoData> files, string srcdir)
		{
			if (!Directory.Exists(srcdir)) return;
			foreach(string fn in Directory.GetFiles(srcdir)) files.Add(new FileInfoData(fn));
			foreach (string dn in Directory.GetDirectories(srcdir)) collect(files, dn);
		}

		internal void SourceDirsToRelative(string targetDir)
		{
			for (int i = 0; i < SourceDirs.Length; ++i)
			{
				if (SourceDirs[i].StartsWith(targetDir, StringComparison.InvariantCultureIgnoreCase))
				{
					SourceDirs[i] = SourceDirs[i].Substring(targetDir.Length);
				}
			}
		}
	}

}
