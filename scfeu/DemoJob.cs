using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace scfeu {

	class DemoJob : AbstractJobThread {

		protected override void work() {
			Progress = -1.0;

			Thread.Sleep(5000);

			for (int i = 1; i < 500; ++i) {
				Progress = (double)i / 500.0;
				Thread.Sleep(10);
			}
		}
	}
}
