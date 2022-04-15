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

		internal void SourceDirsToAbsolute(string targetDir)
		{
			for (int i = 0; i < SourceDirs.Length; ++i)
			{
				SourceDirs[i] = System.IO.Path.Combine(targetDir, SourceDirs[i]);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="files"></param>
		/// <returns>True if the information of any file has been updated</returns>
		internal bool Update(FileCollectionInfoData files)
		{
			bool retval = false;
			// this is the old/known state
			// files is the potentially new state

			// Update:
			//   File only in this --> delete
			//   File only in files --> add
			//   Files in both
			//     length inequal --> files
			//     md5 inequal --> files
			//       use known --> set date & attributes on existing file

			Console.WriteLine();
			List<FileInfoData> kfs = new List<FileInfoData>();
			foreach (FileInfoData fid in Files)
			{
				Console.Write(fid.Path + " - ");

				FileInfoData newfid = files.Files.FirstOrDefault((FileInfoData i) => { return i.Path.Equals(fid.Path, StringComparison.InvariantCultureIgnoreCase); });
				if (newfid == null)
				{
					Console.WriteLine("Removed");
					retval = true;
					continue; // skip fid, will kill it
				}

				if (newfid.Length != fid.Length)
				{
					kfs.Add(newfid);
					Console.WriteLine("Size changed");
					retval = true;
					continue;
				}
				if (!newfid.Md5Hash.Equals(fid.Md5Hash, StringComparison.InvariantCultureIgnoreCase))
				{
					kfs.Add(newfid);
					Console.WriteLine("Content changed");
					retval = true;
					continue;
				}

				// length and md5 are equal
				if (newfid.Attributes != fid.Attributes || newfid.WriteTime != fid.WriteTime)
				{
					Console.WriteLine("Restoring Date and Attributes");
					// set file info
					System.IO.File.SetLastWriteTimeUtc(fid.Path, fid.WriteTime);
					System.IO.File.SetAttributes(fid.Path, fid.Attributes);

					// update and take
					newfid.Collect();
					kfs.Add(newfid);
					continue;
				}

				Console.WriteLine("Unchanged");
				kfs.Add(fid);
			}

			foreach (FileInfoData fid in files.Files)
			{
				FileInfoData oldfid = Files.FirstOrDefault((FileInfoData i) => { return i.Path.Equals(fid.Path, StringComparison.InvariantCultureIgnoreCase); });
				if (oldfid != null) continue; // already there
				Console.WriteLine(fid.Path + " - Added");
				retval = true;
				kfs.Add(fid); // new --> add
			}

			Console.WriteLine();

			Files = kfs.ToArray();

			return retval;
		}
	}

}
