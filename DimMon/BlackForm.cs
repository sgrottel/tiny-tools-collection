namespace DimMon
{
	public partial class BlackForm : Form
	{
		bool cursorShown = true;

		public BlackForm()
		{
			InitializeComponent();
			hideMouse.Tick += HideMouse_Tick;
		}

		private void HideMouse_Tick(object? sender, EventArgs e)
		{
			if (Bounds.Contains(Cursor.Position))
			{
				if (cursorShown)
				{
					Cursor.Hide();
					cursorShown = false;
				}
			}

			hideMouse.Stop();
		}

		private void BlackForm_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void BlackForm_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				Application.Exit();
			}
		}

		private System.Windows.Forms.Timer hideMouse = new System.Windows.Forms.Timer() { Interval = 1250, Enabled = false };

		private void BlackForm_MouseMove(object sender, MouseEventArgs e)
		{
			if (!cursorShown)
			{
				Cursor.Show();
				cursorShown = true;
			}
			hideMouse.Stop();
			hideMouse.Start();
		}

		private void BlackForm_MouseLeave(object sender, EventArgs e)
		{
			hideMouse.Stop();
		}
	}
}