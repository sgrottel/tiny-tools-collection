using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scfeu.Properties;

namespace scfeu
{
	public class JobSetup : INotifyPropertyChanged
	{

		public const string DefaultIncludePattern
			= "*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx;*.hh;*.inl;*.ipp;*.rc;*.resx;*.idl;*.asm;*.inc;" // C++ project and source files
			+ "*.cs;*.resx;*.resw;*.xsd;*.wsdl;*.xaml;*.xml;*.htm;*.html;*.css;" // C# project and source files
			+ "*.frg;*.vrt;*.geo;*.comp;*.tessctrl;*.tesseval;" // MORF shader files
			+ "*.glsl;*.frag;*.vert;*.geom;*.tesc;*.tese;" // Other known glsl files
			+ "*.md;*.txt"; // General text files
		public const string DefaultExcludePattern = "*.sln;.git;.vs";

		private string dir;
		public string Directory {
			get { return dir; }
			set {
				if (dir != value) {
					dir = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Directory)));
					evalCanScanDir();
				}
			}
		}

		private string incPat = DefaultIncludePattern;
		public string IncludePattern {
			get { return incPat; }
			set {
				if (incPat != value) {
					incPat = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IncludePattern)));
				}
			}
		}

		private string exPat = DefaultExcludePattern;
		public string ExcludePattern {
			get { return exPat; }
			set {
				if (exPat != value) {
					exPat = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExcludePattern)));
				}
			}
		}

		private LineBreak lb = LineBreak.Unix;
		public LineBreak LineBreak {
			get { return lb; }
			set {
				if (lb != value) {
					lb = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LineBreak)));
				}
			}
		}

		private Encoding enc = Encoding.UTF8;
		public Encoding Encoding {
			get { return enc; }
			set {
				if (enc != value) {
					enc = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Encoding)));
				}
			}
		}

		private bool removeTrailingWhitespace = true;
		public bool RemoveTrailingWhitespace {
			get { return removeTrailingWhitespace; }
			set {
				if (removeTrailingWhitespace != value) {
					removeTrailingWhitespace = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemoveTrailingWhitespace)));
				}
			}
		}

		private int tabSize = 4;
		public int TabSize {
			get { return tabSize; }
			set {
				if (tabSize != value) {
					tabSize = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TabSize)));
				}
			}
		}

		private LeadingWhitespace leadingWhitespace = LeadingWhitespace.MajorityVoteTabs;
		public LeadingWhitespace LeadingWhitespace {
			get { return leadingWhitespace; }
			set {
				if (leadingWhitespace != value) {
					leadingWhitespace = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LeadingWhitespace)));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private bool enabled = true;
		public bool IsEnabled {
			get { return enabled; }
			set {
				if (enabled != value) {
					enabled = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
					evalCanScanDir();
					evalCanFixFiles();
				}
			}
		}

		private bool canScanDir = false;
		public bool CanScanDir {
			get { return canScanDir; }
			set {
				if (canScanDir != value) {
					canScanDir = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanScanDir)));
				}
			}
		}

		private void evalCanScanDir() {
			CanScanDir = enabled
				&& System.IO.Directory.Exists(dir);
		}

		private bool canFixFiles = false;
		public bool CanFixFiles {
			get { return canFixFiles; }
			set {
				if (canFixFiles != value) {
					canFixFiles = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanFixFiles)));
				}
			}
		}

		private void evalCanFixFiles() {
			CanFixFiles = enabled
				&& false; // TODO: Implement
		}

		internal void LoadFrom(Settings s) {
			try {
				if (System.IO.Directory.Exists(s.Directory))
					Directory = s.Directory;
			} catch { }
			try {
				if (!string.IsNullOrWhiteSpace(s.IncludePattern))
					IncludePattern = s.IncludePattern;
			} catch { }
			try {
				if (!string.IsNullOrWhiteSpace(s.ExcludePattern))
					ExcludePattern = s.ExcludePattern;
			} catch { }
			try {
				if (!string.IsNullOrWhiteSpace(s.LineBreak))
					LineBreak = (LineBreak)Enum.Parse(typeof(LineBreak), s.LineBreak);
			} catch { }
			try {
				if (!string.IsNullOrWhiteSpace(s.Encoding))
					Encoding = Encoding.GetEncoding(s.Encoding);
			} catch { }
			try {
				if (!string.IsNullOrWhiteSpace(s.TrailingWhitespace))
					RemoveTrailingWhitespace = bool.Parse(s.TrailingWhitespace);
			} catch { }
			try {
				if (!string.IsNullOrWhiteSpace(s.LeadingWhitespace))
					LeadingWhitespace = (LeadingWhitespace)Enum.Parse(typeof(LeadingWhitespace), s.LeadingWhitespace);
			} catch { }
			try {
				if (!string.IsNullOrWhiteSpace(s.TabSize))
					tabSize = int.Parse(s.TabSize);
			} catch { }

		}

		internal void SaveTo(Settings s) {
			s.Directory = Directory;
			if (!string.Equals(IncludePattern, DefaultIncludePattern))
				s.IncludePattern = IncludePattern;
			if (!string.Equals(ExcludePattern, DefaultExcludePattern))
				s.ExcludePattern = ExcludePattern;
			s.LineBreak = LineBreak.ToString();
			s.Encoding = Encoding.WebName;
			s.TrailingWhitespace = removeTrailingWhitespace.ToString();
			s.LeadingWhitespace = leadingWhitespace.ToString();
			s.TabSize = tabSize.ToString();
		}
	}
}
