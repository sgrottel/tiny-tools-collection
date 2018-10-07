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
using System.Windows.Shell;

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

		private IJob job = null;
		public IJob Job {
			get {
				return job;
			}
			private set {
				if (value != null) {
					if (job != null) {
						if (job == value) return;
						throw new InvalidOperationException();
					}
					value.Done += (object s, EventArgs a) => { if (Job == s) Job = null; };
					value.PropertyChanged += (object s, PropertyChangedEventArgs a) => {
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JobProgress)));
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JobProgressPercent)));
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JobIndeterminate)));
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JobProgressState)));
					};

					IsUiInteractive = false;
					job = value;
					job.Start();

				} else {
					if (job == null) return;
					if (job.Running) throw new InvalidOperationException();
					IsUiInteractive = true;
					job = null;

				}
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Job)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JobProgress)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JobProgressPercent)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JobIndeterminate)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JobProgressState)));
			}
		}
		public int JobProgress { get { return (int)(1000.0 * JobProgressPercent); } }
		public double JobProgressPercent { get { return ((job != null) ? Math.Min(1.0, Math.Max(0.0, job.Progress)) : 0.0); } }
		public bool JobIndeterminate { get { return (job != null) ? job.Progress < 0.0 : false; } }
		public TaskbarItemProgressState JobProgressState {
			get {
				if (job == null) return TaskbarItemProgressState.None;
				if (job.Progress < 0.0) return TaskbarItemProgressState.Indeterminate;
				return TaskbarItemProgressState.Normal;
			}
		}

		public MainWindow() {
			InitializeComponent();
			if (string.IsNullOrEmpty(Properties.Settings.Default.Directory)) {
				Properties.Settings.Default.Upgrade();
				Properties.Settings.Default.Save();
			}

			var lineBreakStyles = typeof(LineBreak).GetEnumValues();
			Array.Sort(lineBreakStyles, new LineBreakComparer());
			lineEndingsComboBox.ItemsSource = lineBreakStyles;

			encodingComboBox.ItemsSource = Encoding.GetEncodings().Select(e => e.GetEncoding()).ToArray();
			encodingComboBox.Items.SortDescriptions.Add(new SortDescription("EncodingName", ListSortDirection.Ascending));

			JobSetup.LoadFrom(Properties.Settings.Default);
		}

		public event PropertyChangedEventHandler PropertyChanged;

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

		private void AbortJobButton_Click(object sender, RoutedEventArgs e) {
			Job?.Abort();
		}

		private void window_Closed(object sender, EventArgs e) {
			try {
				JobSetup?.SaveTo(Properties.Settings.Default);
				Properties.Settings.Default.Save();
			} catch { }
		}

		private Scanned.RootDirectory root = null;
		internal Scanned.RootDirectory RootDir {
			get { return root; }
			set {
				if (root != value) {
					if (root != null) {
						root.PropertyChanged -= Root_PropertyChanged;
					}
					root = value;
					if (root != null) {
						root.PropertyChanged += Root_PropertyChanged;
						if (JobSetup != null) JobSetup.SelectedFiles = root.SelectedFiles;
					}
					filesTreeView.ItemsSource = new Scanned.RootDirectory[] { root };
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RootDir)));
				}
			}
		}

		private void Root_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (string.Equals(e?.PropertyName, nameof(Scanned.RootDirectory.SelectedFiles))) {
				if (JobSetup != null) JobSetup.SelectedFiles = RootDir.SelectedFiles;
			}
		}

		private void ScanDirectoryButton_Click(object sender, RoutedEventArgs e) {
			CollectFilesJob j = new CollectFilesJob();
			j.JobSetup = JobSetup?.Clone();
			RootDir = j.Root = new Scanned.RootDirectory() { Path = JobSetup?.Directory };
			Job = j;
		}

		private void FixFilesButton_Click(object sender, RoutedEventArgs e) {
			FixFilesJob j = new FixFilesJob();
			j.Setup = JobSetup?.Clone();
			j.Files = RootDir?.GetSelectedFiles();
			j.Done += (object o, EventArgs ea) => { RootDir?.RescanFiles(); };
			Job = j;
		}

		private void SelectNoFilesButton_Click(object sender, RoutedEventArgs e) {
			RootDir?.AllFiles(false);
		}
		private void SelectAllFilesButton_Click(object sender, RoutedEventArgs e) {
			RootDir?.AllFiles(true);
		}
	}
}
