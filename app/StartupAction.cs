using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LittleStarter
{
	public class StartupAction : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private bool isSelected = false;
		private string name = "";
		private bool isEnabled = true;
		private Uri? iconUri = null;
		private ImageSource? icon = null;
		private string filename = "";
		private string[] argumentList = Array.Empty<string>();
		private string workingDirectory = "";
		private string verb = "";
		private bool useShellExecute;
		private TimeSpan delay = TimeSpan.Zero;

		public bool IsSelected
		{
			get
			{
				return isSelected && isEnabled;
			}
			set
			{
				if (isSelected != value)
				{
					isSelected = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
				}
			}
		}

		public string Name
		{
			get => name;
			set
			{
				if (name != value)
				{
					name = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
				}
			}
		}

		public bool IsEnabled
		{
			get => isEnabled;
			set
			{
				if (isEnabled != value)
				{
					isEnabled = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
				}
			}
		}

		public Uri? IconUri
		{
			get => iconUri; set
			{
				if (iconUri != value)
				{
					iconUri = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconUri)));
				}
			}
		}

		public ImageSource? Icon
		{
			get => icon;
			set
			{
				if (icon != value)
				{
					icon = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
				}
			}
		}

		public string Filename
		{
			get => filename;
			set
			{
				if (filename != value)
				{
					filename = value;

					if (System.IO.Path.IsPathFullyQualified(filename))
					{
						string? root = System.IO.Path.GetPathRoot(filename);
						if (root != null)
						{
							bool exists = System.IO.Directory.Exists(root);
							if (!exists)
							{
								IsEnabled = false;
							}
						}
					}

					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Filename)));
				}
			}
		}

		public string[] ArgumentList
		{
			get => argumentList;
			set
			{
				if (argumentList != value)
				{
					argumentList = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ArgumentList)));
				}
			}
		}

		public string WorkingDirectory
		{
			get => workingDirectory;
			set
			{
				if (workingDirectory != value)
				{
					workingDirectory = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WorkingDirectory)));
				}
			}
		}
		public string Verb
		{
			get => verb;
			set
			{
				if (verb != value)
				{
					verb = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Verb)));
				}
			}
		}
		public bool UseShellExecute
		{
			get => useShellExecute;
			set
			{
				if (useShellExecute != value)
				{
					useShellExecute = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseShellExecute)));
				}
			}
		}
		public TimeSpan Delay
		{
			get => delay;
			set
			{
				if (delay != value)
				{
					delay = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Delay)));
				}
			}
		}
	}
}
