using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scfeu {

	public enum LineBreak {

		[Description("Unix  (LF \\n)")]
		Unix,

		[Description("Windows  (CR+LF \\r\\f)")]
		Windows,

		[Description("Max  (CR \\r)")]
		Mac
	}

}
