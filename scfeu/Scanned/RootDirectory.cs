using System;
using System.Collections.Generic;
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

	}
}
