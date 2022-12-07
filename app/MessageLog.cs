using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LittleStarter
{

	internal class MessageLog
	{
		private List<string> msgs = new();
		private object sync = new();
		private CancellationTokenSource? cancelTokenSource = null;

		/// <summary>
		/// The file system path to the directory in which the log file will be saved
		/// </summary>
		public string SavePath { get; set; }

		public void Add(string msg)
		{
			lock (sync)
			{
				msgs.Add(msg);

				// debounce the callback to not waste too many threads
				cancelTokenSource?.Cancel();
				cancelTokenSource = new CancellationTokenSource();
				Task.Delay(100, cancelTokenSource.Token)
					.ContinueWith(t =>
					{
						if (t.IsCompletedSuccessfully)
						{
							Updated?.Invoke(this, new EventArgs());
						}
					}, TaskScheduler.Default);
			}
		}

		public override string ToString()
		{
			lock (sync)
			{
				return string.Join("\n", msgs);
			}
		}

		public event EventHandler? Updated;

		public void Add(string format, object? arg0)
		{
			Add(string.Format(format, arg0));
		}

		public void Add(string format, object? arg0, object? arg1)
		{
			Add(string.Format(format, arg0, arg1));
		}

		public void Add(string format, object? arg0, object? arg1, object? arg2)
		{
			Add(string.Format(format, arg0, arg1, arg2));
		}

		public void Add(string format, params object?[] args)
		{
			Add(string.Format(format, args));
		}

		public void Save()
		{
			lock (sync)
			{
				if (!Directory.Exists(SavePath)) return;

				const int maxLogFiles = 10 - 1;
				string[] files = Directory.GetFiles(SavePath);
				if (files.Length > maxLogFiles)
				{
					Array.Reverse(files);
					DateTime[] writeTimes = new DateTime[files.Length];
					for (int i = 0; i < files.Length; ++i)
					{
						writeTimes[i] = File.GetLastWriteTimeUtc(files[i]);
					}
					Array.Sort(writeTimes, files);

					for (int i = 0; i < files.Length - maxLogFiles; ++i)
					{
						try
						{
							File.Delete(files[i]);
						}
						catch (Exception e)
						{
							Add("Failed to delete old log file \"{0}\": {1}", files[i], e);
						}
					}
				}

				string filename = string.Format("LittleStarter_{0}.log", DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture));

				using (StreamWriter writer = new StreamWriter(Path.Combine(SavePath, filename), false, new UTF8Encoding(false)))
				{
					foreach (string msg in msgs)
					{
						writer.WriteLine(msg);
					}
					writer.Close();
				}
			}
		}

	}

}
