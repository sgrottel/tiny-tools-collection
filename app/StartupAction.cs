using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleStarter
{
	public class StartupAction : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private bool isSelected = false;
		private string name = "";
		private string filename = "";
		private string[] argumentList = Array.Empty<string>();
		private string workingDirectory = "";
		private string verb = "";
		private bool useShellExecute;

		public bool IsSelected
		{
			get => isSelected;
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


		public string Filename
		{
			get => filename;
			set
			{
				if (filename != value)
				{
					filename = value;
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

	}
}
