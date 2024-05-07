using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LocalHtmlInterop.Handler
{
	internal class MessageBox
	{
		internal const uint MB_OK = 0x00000000;
		internal const uint MB_ICONERROR = 0x00000010;
		internal const uint MB_ICONQUESTION = 0x00000020;
		internal const uint MB_ICONWARNING = 0x00000030;
		internal const uint MB_ICONINFORMATION = 0x00000040;

		[DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern int NativeMessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

		static internal int Show(IntPtr hWnd, string lpText, string lpCaption, uint uType)
		{
			return NativeMessageBox(hWnd, lpText, lpCaption, uType);
		}

	}

}
