using System;
using System.Collections.Generic;
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
				// TODO: Implement
				//throw new NotImplementedException();
			}
		}

	}

}
