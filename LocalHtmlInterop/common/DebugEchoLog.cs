using SGrottel;
using System.Diagnostics;

namespace LocalHtmlInterop
{
#if DEBUG
	public sealed class DebugEchoLog<T> : SGrottel.ISimpleLog where T : SimpleLog, new()
	{
		private T baseLog;
		public DebugEchoLog()
		{
			baseLog = new T();
		}
		public DebugEchoLog(string directory, string name, int retention)
		{ 
			baseLog = new T();
		}
		public void Write(string message)
		{
			Write(0, message);
		}
		public void Write(uint flags, string message)
		{
			string prefix = "";
			if ((flags & ISimpleLog.FlagError) == ISimpleLog.FlagError) prefix = "ERROR ";
			else if ((flags & ISimpleLog.FlagWarning) == ISimpleLog.FlagWarning) prefix = "WARNING ";
			Debug.WriteLine($"{prefix}{message}");
			baseLog.Write(flags, message);
		}
	}
#endif
}
