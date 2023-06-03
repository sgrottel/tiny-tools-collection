using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FolderSummary.Summary;

namespace FolderSummary
{
	internal class Comparer
	{
		private class ReportStack
		{
			public string Name { get; set; } = string.Empty;
			public int Indent { get; set; } = 0;
			public ReportStack? Parent { get; set; } = null;
			public bool Printed { get; set; } = false;

			public ReportStack(string name)
			{
				Name = name;
			}

			public ReportStack(ReportStack parent, string name)
			{
				Parent = parent;
				Indent = parent.Indent + 1;
				Name = name;
			}

			internal void Print(int red = 2)
			{
				Parent?.Print(red - 1);
				if (Printed)
				{
					return;
				}
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = (red > 0) ? ConsoleColor.Red : ConsoleColor.Gray;
				if (Indent > 0)
				{
					Console.Write(new string(' ', 2 * Indent));
				}
				Console.WriteLine(Name);
				Console.ResetColor();
				Printed = true;
			}
		}
		private ReportStack root;

		public Comparer(FileInfo json, DirectoryInfo dir)
		{
			root = new($"Compare '{json.FullName}' to '{dir.FullName}'");
		}

		public bool IgnoreDate { get; set; } = false;

		private bool CompareDictionary<T>(
			Dictionary<string, T>? left,
			Dictionary<string, T>? right,
			Func<string, T?, T?, bool> compObj) where T : class
		{
			bool retVal = true;

			if (left == null)
			{
				if (right == null) return true;
				foreach (KeyValuePair<string, T> r in right)
				{
					if (!compObj(r.Key, null, r.Value))
					{
						retVal = false;
					}
				}
				return retVal;
			}
			if (right == null)
			{
				foreach (KeyValuePair<string, T> l in left)
				{
					if (!compObj(l.Key, l.Value, null))
					{
						retVal = false;
					}
				}
				return retVal;
			}

			Dictionary<string, T> rCopy = new(right);
			foreach (KeyValuePair<string, T> l in left)
			{
				T? rVal;
				if (rCopy.TryGetValue(l.Key, out rVal))
				{
					rCopy.Remove(l.Key);
				}
				else
				{
					rVal = null;
				}
				if (!compObj(l.Key, l.Value, rVal))
				{
					retVal = false;
				}
			}
			foreach (KeyValuePair<string, T> r in rCopy)
			{
				if (!compObj(r.Key, null, r.Value))
				{
					retVal = false;
				}
			}

			return retVal;
		}

		private bool CompareImpl(ReportStack rep, DirectoryData expected, DirectoryData found)
		{
			bool dirComp = CompareDictionary(
				expected.Directories,
				found.Directories,
				delegate(string name, DirectoryData? exp, DirectoryData? found)
				{
					if (exp == null && found == null) return true;
					ReportStack r = new(rep, name);
					if (exp == null)
					{
						new ReportStack(r, "> Extra directory").Print();
						return false;
					}
					if (found == null)
					{
						new ReportStack(r, "> Missing directory").Print();
						return false;
					}

					return CompareImpl(r, exp, found);
				});

			bool fileComp = CompareDictionary(
				expected.Files,
				found.Files,
				(string name, FileData? exp, FileData? found) =>
				{
					if (exp == null && found == null) return true;
					if (exp == null)
					{
						new ReportStack(new(rep, name), "> Extra file").Print();
						return false;
					}
					if (found == null)
					{
						new ReportStack(new(rep, name), "> Missing file").Print();
						return false;
					}

					if (exp.Size != found.Size)
					{
						new ReportStack(new(rep, name), $"> Size mismatch ({exp.Size} expected, {found.Size} found)").Print();
						return false;
					}

					if (!IgnoreDate)
					{
						if (exp.Date != found.Date)
						{
							new ReportStack(new(rep, name), $"> Date mismatch ({exp.Date} expected, {found.Date} found)").Print();
							return false;
						}
					}

					return true;
				});

			return dirComp && fileComp;
		}

		public void Compare(DirectoryData expected, DirectoryData found)
		{
			bool identical = CompareImpl(root, expected, found);

			root.Print();
			Console.BackgroundColor = ConsoleColor.Black;
			if (identical)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Identical");
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Different");
			}
			Console.ResetColor();
		}
	}
}
