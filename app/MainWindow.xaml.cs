using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace app
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			InitialWindowPlacement();
		}

		private void InitialWindowPlacement()
		{
			Width = SystemParameters.WorkArea.Width / 3;
			Left = SystemParameters.WorkArea.Left + (SystemParameters.WorkArea.Width - Width) / 2;
			Height = SystemParameters.WorkArea.Height / 2;
			Top = SystemParameters.WorkArea.Top + (SystemParameters.WorkArea.Height - Height) / 2;
		}

	}
}
