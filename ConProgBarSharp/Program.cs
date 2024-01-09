namespace ConProgBarSharp
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello, World!");

			ConProgBar bar = new()
			{
				Text = "Demoing"// + " with a very long title to see if the caption truncation does work as intended"
			};

			bar.MaximumWidth = Console.WindowWidth - 1;
			bar.MinimumWidth = bar.MaximumWidth;

			bar.Show = true;

			const int maxI = 283;
			for (int i = 0; i < maxI; ++i)
			{
				bar.Value = (double)i / maxI;

				Thread.Sleep(50);

				if (i == 10)
				{
					bar.Show = false;
					Console.WriteLine("Intermission...");
					bar.Show = true;
				}

			}

			bar.Show = false;

			Console.WriteLine("Done.");
		}
	}
}
