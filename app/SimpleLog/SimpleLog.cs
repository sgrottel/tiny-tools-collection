// Copyright 2022-2023 SGrottel (www.sgrottel.de)
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

// Version: 2.3.2

#nullable enable

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SGrottel
{

	/// <summary>
	/// SimpleLog interface for writing a message
	/// </summary>
	public interface ISimpleLog
	{
		/// <summary>
		/// Flag message as warning
		/// </summary>
		const uint FlagWarning = 1;

		/// <summary>
		/// Flag message as error
		/// </summary>
		const uint FlagError = 2;

		/// <summary>
		/// Write a message to the log
		/// </summary>
		/// <param name="message">The message string. Expected to NOT contain a new line at the end.</param>
		void Write(string message);

		/// <summary>
		/// Write a message to the log
		/// </summary>
		/// <param name="flags">The message flags</param>
		/// <param name="message">The message string. Expected to NOT contain a new line at the end.</param>
		void Write(uint flags, string message);
	}

	/// <summary>
	/// SimpleLog implementation
	/// </summary>
	public class SimpleLog : ISimpleLog
	{
		/// <summary>
		/// Major version number constant
		/// </summary>
		public const int VERSION_MAJOR = 2;
		/// <summary>
		/// Minor version number constant
		/// </summary>
		public const int VERSION_MINOR = 3;
		/// <summary>
		/// Patch version number constant
		/// </summary>
		public const int VERSION_PATCH = 2;

		#region Static Write Convenience Functions
		static public void Write(ISimpleLog? log, string message) { log?.Write(message); }
		static public void Write(ISimpleLog? log, string format, object? arg0) { log?.Write(string.Format(format, arg0)); }
		static public void Write(ISimpleLog? log, string format, object? arg0, object? arg1) { log?.Write(string.Format(format, arg0, arg1)); }
		static public void Write(ISimpleLog? log, string format, object? arg0, object? arg1, object? arg2) { log?.Write(string.Format(format, arg0, arg1, arg2)); }
		static public void Write(ISimpleLog? log, string format, object?[] args) { log?.Write(string.Format(format, args)); }
		static public void Warning(ISimpleLog? log, string message) { log?.Write(ISimpleLog.FlagWarning, message); }
		static public void Warning(ISimpleLog? log, string format, object? arg0) { log?.Write(ISimpleLog.FlagWarning, string.Format(format, arg0)); }
		static public void Warning(ISimpleLog? log, string format, object? arg0, object? arg1) { log?.Write(ISimpleLog.FlagWarning, string.Format(format, arg0, arg1)); }
		static public void Warning(ISimpleLog? log, string format, object? arg0, object? arg1, object? arg2) { log?.Write(ISimpleLog.FlagWarning, string.Format(format, arg0, arg1, arg2)); }
		static public void Warning(ISimpleLog? log, string format, object?[] args) { log?.Write(ISimpleLog.FlagWarning, string.Format(format, args)); }
		static public void Error(ISimpleLog? log, string message) { log?.Write(ISimpleLog.FlagError, message); }
		static public void Error(ISimpleLog? log, string format, object? arg0) { log?.Write(ISimpleLog.FlagError, string.Format(format, arg0)); }
		static public void Error(ISimpleLog? log, string format, object? arg0, object? arg1) { log?.Write(ISimpleLog.FlagError, string.Format(format, arg0, arg1)); }
		static public void Error(ISimpleLog? log, string format, object? arg0, object? arg1, object? arg2) { log?.Write(ISimpleLog.FlagError, string.Format(format, arg0, arg1, arg2)); }
		static public void Error(ISimpleLog? log, string format, object?[] args) { log?.Write(ISimpleLog.FlagError, string.Format(format, args)); }
		#endregion

		#region Ctor

		[DllImport("shell32.dll")]
		private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

		/// <summary>
		/// Returns the default directory where log files are stored.
		///
		/// These locations are tested in this priority order:
		/// 1) "%appdata%\LocalLow\sgrottel_simplelog"
		/// 2) "logs" subfolder of the location of the process' executing assembly
		/// 3) the localion of the process' executing assembly
		/// 4) "logs" subfolder of the current working directory
		/// 5) the current working directory
		/// </summary>
		/// <returns>The default path where log files are stored</returns>
		/// <remarks>The function creates folders and files to test access rights. It removes all files and folders again.
		/// If the file system access rights allow for creation but not for deletion, empty test files or folders might stay behind.
		/// </remarks>
		public static string GetDefaultDirectory()
		{
			bool TestCreateFile(string path)
			{
				try
				{
					// Try writing a file
					string filename;
					do
					{
						filename = Path.Combine(path, "temp_" + Path.GetFileName(Path.GetTempFileName()));
					} while (File.Exists(filename));

					File.WriteAllText(filename, "test"); // if `parent` is read only, this is supposed to throw.
					File.Delete(filename);

					return true;
				}
				catch { }
				return false;
			}
			string? parent, path, createdDir = null;

			string CleanDirGuard(string otherPath)
			{
				if (createdDir != null)
				{
					if (Directory.Exists(createdDir))
					{
						Directory.Delete(createdDir);
					}
					createdDir = null;
				}
				return otherPath;
			}

			parent = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			IntPtr pszPath = IntPtr.Zero;
			// try getting "%appdata%\LocalLow"
			int hr = SHGetKnownFolderPath(new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16"), 0, IntPtr.Zero, out pszPath);
			if (hr >= 0) parent = Marshal.PtrToStringAuto(pszPath);
			// else parent will stay as "%appdata%\Local"
			if (pszPath != IntPtr.Zero) Marshal.FreeCoTaskMem(pszPath);
			if (Directory.Exists(parent))
			{
				path = Path.Combine(parent, "sgrottel_simplelog");
				if (!Directory.Exists(path))
				{
					try
					{
						Directory.CreateDirectory(createdDir = path);
					}
					catch { }
				}
				if (Directory.Exists(path))
				{
					if (TestCreateFile(path))
						return CleanDirGuard(path);
				}
				CleanDirGuard("");
				if (TestCreateFile(parent))
					return parent;
			}

			parent = AppContext.BaseDirectory;
			if (Directory.Exists(parent))
			{
				path = Path.Combine(parent, "logs");
				if (!Directory.Exists(path))
				{
					try
					{
						Directory.CreateDirectory(createdDir = path);
					}
					catch { }
				}
				if (Directory.Exists(path))
				{
					if (TestCreateFile(path))
						return CleanDirGuard(path);
				}
				CleanDirGuard("");
				if (TestCreateFile(parent))
					return parent;
			}

			parent = Environment.CurrentDirectory;
			path = Path.Combine(parent, "logs");
			if (!Directory.Exists(path))
			{
				try
				{
					Directory.CreateDirectory(createdDir = path);
				}
				catch { }
			}
			if (Directory.Exists(path))
			{
				if (TestCreateFile(path))
					return CleanDirGuard(path);
			}
			CleanDirGuard("");

			return parent;
		}

		/// <summary>
		/// Determines the default name for log files of this process.
		/// The value is based on the process' executing assembly.
		/// </summary>
		/// <returns>The default name for log files of this process</returns>
		public static string GetDefaultName()
		{
			Assembly asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
			string? name = asm.GetName().Name;
			if (name == null)
			{
				name = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
			}
			return name;
		}

		/// <summary>
		/// Gets the default retention, i.e. how many previous log files are kept in the target directory in addition to the current log file
		/// </summary>
		/// <returns>The default log file retention count.</returns>
		public static int GetDefaultRetention()
		{
			return 10;
		}

		/// <summary>
		/// Creates a SimpleLog with default values for directory, name, and retention
		/// </summary>
		public SimpleLog() : this(GetDefaultDirectory(), GetDefaultName(), GetDefaultRetention()) { }

		private StreamWriter? writer = null;

		/// <summary>
		/// Creates a SimpleLog instance.
		/// </summary>
		/// <param name="directory">The directory where log files are stored</param>
		/// <param name="name">The name for log files of this process without file name extension</param>
		/// <param name="retention">The default log file retention count; must be 2 or larger</param>
		public SimpleLog(string directory, string name, int retention)
		{
			if (directory == null && name == null)
			{
				writer = null;
				return;
			}

			// check arguments
			if (directory == null) throw new ArgumentNullException("directory");
			if (name == null) throw new ArgumentNullException("name");
			if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("directory");
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name");
			if (retention < 2) throw new ArgumentException("retention");

			using (Mutex logSetupMutex = new Mutex(false, "SGROTTEL_SIMPLELOG_CREATION"))
			{
				logSetupMutex.WaitOne();
				try
				{
					if (!Directory.Exists(directory))
					{
						string? pp = Path.GetDirectoryName(directory);
						if (!Directory.Exists(pp)) throw new Exception("Log directory does not exist");
						Directory.CreateDirectory(directory);
						if (!Directory.Exists(directory)) throw new Exception("Failed to create log directory");
					}

					string fn = Path.Combine(directory, string.Format("{0}.{1}.log", name, retention - 1));
					if (File.Exists(fn))
					{
						File.Delete(fn);
						if (File.Exists(fn)) throw new Exception(string.Format("Failed to delete old log file '{0}'", fn));
					}

					for (int i = retention - 1; i > 0; --i)
					{
						string tfn = Path.Combine(directory, string.Format("{0}.{1}.log", name, i));
						string sfn = Path.Combine(directory, string.Format("{0}.{1}.log", name, i - 1));
						if (i == 1) sfn = Path.Combine(directory, string.Format("{0}.log", name));
						if (!File.Exists(sfn)) continue;
						if (File.Exists(tfn)) throw new Exception(string.Format("Log file retention error. Unexpected log file: '{0}'", tfn));
						File.Move(sfn, tfn);
						if (File.Exists(sfn)) throw new Exception(string.Format("Log file retention error. Unable to move log file: '{0}'", sfn));
					}

					// Share mode `Delete` allows other processes to rename the file while it is being written.
					// This works because this process keeps an open file handle to write messages, and never reopens based on a file name.
					var file = File.Open(
						Path.Combine(directory, string.Format("{0}.log", name)),
						FileMode.OpenOrCreate,
						FileAccess.Write,
						FileShare.Read | FileShare.Delete);
					file.Seek(0, SeekOrigin.End); // append
					writer = new StreamWriter(file, new UTF8Encoding(false));
				}
				finally
				{
					logSetupMutex.ReleaseMutex();
				}
			}
		}
		#endregion

		#region Implement ISimpleLog
		/// <summary>
		/// Write a message to the log
		/// </summary>
		/// <param name="message">The message string. Expected to NOT contain a new line at the end.</param>
		public virtual void Write(string message) { Write(0, message); }

		/// <summary>
		/// Write a message to the log
		/// </summary>
		/// <param name="flags">The message flags</param>
		/// <param name="message">The message string. Expected to NOT contain a new line at the end.</param>
		public virtual void Write(uint flags, string message)
		{
			lock (threadLock)
			{
				if (writer == null) return;
				string type = "";
				if ((flags & ISimpleLog.FlagError) == ISimpleLog.FlagError) type = "ERROR";
				else if ((flags & ISimpleLog.FlagWarning) == ISimpleLog.FlagWarning) type = "WARNING";
				writer.WriteLine("{0:u}|{2} {1}", DateTime.Now, message, type);
				writer.Flush();
			}
		}
		#endregion

		/// <summary>
		/// Object used to thread-lock all output
		/// </summary>
		protected object threadLock = new object();

	}

	/// <summary>
	/// Extention to SimpleLog which, which echoes all messages to the console
	/// </summary>
	public class EchoingSimpleLog : SimpleLog
	{
		/// <summary>
		/// Creates a EchoingSimpleLog with default values for directory, name, and retention
		/// </summary>
		public EchoingSimpleLog() : base() { }

		/// <summary>
		/// Creates a EchoingSimpleLog instance.
		/// </summary>
		/// <param name="directory">The directory where log files are stored</param>
		/// <param name="name">The name for log files of this process</param>
		/// <param name="retention">The default log file retention count</param>
		public EchoingSimpleLog(string directory, string name, int retention) : base(directory, name, retention) { }

		/// <summary>
		/// If set to `true`, warning and error message will be echoed to `Console.Error`.
		/// For normal messages or if this is set to `false` the messages will be echoed to `Console.Out`
		/// </summary>
		public bool UseErrorOut { get; set; } = true;

		/// <summary>
		/// If set to `true`, warnings will be printed as yellow on black and errors will be printed as red on black.
		/// </summary>
		public bool UseColor { get; set; } = true;

		/// <summary>
		/// Write a message to the log and echoes the message to the console.
		/// </summary>
		/// <param name="message">The message string. Expected to NOT contain a new line at the end.</param>
		public override void Write(string message) { Write(0, message); }

		/// <summary>
		/// Write a message to the log and echoes the message to the console.
		/// </summary>
		/// <param name="flags">The optional message flags</param>
		/// <param name="message">The message string. Expected to NOT contain a new line at the end.</param>
		public override void Write(uint flags, string message)
		{
			base.Write(flags, message);
			lock (threadLock)
			{
				bool isError = (flags & ISimpleLog.FlagError) == ISimpleLog.FlagError;
				bool isWarning = (flags & ISimpleLog.FlagWarning) == ISimpleLog.FlagWarning;

				if (UseColor && isError)
				{
					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.Red;
				}
				else if (UseColor && isWarning)
				{
					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.Yellow;
				}

				((UseErrorOut && (isError || isWarning)) ? Console.Error : Console.Out).WriteLine(message);

				if (UseColor && (isError || isWarning))
				{
					Console.ResetColor();
				}
			}
		}
	}

}
