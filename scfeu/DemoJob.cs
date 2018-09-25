using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace scfeu {

	class DemoJob : IJob {

		private double p = -1.0;
		public double Progress {
			get { return p; }
			private set {
				if (p != value) {
					p = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
				}
			}
		}

		public bool running { get; private set; } = false;

		public event EventHandler Done;
		public event PropertyChangedEventHandler PropertyChanged;

		private Thread t = null;

		private void run() {
			try {

				Progress = -1.0;

				Thread.Sleep(5000);

				for (int i = 1; i < 500; ++i) {
					Progress = (double)i / 500.0;
					Thread.Sleep(10);
				}

			} catch(ThreadAbortException) {
			} catch {
			} finally {
				running = false;
				try {
					Done?.Invoke(this, null);
				} catch { }
			}
		}

		public void Start() {
			if (t != null) throw new NotSupportedException();
			t = new Thread(run);
			running = true;
			t.Start();
		}

		public void Abort() {
			t.Abort();
		}

	}
}
