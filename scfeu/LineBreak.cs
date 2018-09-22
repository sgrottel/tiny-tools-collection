using System;
using System.Collections;
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

		[Description("Mac  (CR \\r)")]
		Mac
	}

	public class LineBreakComparer : IComparer {
		// Call CaseInsensitiveComparer.Compare with the parameters reversed.
		public int Compare(Object x, Object y) {
			return (new CaseInsensitiveComparer()).Compare(((LineBreak)x).ToString(), ((LineBreak)y).ToString());
		}
	}
}
