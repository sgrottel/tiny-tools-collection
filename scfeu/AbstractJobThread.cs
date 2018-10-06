using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
			} catch {
			} finally {
				Running = false;
				try {
					FireDone();
				} catch { }
			}
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
