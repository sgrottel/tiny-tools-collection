namespace MonShade
{
	internal static class Program
	{
		static List<BlackForm> forms = new();

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
			Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

			foreach (var screen in Screen.AllScreens)
			{
				// TODO: configure via cmd line
				AddForm(screen.Bounds, 0.9);
			}

			Application.Run();

			forms.Clear();
		}

		static void AddForm(Rectangle rect, double opacity)
		{
			var form = new BlackForm();
			forms.Add(form);

			form.Left = 0;
			form.Top = 0;
			form.Width = 100;
			form.Height = 60;

			form.Opacity = opacity;

			form.Show();

			// final placement seems to only work AFTER initial show
			form.Left = rect.Left;
			form.Top = rect.Top;
			form.Size = rect.Size;
		}
	}
}