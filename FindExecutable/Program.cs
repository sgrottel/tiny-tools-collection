//
// Run Docker/Linux test with:
//
// docker build -t sgr/findexecutable .
// docker run --rm sgr/findexecutable
//

namespace FindExecutable
{
	internal class Program
	{
		static void Main(string[] args)
		{
			string[] executables = new string[] {
				"git.exe",
				"git"
			};

			foreach (string executable in executables)
			{
				try
				{
					Console.WriteLine($"Searching for: {executable}");

					string? fullPath = FindExecutable.FullPath(executable);

					if (fullPath != null)
					{
						if (Path.IsPathFullyQualified(fullPath))
						{
							Console.WriteLine($"FOUND: {fullPath}");
						}
						else
						{
							Console.WriteLine($"FOUND w/ WARNING: non-full path, {fullPath}");
						}
					}
					else
					{
						Console.WriteLine("NOT FOUND");
					}
				}
				catch (Exception e)
				{
					Console.Error.WriteLine($"FAILED, Exception: {e}");
				}
			}

			Console.WriteLine("done.");
		}
	}
}
