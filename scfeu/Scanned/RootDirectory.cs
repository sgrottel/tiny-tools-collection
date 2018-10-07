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
	}
}
