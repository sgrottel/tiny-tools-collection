using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace scfeu {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged {

		/// <summary>
		/// Flag to control `IsEnabled` of almost all UI
		/// </summary>
		private bool isUiInteractive = true;
		public bool IsUiInteractive {
			get { return isUiInteractive; }
			set {
				if (isUiInteractive != value) {
					isUiInteractive = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUiInteractive)));
				}
			}
		}

		public MainWindow() {
			InitializeComponent();

			progress.Value = 10.0;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void ScanDirectoryButton_Click(object sender, RoutedEventArgs e) {
			IsUiInteractive = !IsUiInteractive;
		}
	}
}
