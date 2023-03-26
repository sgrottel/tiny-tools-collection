using System.Globalization;

namespace DimMon
{
	internal static class Program
	{
		static List<BlackForm> forms = new();

		static Screen[]? selectedScreens = null;

		static double opacity = 0.9;

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
			Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

			try
			{
				ParseArgs(args);
			}
			catch { }

			foreach (var screen in selectedScreens ?? Screen.AllScreens)
			{
				AddForm(screen.Bounds, opacity);
			}

			Application.Run();

			forms.Clear();
		}

		private static void ParseArgs(string[] args)
		{
			int mode = 0;
			HashSet<Screen> selScreens = new();
			foreach (string a in args)
			{
				if (string.IsNullOrWhiteSpace(a)) continue;

				switch (mode)
				{
					case 0:
						if (string.Equals(a, "-opacity", StringComparison.CurrentCultureIgnoreCase))
						{
							mode = 1;
						}
						else if (a[0] == '@')
						{
							string[] seg = a.Substring(1).Split(';');
							if (seg.Length == 2)
							{
								Point p = new Point(int.Parse(seg[0]), int.Parse(seg[1]));
								foreach (var screen in Screen.AllScreens)
								{
									if (screen.Bounds.Contains(p))
									{
										selScreens.Add(screen);
										break;
									}
								}
							}
						}
						else
						{
							foreach (var screen in Screen.AllScreens)
							{
								if (screen.DeviceName.EndsWith(a, StringComparison.CurrentCultureIgnoreCase))
								{
									selScreens.Add(screen);
									break;
								}
							}
						}
						break;
					case 1:
						opacity = double.Parse(a, CultureInfo.InvariantCulture);
						mode = 0;
						break;
					default:
						throw new Exception("Invalid parsing mode");
				}
			}

			if (selScreens.Count > 0)
			{
				selectedScreens = selScreens.ToArray();
			}
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