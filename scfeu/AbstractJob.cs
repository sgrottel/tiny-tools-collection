using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scfeu
{
	internal abstract class AbstractJob : IJob {

		private double p = -1.0;
		public double Progress {
			get { return p; }
			protected set {
				if (p != value) {
					p = value;
					FirePropertyChanged(nameof(Progress));
				}
			}
		}

		private bool r = false;
		public bool Running {
			get { return r; }
			protected set {
				if (r != value) {
					r = value;
					FirePropertyChanged(nameof(Running));
				}
			}
		}

		public event EventHandler Done;
		protected void FireDone() {
			Done?.Invoke(this, new EventArgs());
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void FirePropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public abstract void Abort();

		public abstract void Start();

	}
}
