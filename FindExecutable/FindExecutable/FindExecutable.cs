//
// FindExecutable.cs
// Copyright, by SGrottel.de  https://www.github.com/sgrottel/tiny-tools-collection
// Open Source under the `MIT license`
//
// Copyright(c) 2023 Sebastian Grottel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FindExecutable
{

	/// <summary>
	/// Utility class to help finding the full file system path from an executable name
	/// </summary>
	public static class FindExecutable
	{

		/// <param name="executable">The name of the executable to find.</param>
		/// <param name="includeCurrentDirectory">If set to true, the current execution directory is included in the list of search paths</param>
		/// <param name="includeBaseDirectory">If set to true, the application domain base directory is included in the list of search paths</param>
		/// <param name="additionalPaths">If set to something other then null, the paths iterated within are included in the list of search paths</param>
		/// <returns>The full file system path to the executable file requested, or null if the file is not found.</returns>
		/// <remarks>
		/// The function searches in all paths specified by the `PATH` environment variable.
		///
		/// Depending on the platform the code is run on, `executable` might be case sensitive.
		///
		/// When searching on Windows and the name does not specify a file name extension,
		/// `.exe` is assumed, if no executable file without file name extension is found.
		///
		/// When searching on Linux and the n ame does contain a file name extension,
		/// the extension is removed, if no executable file is found with it.
		/// </remarks>
		public static string? FullPath(
			string executable,
			bool includeCurrentDirectory = false,
			bool includeBaseDirectory = false,
			IEnumerable<string>? additionalPaths = null)
		{
			IEnumerable<string> paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
			if (includeCurrentDirectory)
				paths = paths.Concat(new string[] { Environment.CurrentDirectory });
			if (includeBaseDirectory)
				paths = paths.Concat(new string[] { AppDomain.CurrentDomain.BaseDirectory });
			if (additionalPaths != null)
				paths = paths.Concat(additionalPaths);

			while (!string.IsNullOrEmpty(executable))
			{
				foreach (string path in paths)
				{
					if (string.IsNullOrWhiteSpace(path)) continue;
					if (!Directory.Exists(path)) continue;
					string p = Path.GetFullPath(path);
					if (!Directory.Exists(p)) continue;

					string f = Path.Combine(p, executable);
					if (File.Exists(f))
					{
						if (IsExecutable(f))
						{
							return f;
						}
					}
				}

				// Not found in first go. Maybe try fallback
				if (OperatingSystem.IsWindows())
				{
					if (string.IsNullOrEmpty(Path.GetExtension(executable)))
					{
						executable = Path.ChangeExtension(executable, ".exe");
					}
					else break;
				}
				else
				{
					if (!string.IsNullOrEmpty(Path.GetExtension(executable)))
					{
						executable = Path.GetFileNameWithoutExtension(executable);
					}
					else break;
				}
			}

			return null;
		}

		#region Test if File is Executable

		#region Windows P/Invoke
		[DllImport("shell32.dll")]
		private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, IntPtr psfi, uint cbSizeFileInfo, uint uFlags);
		private const uint SHGFI_EXETYPE = 0x000002000;
		#endregion

		[SupportedOSPlatform("Windows")]
		private static bool IsExecutableOnWindow(string path)
		{
			try
			{
				// Ask the windows shell what kind of file `path` points to.
				IntPtr exeType = SHGetFileInfo(path, 128, IntPtr.Zero, 0, SHGFI_EXETYPE);
				long wparam = exeType.ToInt64();
				int loWord = (int)(wparam & 0xffff);
				int hiWord = (int)(wparam >> 16);

				if (wparam != 0)
				{
					if (hiWord == 0x0000 && loWord == 0x5a4d)
					{
						return true; // Dos
					}
					else if (hiWord == 0x0000 && loWord == 0x4550)
					{
						return true; // Console
					}
					else if ((hiWord != 0x0000) && (loWord == 0x454E || loWord == 0x4550 || loWord == 0x454C))
					{
						return true; // Windows
					}
				}
				return false; // Very likely not an executable
			}
			catch { }

			// it exists, but shell does not answer. So, in doubt, give it a try... might work.
			return true;
		}

		#region Linux P/Invoke
		[DllImport("libc", SetLastError = true)]
		private static extern int access(string pathname, int mode);
		// https://codebrowser.dev/glibc/glibc/posix/unistd.h.html#283
		private const int X_OK = 1;
		#endregion

		[UnsupportedOSPlatform("Windows")]
		private static bool IsExecutableOnLinux(string path)
		{
			try
			{
				// access with X_OK to test if the user of the current process
				// should be able to execute the specified file.
				if (access(path, X_OK) == 0)
				{
					return true;
				}
			}
			catch { }
			return false;
		}

		/// <summary>
		/// Tests if a file is (likely) executable
		/// </summary>
		/// <param name="path">The path to the file to test</param>
		/// <returns>True if the file is executable, false otherwise.</returns>
		/// <remarks>If `path` does not point to an existing file, the function returns false.</remarks>
		public static bool IsExecutable(string path)
		{
			if (!File.Exists(path)) return false;
			if (OperatingSystem.IsWindows())
			{
				return IsExecutableOnWindow(path);
			}
			else
			{
				return IsExecutableOnLinux(path);
			}
		}

		#endregion

	}

}
