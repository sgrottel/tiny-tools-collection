using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scfeu.Scanned
{
	class Element : INotifyPropertyChanged {

		private string name = null;
		public string Name {
			get { return name; }
			set {
				if (name != value) {
					name = value;
					FirePropertyChanged(nameof(Name));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void FirePropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

	}
}
