
namespace ConProgBarSharp
{

	internal class ConProgBar
	{

		private bool show = false;

		public bool Show
		{
			get => show;
			set
			{
				if (show != value)
				{
					show = value;
					if (show)
					{
						Update();
					}
					else
					{
						Clear();
					}
					SetOrResetEtaTimer();
				}
			}
		}

		private string text = string.Empty;
		public string Text
		{
			get => text;
			set
			{
				if (text != value)
				{
					text = value;
					if (show)
					{
						Update();
					}
				}
			}
		}

		private int minWidth = 40;
		public int MinimumWidth
		{
			get => minWidth;
			set
			{
				if (minWidth != value)
				{
					minWidth = value;
					if (show)
					{
						Update();
					}
				}
			}
		}

		private int maxWidth = 80;
		public int MaximumWidth
		{
			get => maxWidth;
			set
			{
				if (maxWidth != value)
				{
					maxWidth = value;
					if (show)
					{
						Update();
					}
				}
			}
		}

		private double progressValue = 0.0;
		public double Value
		{
			get => progressValue;
			set
			{
				double v = Math.Clamp(value, 0.0, 1.0);
				if (progressValue != v)
				{
					progressValue = v;
					eta.Push(v);
					if (show)
					{
						Update();
					}
				}
			}
		}

		private bool showEta = false;
		public bool ShowEta
		{
			get => showEta;
			set
			{
				if (showEta != value)
				{
					showEta = value;
					if (show)
					{
						Update();
						SetOrResetEtaTimer();
					}
				}
			}
		}

		private Timer? updateOnEta;
		private void SetOrResetEtaTimer()
		{
			if (show && showEta)
			{
				if (updateOnEta == null)
				{
					updateOnEta = new Timer(Update, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
				}
			}
			else
			{
				if (updateOnEta != null)
				{
					updateOnEta.Change(Timeout.Infinite, Timeout.Infinite);
					updateOnEta.Dispose();
					updateOnEta = null;
				}
			}
		}

		/// <summary>
		/// Very simple estimator.
		/// Assumes linear progression without hickups.
		/// </summary>
		private class EtaEstimator
		{
			private DateTime start = DateTime.Now;
			private DateTime end = DateTime.Now;

			internal void Push(double v)
			{
				v = Math.Clamp(v, 0.0, 1.0);

				if (v <= 0.0001)
				{
					start = end = DateTime.Now;
				}
				else if (v <= 0.01)
				{
					// don't update 'start'
					end = start; // reset eta to zero to hide it
				}
				else
				{
					TimeSpan dur = DateTime.Now - start;
					dur *= 1.0 / v;
					end = start + dur;
				}
			}

			internal TimeSpan Value
			{
				get
				{
					TimeSpan d = end - DateTime.Now;
					if (d <= TimeSpan.Zero)
					{
						return TimeSpan.Zero;
					}
					return d;
				}
			}

		};

		private int lastLineLen = 0;
		private string lastText = string.Empty;
		private EtaEstimator eta = new();

		private void Clear()
		{
			if (lastLineLen <= 0) return;
			Console.Write("\r" + new string(' ', lastLineLen) + "\r");
			lastLineLen = 0;
			lastText = string.Empty;
		}

		private void Update(object? _ = null)
		{
			string p = $"{progressValue:P1}";

			if (showEta)
			{
				int etaSec = (int)eta.Value.TotalSeconds;
				if (etaSec > 60 * 60)
				{
					p += $" - {etaSec / 60 * 60}:{(etaSec / 60) % 60:00}h";
				}
				else
				if (etaSec > 60)
				{
					p += $" - {etaSec / 60}:{etaSec % 60:00}";
				}
				else
				if (etaSec > 5)
				{
					p += $" - {etaSec}s";
				}
				else
				if (etaSec > 1)
				{
					p += " - <5s";
				}
			}

			string t = text;
			if (3 + p.Length + t.Length > maxWidth)
			{
				t = t.Substring(0, maxWidth - p.Length - 5) + "...";
			}
			t += $" {p}";

			if (lastText == t) return; // no update

			lastText = t;

			if (t.Length + 2 < minWidth)
			{
				t += new string(' ', minWidth - t.Length - 2);
			}
			int lineLen = t.Length;

			// format an color magic with: virtual terminal sequences
			// https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
			int progPos = Math.Clamp((int)(0.5 + t.Length * progressValue), 0, t.Length);
			t = t.Insert(progPos, "\u001b[40m");
			t = $"\r[\u001b[37m\u001b[44m{t}\u001b[39m\u001b[49m]";

			if (lineLen + 2 < lastLineLen)
			{
				t += new string(' ', lastLineLen - lineLen - 2);
			}
			lastLineLen = lineLen + 2;

			Console.Write(t);
		}

	}

}
