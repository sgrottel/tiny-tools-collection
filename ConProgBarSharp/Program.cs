namespace ConProgBarSharp
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello, World!");

			ConProgBar bar = new()
			{
				ShowEta = true,
				Text = "Demoing"// + " with a very long title to see if the caption truncation does work as intended"
			};

			bar.MaximumWidth = Console.WindowWidth - 1;
			bar.MinimumWidth = bar.MaximumWidth;

			bar.Show = true;

			const int maxI = 18;
			for (int i = 0; i < maxI; ++i)
			{
				bar.Value = (double)i / maxI;

				Thread.Sleep(5000);
				//if (i % 5 == 1)
				//{
				//	Thread.Sleep(TimeSpan.FromSeconds(2));
				//}

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
