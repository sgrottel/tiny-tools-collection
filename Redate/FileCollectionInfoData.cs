using SGrottel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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

		// The `Log` member was serialized before.
		// Therefore, to stay compatible with old files, this member name must never be used again.

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
		internal bool Update(FileCollectionInfoData files, ISimpleLog Log)
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

			List<FileInfoData> kfs = new List<FileInfoData>();
			foreach (FileInfoData fid in Files)
			{
				FileInfoData newfid = files.Files.FirstOrDefault((FileInfoData i) => { return i.Path.Equals(fid.Path, StringComparison.InvariantCultureIgnoreCase); });
				if (newfid == null)
				{
					Log.Write(fid.Path + " - Removed");
					retval = true;
					continue; // skip fid, will kill it
				}

				if (newfid.Length != fid.Length)
				{
					kfs.Add(newfid);
					Log.Write(fid.Path + " - Size changed");
					retval = true;
					continue;
				}
				if (!newfid.Md5Hash.Equals(fid.Md5Hash, StringComparison.InvariantCultureIgnoreCase))
				{
					kfs.Add(newfid);
					Log.Write(fid.Path + " - Content changed");
					retval = true;
					continue;
				}

				// length and md5 are equal
				if (newfid.Attributes != fid.Attributes || newfid.WriteTime != fid.WriteTime)
				{
					Log.Write(fid.Path + " - Restoring Date and Attributes");
					// set file info
					System.IO.File.SetLastWriteTimeUtc(fid.Path, fid.WriteTime);
					System.IO.File.SetAttributes(fid.Path, fid.Attributes);

					// update and take
					newfid.Collect();
					kfs.Add(newfid);
					continue;
				}

				Log.Write(fid.Path + " - Unchanged");
				kfs.Add(fid);
			}

			foreach (FileInfoData fid in files.Files)
			{
				FileInfoData oldfid = Files.FirstOrDefault((FileInfoData i) => { return i.Path.Equals(fid.Path, StringComparison.InvariantCultureIgnoreCase); });
				if (oldfid != null) continue; // already there
				Log.Write(fid.Path + " - Added");
				retval = true;
				kfs.Add(fid); // new --> add
			}

			Files = kfs.ToArray();

			return retval;
		}
	}

}
