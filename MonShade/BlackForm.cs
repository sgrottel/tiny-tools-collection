namespace MonShade
{
	public partial class BlackForm : Form
	{
		public BlackForm()
		{
			InitializeComponent();
			// TODO: less visible mouse cursor
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
	}
}