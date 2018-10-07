using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scfeu {

	class FixFilesJob : AbstractJobThread {

		public JobSetup Setup { get; set; }
		public string[] Files { get; set; }

		protected override void work() {
			if (Setup == null) throw new ArgumentNullException();
			if (Files == null) throw new ArgumentNullException();
			if (Files.Length <= 0) throw new ArgumentNullException();

			int fi = 1;
			int fc = 2 * Files.Length;
			foreach (string file in Files) {
				Progress = (double)fi / (double)fc;

				fixFile(file);

				fi += 2;
			}
		}

		private void fixFile(string file) {
			//throw new NotImplementedException();
		}
	}
}
