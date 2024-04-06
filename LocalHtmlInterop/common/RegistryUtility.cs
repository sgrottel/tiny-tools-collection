using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace LocalHtmlInterop
{

	public static class RegistryUtility
	{

		public const string SubKeyName = "Software\\SGrottel\\LocalHtmlInterop";

		public static HashSet<string> LoadFilePaths()
		{
			HashSet<string> files = LoadFilePathsFromLocalMachine();
			if (OperatingSystem.IsWindows())
			{
				LoadFilePaths(files, Registry.CurrentUser);
			}
			return files;
		}

		public static HashSet<string> LoadFilePathsFromLocalMachine()
		{
			HashSet<string> files = new();
			if (OperatingSystem.IsWindows())
			{
				LoadFilePaths(files, Registry.LocalMachine);
			}
			return files;
		}

		[SupportedOSPlatform("windows")]
		private static void LoadFilePaths(HashSet<string> files, RegistryKey baseKey)
		{
			using (var regkey = baseKey.OpenSubKey(SubKeyName))
			{
				if (regkey == null) return;

				IEnumerable<string> knownFiles
					= regkey
					.GetValueNames()
					.Where((s) => s.StartsWith("file-"))
					.Select((s) => regkey.GetValue(s)?.ToString() ?? string.Empty);

				foreach (string f in knownFiles)
				{
					files.Add(f);
				}
			}
		}

	}

}
