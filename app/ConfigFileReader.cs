using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Diagnostics.Contracts;
using System.Windows.Annotations;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LittleStarter
{
	internal class ConfigFileReader
	{
		private string filename;
		private Timer timer;
		private DateTime lastWriteTime;
		private string lastError = string.Empty;

		private bool oneTimerExecution = true;
		private object oneTimerExecutionLock = new();

		public delegate void ClearConfigEventHandler(ConfigFileReader sender);
		public delegate void ActionsLoadedEventHandler(ConfigFileReader sender, StartupAction[] startupActions);
		public delegate void FailedLoadingEventHandler(ConfigFileReader sender, string errorMessage);
		public delegate void LogConfigLoadedEventHandler(ConfigFileReader sender, string logsPath);

		public event ClearConfigEventHandler? ClearConfig;
		public event ActionsLoadedEventHandler? ActionsLoaded;
		public event FailedLoadingEventHandler? FailedLoading;
		public event LogConfigLoadedEventHandler? LogConfigLoaded;

		public string Filename
		{
			get { return filename; }
		}

		public ConfigFileReader(string filename)
		{
			this.filename = filename;

			lastWriteTime = DateTime.MinValue + TimeSpan.FromDays(1);

			timer = new Timer(checkFile, null, Timeout.Infinite, 1000);
		}

		public void Start()
		{
			timer.Change(0, 1000);
		}

		private class ActionConfig
		{
			public string? Name { get; set; }
			public bool? IsSelectedByDefault { get; set; }
			public string? Filename { get; set; }
			public List<string>? ArgumentList { get; set; }
			public string? WorkingDirectory { get; set; }
			public string? Verb { get; set; }
			public bool? UseShellExecute { get; set; }
			public bool? IsEnabled { get; set; }
			public string? Icon { get; set; }
		}

		private class LogConfig
		{
			public string? Path { get; set; }
		}

		private class Config
		{
			public string? Version { get; set; }
			public List<ActionConfig>? Actions { get; set; }
			public LogConfig? Log { get; set;}
		}

		private void checkFile(object? state)
		{
			lock (oneTimerExecutionLock)
			{
				if (!oneTimerExecution) return;
				oneTimerExecution = false;
			}
			try
			{
				if (!System.IO.File.Exists(filename))
				{
					DateTime z = DateTime.MinValue;
					if (lastWriteTime != z)
					{
						lastWriteTime = z;
						ClearConfig?.Invoke(this);
					}
					return;
				}
				DateTime lwt = System.IO.File.GetLastWriteTime(filename);
				if (lastWriteTime == lwt) return;
				lastWriteTime = lwt;

				using (var input = new StreamReader(filename))
				{
					var deserializer = new DeserializerBuilder()
						.WithNamingConvention(CamelCaseNamingConvention.Instance)
						.IgnoreUnmatchedProperties()
						.Build();
					var config = deserializer.Deserialize<Config>(input);
					if (config == null)
					{
						throw new ArgumentException("Config data seems empty");
					}

					Version? ver;
					if (!Version.TryParse(config.Version, out ver))
					{
						throw new ArgumentException("Version format error");
					}
					if (ver == null || ver != new Version(1, 0))
					{
						throw new ArgumentException("Unsupported version");
					}

					config.Actions?.RemoveAll((ActionConfig ac) => ac == null);

					StartupAction[] actions = config.Actions?.ConvertAll((ActionConfig ac) =>
					{

						StartupAction sa = new StartupAction();

						sa.Name = ac.Name ?? "Unnamed";

						if (ac.Filename != null)
						{
							sa.Filename = ac.Filename;
						}

						if (ac.IsSelectedByDefault != null)
						{
							sa.IsSelected = ac.IsSelectedByDefault.Value;
						}

						sa.ArgumentList = (ac.ArgumentList?.ToArray()) ?? Array.Empty<string>();

						if (ac.WorkingDirectory != null)
						{
							sa.WorkingDirectory = ac.WorkingDirectory;
						}

						if (ac.Verb != null)
						{
							sa.Verb = ac.Verb;
						}

						if (ac.UseShellExecute != null)
						{
							sa.UseShellExecute = ac.UseShellExecute.Value;
						}

						if (ac.IsEnabled != null)
						{
							sa.IsEnabled = ac.IsEnabled.Value;
						}

						if (ac.Icon != null)
						{
							if (File.Exists(ac.Icon))
							{
								try
								{
									Dispatcher.CurrentDispatcher.Invoke(() =>
									{
										sa.IconUri = new Uri(ac.Icon);
									});
								}
								catch { }
							}
						}

						return sa;
					}).ToArray() ?? Array.Empty<StartupAction>();

					ActionsLoaded?.Invoke(this, actions);

					if (config.Log != null)
					{
						if (!string.IsNullOrEmpty(config.Log.Path))
						{
							config.Log.Path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty, config.Log.Path);
							LogConfigLoaded?.Invoke(this, config.Log.Path);
						}
					}

				}

			}
			catch (Exception ex)
			{
				string e = ex.ToString();
				if (ex.InnerException != null)
				{
					e += " > " + ex.InnerException.ToString();
				}
				if (!lastError.Equals(e))
				{
					lastError = e;
					FailedLoading?.Invoke(this, e);
				}
			}
			finally
			{
				lock (oneTimerExecutionLock)
				{
					oneTimerExecution = true;
				}
			}
		}

		internal void Reload()
		{
			lastWriteTime = DateTime.MinValue + TimeSpan.FromDays(1);
		}
	}
}
