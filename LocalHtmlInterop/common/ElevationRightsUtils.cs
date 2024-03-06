using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace LocalHtmlInterop
{
	static public class ElevationRightsUtils
	{

		public static bool IsAdministrator()
		{
			if (OperatingSystem.IsWindows())
			{
				using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
				{
					WindowsPrincipal principal = new WindowsPrincipal(identity);
					return principal.IsInRole(WindowsBuiltInRole.Administrator);
				}
			}

			return false;
		}

	}

}
