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

			TaskbarProgress tbProg = new NullTaskbarProgress();
			if (OperatingSystem.IsWindows())
			{
				tbProg = new WinTaskbarProgress() { Handle = WinTaskbarProgress.FindConsoleWindowHandle() };
			}

			bar.MaximumWidth = Console.WindowWidth - 1;
			bar.MinimumWidth = bar.MaximumWidth;

			tbProg.SetState(TaskbarProgress.State.Indeterminate);
			bar.Show = true;

			const int maxI = 18;
			for (int i = 0; i < maxI; ++i)
			{
				bar.Value = (double)i / maxI;
				tbProg.SetValue((ulong)i, maxI);

				Thread.Sleep(500);
				//if (i % 5 == 1)
				//{
				//	Thread.Sleep(TimeSpan.FromSeconds(2));
				//}

				if (i == 10)
				{
					bar.Show = false;
					Console.WriteLine("Intermission...");
					tbProg.SetState(TaskbarProgress.State.Paused); // pause = yellow/warning
					Thread.Sleep(2000);
					tbProg.SetState(TaskbarProgress.State.Normal);

					bar.Show = true;
				}

			}

			bar.Show = false;
			tbProg.SetState(TaskbarProgress.State.None);

			Console.WriteLine("Done.");
		}
	}
}
