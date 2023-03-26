using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scfeu.Scanned {

	class Directory : Element {

		private ObservableCollection<Element> children = new ObservableCollection<Element>();
		public ObservableCollection<Element> Children {
			get { return children; }
			set {
				if (children != value) {
					children = value;
					FirePropertyChanged(nameof(Children));
				}
			}
		}

		public void AllFiles(bool select) {
			foreach (Element e in Children) {
				(e as Directory)?.AllFiles(select);
				File f = e as File;
				if (f != null) f.Selected = select;
			}
		}
	}

}

namespace scfeu {
	static internal class DirectoryUtil {

		static private int Compare(Scanned.Element e1, Scanned.Element e2) {
			if (e1 is Scanned.Directory && e2 is Scanned.File) return -1;
			if (e2 is Scanned.Directory && e1 is Scanned.File) return 1;

			return string.Compare(e1.Name, e2.Name, true);
		}

		static internal void SortIn(this ObservableCollection<Scanned.Element> col, Scanned.Element e) {
			if (col.Count <= 0) { col.Add(e); return; }

			if (Compare(col.Last(), e) < 0) { col.Add(e); return; }

			for (int i = 0; i < col.Count; ++i) {
				if (Compare(col[i], e) > 0) {
					col.Insert(i, e);
					return;
				}
			}

			col.Add(e);
		}
	}
}
