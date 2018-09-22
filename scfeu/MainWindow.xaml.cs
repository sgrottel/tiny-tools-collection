using Microsoft.WindowsAPICodePack.Dialogs;
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

		public JobSetup JobSetup {
			get; set;
		} = new JobSetup();

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
					JobSetup.IsEnabled = value;
				}
			}
		}

		public MainWindow() {
			InitializeComponent();
			var lineBreakStyles = typeof(LineBreak).GetEnumValues();
			Array.Sort(lineBreakStyles, new LineBreakComparer());
			lineEndingsComboBox.ItemsSource = lineBreakStyles;
			encodingComboBox.ItemsSource = Encoding.GetEncodings().Select(e => e.GetEncoding()).ToArray();
			encodingComboBox.Items.SortDescriptions.Add(new SortDescription("EncodingName", ListSortDirection.Ascending));

			//progressBar.Value = 10.0;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void ScanDirectoryButton_Click(object sender, RoutedEventArgs e) {
		}

		private void window_DragOver(object sender, DragEventArgs e) {
			e.Effects = DragDropEffects.None;
			try {
				if (e.Data.GetDataPresent("FileNameW")) {
					string path = ((string[])e.Data.GetData("FileNameW"))[0];
					if (System.IO.Directory.Exists(path) && IsUiInteractive) {
						e.Effects = DragDropEffects.Copy;
					}
				}
			} catch {
			}
			e.Handled = true;
		}

		private void window_Drop(object sender, DragEventArgs e) {
			try {
				if (e.Data.GetDataPresent("FileNameW")) {
					string path = ((string[])e.Data.GetData("FileNameW"))[0];
					if (System.IO.Directory.Exists(path) && IsUiInteractive) {
						JobSetup.Directory = path;
						e.Handled = true;
						ScanDirectoryButton_Click(this, null);
					}
				}
			} catch {
			}
		}

		private void JobIncludePatternDefaultButton_Click(object sender, RoutedEventArgs e) {
			JobSetup.IncludePattern = JobSetup.DefaultIncludePattern;
		}

		private void JobExcludePatternDefaultButton_Click(object sender, RoutedEventArgs e) {
			JobSetup.ExcludePattern = JobSetup.DefaultExcludePattern;
		}

		private void BrowseDirButton_Click(object sender, RoutedEventArgs e) {
			var dialog = new CommonOpenFileDialog();
			dialog.IsFolderPicker = true;
			dialog.InitialDirectory = JobSetup.Directory;
			CommonFileDialogResult result = dialog.ShowDialog();
			if (result == CommonFileDialogResult.Ok) {
				if (System.IO.Directory.Exists(dialog.FileName)) {
					JobSetup.Directory = dialog.FileName;
				}
			}
		}

		private System.IO.DirectoryInfo autoCompleteDir = null;
		public IEnumerable<String> AutoCompleteFileSystemFolders {
			get {
				List<String> dirz = new List<string>();

				foreach (var d in System.IO.DriveInfo.GetDrives()) {
					dirz.Add(d.RootDirectory.ToString());
				}

				if (autoCompleteDir != null) {
					var dir = autoCompleteDir;
					while (dir != null) {
						foreach (var d in dir.GetDirectories()) {
							if (d.Attributes.HasFlag(System.IO.FileAttributes.Hidden)) continue;
							if (d.Attributes.HasFlag(System.IO.FileAttributes.System)) continue;
							dirz.Add(d.FullName);
						}
						dir = dir.Parent;
					}
				}

				return dirz;
			}
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
			string path = ((TextBox)e.Source).Text;
			if (path.EndsWith("\\")) {
				if (System.IO.Directory.Exists(path)) {
					autoCompleteDir = new System.IO.DirectoryInfo(path);
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoCompleteFileSystemFolders)));
				}
			}
		}
	}
}
