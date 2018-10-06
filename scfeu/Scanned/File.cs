using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scfeu.Scanned
{
	class File : Element
	{
		private bool enabled = false;
		private bool selected = false;
		private Encoding encoding = null;
		private int cntWinNL = 0;
		private int cntUnixNL = 0;
		private int cntMacNL = 0;
		private bool trailingWS = false;
		private int indentTabs = 0;
		private int indentSpaces = 0;
		private object testFilePath;

		public bool Enabled {
			get { return enabled; }
			set {
				if (enabled != value) {
					enabled = value;
					FirePropertyChanged(nameof(Enabled));
				}
			}
		}

		public bool Selected {
			get { return selected; }
			set {
				if (selected != value) {
					selected = value;
					FirePropertyChanged(nameof(Selected));
				}
			}
		}

		public Encoding Encoding {
			get { return encoding; }
			set {
				if (encoding != value) {
					encoding = value;
					FirePropertyChanged(nameof(Encoding));
					FirePropertyChanged(nameof(EncodingName));
				}
			}
		}

		public string EncodingName {
			get { return encoding == null ? "Unknown / Binary" : encoding.WebName; }
		}

		public string LineBreakInfo {
			get {
				string s = "";
				// TODO: Implement
				return string.IsNullOrEmpty(s) ? "?" : s;
			}
		}

		public string LeadingSpaceInfo {
			get {
				string s = "";
				// TODO: Implement
				return string.IsNullOrEmpty(s) ? "?" : s;
			}
		}

		public string TrailingSpaceInfo {
			get {
				if (encoding == null) return "?";
				return trailingWS.ToString();
			}
		}

		internal void Analyse(string path) {

			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
				Encoding = null;
				{
					Ude.CharsetDetector cdet = new Ude.CharsetDetector();
					cdet.Feed(fs);
					cdet.DataEnd();
					if (cdet.Charset != null) {
						if (cdet.Confidence > 0.85) {
							try {
								Encoding = Encoding.GetEncoding(cdet.Charset);
							} catch {
								throw;
							}
						}
					}
				}
			}

		}

	}
}
