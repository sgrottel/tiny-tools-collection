using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redate
{
	static class FileTypeReg
	{

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "So don't care")]
		internal static void Register()
		{
			string appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

			using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(@".redate", true))
			{
				key.SetValue("", "SGR.Redate.File");
			}

			using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"SGR.Redate.File", true))
			{
				key.SetValue("", "Redate State Definition File");
				using (RegistryKey iconKey = key.CreateSubKey("DefaultIcon", true))
				{
					iconKey.SetValue("", "\"" + appPath + "\",0");
				}
				using (RegistryKey shellKey = key.CreateSubKey("shell", true))
				{
					shellKey.SetValue("", "open");
					using (RegistryKey openKey = shellKey.CreateSubKey(@"open\command", true))
					{
						openKey.SetValue("", "\"" + appPath + "\" run \"%1\"");
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "So don't care")]
		internal static void Unregister()
		{
			Registry.ClassesRoot.DeleteSubKeyTree(@".redate", false);
			Registry.ClassesRoot.DeleteSubKeyTree(@"SGR.Redate.File", false);
		}

	}
}
