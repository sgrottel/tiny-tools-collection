using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scfeu.Scanned
{
	class RootDirectory : Directory
	{

		private string path = null;
		public string Path {
			get { return path; }
			set {
				if (path != value) {
					path = value;
					Name = path;
					FirePropertyChanged(nameof(Path));
				}
			}
		}

		private int cntSelFiles = 0;
		public int SelectedFiles {
			get { return cntSelFiles; }
			set {
				if (cntSelFiles != value) {
					cntSelFiles = value;
					FirePropertyChanged(nameof(SelectedFiles));
				}
			}
		}

		private static uint countSelFilesRec(Directory dir) {
			uint c = 0;
			foreach (Element e in dir.Children) {
				Directory d = e as Directory;
				if (d != null) c += countSelFilesRec(d);
				File f = e as File;
				if (f != null && f.Selected) c++;
			}
			return c;
		}

		internal void File_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (string.Equals(e?.PropertyName, nameof(Scanned.File.Selected))) {
				SelectedFiles = (int)countSelFilesRec(this);
			}
		}

		private static void getSelectedFiles(List<string> files, Directory dir, string path) {
			foreach (Element e in dir.Children) {
				Directory d = e as Directory;
				if (d != null) getSelectedFiles(files, d, System.IO.Path.Combine(path, d.Name));
				File f = e as File;
				if (f != null && f.Selected) files.Add(System.IO.Path.Combine(path, f.Name));
			}
		}

		internal string[] GetSelectedFiles() {
			List<string> s = new List<string>();
			getSelectedFiles(s, this, Path);
			return s.ToArray();
		}

		private static void rescanFiles(Directory dir, string path) {
			foreach (Element e in dir.Children) {
				Directory d = e as Directory;
				if (d != null) rescanFiles(d, System.IO.Path.Combine(path, d.Name));
				File f = e as File;
				if (f != null) {
					bool s = f.Selected;
					f.Analyse(System.IO.Path.Combine(path, f.Name));
					f.Selected = s;
				}
			}
		}

		internal void RescanFiles() {
			rescanFiles(this, Path);
		}
	}
}
