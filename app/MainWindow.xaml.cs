﻿using LittleStarter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration.Internal;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
	public partial class MainWindow : Window, INotifyPropertyChanged
	{

		public ObservableCollection<StartupAction> Actions { get; } = new ObservableCollection<StartupAction>();

		public string Messages { get; private set; } = "";

		public event PropertyChangedEventHandler? PropertyChanged;

		private ConfigFileReader configFile;

		private bool loading = true;
		public bool IsLoading
		{
			get => loading;
			set
			{
				if (loading != value)
				{
					loading = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
				}
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			configFile = new ConfigFileReader(
				System.IO.Path.Combine(
					System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
					"LittleStarterConfig.yaml")
				);
			configFile.ClearConfig += ConfigFile_ClearConfig;
			configFile.FailedLoading += ConfigFile_FailedLoading;
			configFile.ActionsLoaded += ConfigFile_ActionsLoaded;

			InitialWindowPlacement();
			DataContext = this;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			configFile.Start();
		}

		private void ConfigFile_ActionsLoaded(ConfigFileReader _, StartupAction[] actions)
		{
			AddMessage("Actions loaded.");
			Dispatcher.Invoke(() =>
			{
				Actions.Clear();
				foreach (StartupAction sa in actions)
				{
					if (sa.IconUri != null)
					{
						try
						{
							sa.Icon = new BitmapImage(sa.IconUri);
						}
						catch { }
					}

					Actions.Add(sa);
				}
				IsLoading = false;
			});
		}

		private void ConfigFile_ClearConfig(ConfigFileReader _)
		{
			AddMessage("No configuration loaded");
			Dispatcher.Invoke(() =>
			{
				Actions.Clear();
				IsLoading = false;
			});
		}

		private void ConfigFile_FailedLoading(ConfigFileReader _, string errorMsg)
		{
			AddMessage(string.Format("Failed loading configuration: {0}", errorMsg));
			Dispatcher.Invoke(() =>
			{
				Actions.Clear();
				IsLoading = false;
			});
		}

		private void InitialWindowPlacement()
		{
			Width = SystemParameters.WorkArea.Width / 3;
			Left = SystemParameters.WorkArea.Left + (SystemParameters.WorkArea.Width - Width) / 2;
			Height = SystemParameters.WorkArea.Height / 2;
			Top = SystemParameters.WorkArea.Top + 2 * (SystemParameters.WorkArea.Height - Height) / 5;
		}

		internal void AddMessage(string msg)
		{
			Dispatcher.Invoke(() =>
			{
				if (!string.IsNullOrEmpty(Messages)) Messages += "\n";
				Messages += msg;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Messages)));
			});
		}

		private void ButtonAction_Click(object sender, RoutedEventArgs? e)
		{
			try
			{
				Parallel.ForEach(Actions, (StartupAction a) =>
				{
					if (!a.IsEnabled) return;
					if (!a.IsSelected) return;

					ProcessStartInfo psi = new ProcessStartInfo();
					psi.FileName = a.Filename;
					psi.WorkingDirectory = a.WorkingDirectory;
					foreach (string aa in a.ArgumentList) psi.ArgumentList.Add(aa);
					psi.Verb = a.Verb;
					psi.UseShellExecute = a.UseShellExecute;

					Process.Start(psi);
				});

				Close();
			}
			catch(Exception ex)
			{
				AddMessage(string.Format("Failed: {0}", ex.ToString()));
			}

		}

		private void ButtonSelectAll_Click(object sender, RoutedEventArgs? e)
		{
			foreach (StartupAction a in Actions)
			{
				a.IsSelected = a.IsEnabled;
			}
		}

		private void ButtonSelectNone_Click(object sender, RoutedEventArgs? e)
		{
			foreach (StartupAction a in Actions)
			{
				a.IsSelected = false;
			}
		}

		private void ButtonRefresh_Click(object sender, RoutedEventArgs? e)
		{
			configFile.Reload();
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.A:
					ButtonSelectAll_Click(sender, null);
					e.Handled = true;
					break;
				case Key.N:
					ButtonSelectNone_Click(sender, null);
					e.Handled = true;
					break;
				case Key.Escape:
					Close();
					e.Handled = true;
					break;
				case Key.F5:
					ButtonRefresh_Click(sender, null);
					e.Handled = true;
					break;
				case Key.Enter:
					if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
					{
						ButtonAction_Click(sender, null);
						e.Handled = true;
					}
					break;
			}
		}
	}
}
