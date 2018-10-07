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

		private Encoding secondGuess(byte[] inData, Encoding enc) {
			try {
				string s = enc.GetString(inData);
				int good = 0;
				int bad = 0;
				foreach (char c in s) {
					bool g = false;
					switch (char.GetUnicodeCategory(c)) {
						case System.Globalization.UnicodeCategory.UppercaseLetter: g = true; break;
						case System.Globalization.UnicodeCategory.LowercaseLetter: g = true; break;
						case System.Globalization.UnicodeCategory.TitlecaseLetter: g = true; break;
						case System.Globalization.UnicodeCategory.ModifierLetter: g = true; break;
						case System.Globalization.UnicodeCategory.OtherLetter: g = true; break;
						case System.Globalization.UnicodeCategory.NonSpacingMark: break;
						case System.Globalization.UnicodeCategory.SpacingCombiningMark: break;
						case System.Globalization.UnicodeCategory.EnclosingMark: g = true; break;
						case System.Globalization.UnicodeCategory.DecimalDigitNumber: g = true; break;
						case System.Globalization.UnicodeCategory.LetterNumber: g = true; break;
						case System.Globalization.UnicodeCategory.OtherNumber: break;
						case System.Globalization.UnicodeCategory.SpaceSeparator: g = true; break;
						case System.Globalization.UnicodeCategory.LineSeparator: g = true; break;
						case System.Globalization.UnicodeCategory.ParagraphSeparator: g = true; break;
						case System.Globalization.UnicodeCategory.Control: g = (c == '\n') || (c == '\r'); break;
						case System.Globalization.UnicodeCategory.Format: break;
						case System.Globalization.UnicodeCategory.Surrogate: break;
						case System.Globalization.UnicodeCategory.PrivateUse: break;
						case System.Globalization.UnicodeCategory.ConnectorPunctuation: g = true; break;
						case System.Globalization.UnicodeCategory.DashPunctuation: g = true; break;
						case System.Globalization.UnicodeCategory.OpenPunctuation: g = true; break;
						case System.Globalization.UnicodeCategory.ClosePunctuation: g = true; break;
						case System.Globalization.UnicodeCategory.InitialQuotePunctuation: g = true; break;
						case System.Globalization.UnicodeCategory.FinalQuotePunctuation: g = true; break;
						case System.Globalization.UnicodeCategory.OtherPunctuation: g = true; break;
						case System.Globalization.UnicodeCategory.MathSymbol: g = true; break;
						case System.Globalization.UnicodeCategory.CurrencySymbol: g = true; break;
						case System.Globalization.UnicodeCategory.ModifierSymbol: g = true; break;
						case System.Globalization.UnicodeCategory.OtherSymbol: break;
						case System.Globalization.UnicodeCategory.OtherNotAssigned: break;
					}
					if (g) good++; else bad++;
				}

				double ratio = (double)bad / (double)(bad + good);
				if (ratio < 0.1) return enc;
			} catch {
			}
			return null;
		}

		private void fixFile(string file) {
			string[] nlWin = new string[] { "\r\n" };
			char[] nlOther = new char[] { '\r', '\n' };
			char[] ws = new char[] { ' ', '\t' };

			Encoding inEnc = Scanned.File.GuessEncoding(file);
			byte[] inData = System.IO.File.ReadAllBytes(file);

			// read lines from file
			if (inEnc == null) inEnc = secondGuess(inData, Encoding.UTF8);
			if (inEnc == null) inEnc = secondGuess(inData, Encoding.ASCII);
			if (inEnc == null) inEnc = secondGuess(inData, Encoding.Default);
			if (inEnc == null) throw new Exception("Failed to load data as text");

			string[] lines = null;
			{
				int dataStart = 0;
				byte[] BOM = inEnc.GetPreamble();
				if (BOM != null && BOM.Length > 0) {
					bool bomMatch = true;
					for (int i = 0; i < BOM.Length; ++i) {
						bomMatch &= BOM[i] == inData[i];
					}
					if (bomMatch) dataStart = BOM.Length;
				}

				List<string> ls = new List<string>();
				foreach (string l in inEnc.GetString(inData, dataStart, inData.Length - dataStart).Split(nlWin, StringSplitOptions.None)) {
					ls.AddRange(l.Split(nlOther));
				}
				lines = ls.ToArray();
			}

			// remove trailing
			if (Setup.RemoveTrailingWhitespace) {
				for (int i = 0; i < lines.Length; ++i) {
					lines[i] = lines[i].TrimEnd(ws);
				}
			}

			// fix indention
			if (Setup.LeadingWhitespace != LeadingWhitespace.Keep) {
				int cs = 0;
				int ct = 0;
				foreach (string l in lines) {
					if (string.IsNullOrEmpty(l)) continue;
					if (l[0] == ' ') cs++;
					if (l[0] == '\t') ct++;
				}
				bool tabify = (Setup.LeadingWhitespace == LeadingWhitespace.Tabs)
					|| (Setup.LeadingWhitespace == LeadingWhitespace.MajorityVoteTabs && ct >= cs)
					|| (Setup.LeadingWhitespace == LeadingWhitespace.MajorityVoteSpaces && ct > cs);

				int tabSize = Setup.TabSize;

				if (tabify) {
					// use tabs
					for (int i = 0; i < lines.Length; ++i) {
						if (string.IsNullOrEmpty(lines[i])) continue;
						int l = 0;
						while (l < lines[i].Length && lines[i][l] == ' ') ++l;
						if (l < tabSize) continue;
						l /= tabSize;
						lines[i] = new string('\t', l) + lines[i].Substring(l * tabSize);
					}

				} else {
					// use spaces
					for (int i = 0; i < lines.Length; ++i) {
						if (string.IsNullOrEmpty(lines[i])) continue;
						int l = 0;
						while (l < lines[i].Length && lines[i][l] == '\t') ++l;
						if (l <= 0) continue;
						lines[i] = new string(' ', l * tabSize) + lines[i].Substring(l);
					}

				}
			}

			// prep newlines
			string nl = "\n";
			switch (Setup.LineBreak) {
				case LineBreak.Mac: nl = "\r"; break;
				case LineBreak.Unix: nl = "\n"; break;
				case LineBreak.Windows: nl = "\r\n"; break;
			}
			string content = string.Join(nl, lines);

			// Encode
			byte[] data = Setup.Encoding.GetBytes(content);
			if (!Setup.WriteNoBOM) {
				byte[] bom = Setup.Encoding.GetPreamble();
				byte[] d = new byte[bom.Length + data.Length];
				bom.CopyTo(d, 0);
				data.CopyTo(d, bom.Length);
				data = d;
			}

			if (!data.SequenceEqual(inData)) {
				System.IO.File.WriteAllBytes(file, data);
			}
		}
	}
}
