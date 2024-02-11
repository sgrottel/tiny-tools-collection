using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace LocalHtmlInterop
{
	public class ServerPortConfig
	{
		public const ushort DefaultPort = 18245;
		private const string KeyName = @"Software\SGrottel\LocalHtmlInterop";

		public ushort GetValue()
		{
			if (!OperatingSystem.IsWindows())
			{
				return DefaultPort;
			}

			using (RegistryKey hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
			{
				if (hkcu == null) throw new InvalidOperationException("Failed to open HKCurrentUser");
				using (RegistryKey? appKey = hkcu.OpenSubKey(KeyName))
				{
					if (appKey == null) return DefaultPort;
					int? o = appKey.GetValue("port") as int?;
					return o.HasValue ? ((ushort)o.Value) : DefaultPort;
				}
			}
		}

		[SupportedOSPlatform("windows")]
		public void SetValue(ushort port)
		{
			using (RegistryKey hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
			{
				if (hkcu == null) throw new InvalidOperationException("Failed to open HKCurrentUser");
				using (RegistryKey appKey = hkcu.CreateSubKey(KeyName))
				{
					appKey.SetValue("port", port, RegistryValueKind.DWord);
				}
			}
		}

	}
}
