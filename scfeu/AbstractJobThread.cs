using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace scfeu
{

	internal abstract class AbstractJobThread : AbstractJob {

		private Thread t = null;

		protected abstract void work();

		private void run() {
			try {
				work();
				Progress = 1.0;
			} catch (ThreadAbortException) {
			} catch (Exception e) {
				MessageBox.Show("ERROR: " + e.ToString(), "scfeu", MessageBoxButton.OK, MessageBoxImage.Error);
			} finally {
				Running = false;
				try {
					FireDone();
				} catch { }
			}
		}

		protected void Dispatch(Action a) {
			Application.Current.Dispatcher.BeginInvoke(a, System.Windows.Threading.DispatcherPriority.Background, null);
		}

		public override void Start() {
			if (t != null) throw new NotSupportedException();
			t = new Thread(run);
			Running = true;
			Progress = -1.0;
			t.Start();
		}

		public override void Abort() {
			t.Abort();
		}

	}

}
