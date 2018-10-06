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

	}

}

namespace scfeu {
	static internal class DirectoryUtil {
		static internal void SortIn(this ObservableCollection<Scanned.Element> col, Scanned.Element e) {
			// TODO: Implement sorted insert
			col.Add(e);
		}
	}
}
