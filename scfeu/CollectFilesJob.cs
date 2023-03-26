using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace scfeu
{

	internal class CollectFilesJob : AbstractJobThread {

		public JobSetup JobSetup { get; set; } = null;
		public Scanned.RootDirectory Root { get; set; } = null;

		private class Pattern {

			private Regex regex;

			public static Pattern FromString(string s) {
				Pattern p = new Pattern();
				p.regex = new Regex("^" + Regex.Escape(s).Replace(@"\*", ".*").Replace(@"\?", ".") + "$");
				return p;
			}

			public bool Matches(string i) {
				return regex.IsMatch(i);
			}
		};

		private Pattern[] excludes = null;
		private Pattern[] includes = null;

		private bool MatchesAny(string s, Pattern[] ps) {
			bool m = false;
			foreach (Pattern p in ps) m |= p.Matches(s);
			return m;
		}
		private bool MatchesExcludes(string s) { return MatchesAny(s, excludes); }
		private bool MatchesIncludes(string s) { return MatchesAny(s, includes); }

		protected override void work() {
			if (JobSetup == null) throw new ArgumentNullException(nameof(JobSetup));
			if (Root == null) Root = new Scanned.RootDirectory() { Path = JobSetup.Directory };

			excludes = Array.ConvertAll(JobSetup.ExcludePattern.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries), (string s) => { return Pattern.FromString(s); });
			if (excludes == null) excludes = new Pattern[0];
			includes = Array.ConvertAll(JobSetup.IncludePattern.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries), (string s) => { return Pattern.FromString(s); });
			if (includes == null) includes = new Pattern[] { Pattern.FromString("*"), Pattern.FromString("*.*") };

			Dispatch(() => { Root.Children.Clear(); });

			DirectoryInfo di = new DirectoryInfo(Root.Path);
			if (!di.Exists) throw new ArgumentException("Directory must exist");
			Stack<Tuple<DirectoryInfo, Scanned.Directory>> stack = new Stack<Tuple<DirectoryInfo, Scanned.Directory>>();
			stack.Push(new Tuple<DirectoryInfo, Scanned.Directory>(di, Root));
			List<Tuple<FileInfo, Scanned.File>> files = new List<Tuple<FileInfo, Scanned.File>>();

			while (stack.Any()) {
				Tuple<DirectoryInfo, Scanned.Directory> dir = stack.Pop();
				foreach (DirectoryInfo subDir in dir.Item1.GetDirectories()) {
					if (MatchesExcludes(subDir.Name)) continue;
					Scanned.Directory sd = new Scanned.Directory() { Name = subDir.Name };
					stack.Push(new Tuple<DirectoryInfo, Scanned.Directory>(subDir, sd));
					Dispatch(() => { dir.Item2.Children.SortIn(sd); });
				}
				foreach (FileInfo file in dir.Item1.GetFiles()) {
					if (!MatchesIncludes(file.Name)) continue;
					if (MatchesExcludes(file.Name)) continue;

					Scanned.File f = new Scanned.File() { Name = file.Name };
					f.PropertyChanged += Root.File_PropertyChanged;
					files.Add(new Tuple<FileInfo, Scanned.File>(file, f));
					Dispatch(() => {
						dir.Item2.Children.SortIn(f);
						f.Selected = f.Encoding != null;
					});
				}
			}

			if (files.Any()) {
				int fileNum = 0;
				foreach (Tuple<FileInfo, Scanned.File> f in files) {
					Progress = (double)++fileNum / (double)files.Count;
					if (!f.Item1.Exists) {
						f.Item2.Enabled = false;
						f.Item2.Selected = false;
						continue;
					}
					f.Item2.Analyse(f.Item1.FullName);
				}

			}
		}

	}

}
