using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using static ConProgBarSharp.TaskbarProgress;

namespace ConProgBarSharp
{

	/// <summary>
	/// Interface implementation for TaskbarProgress
	/// </summary>
	public abstract class TaskbarProgress
	{
		public enum State
		{
			None = 0,
			Indeterminate = 0x1,
			Normal = 0x2,
			Error = 0x4,
			Paused = 0x8
		}

		public abstract bool SetState(State state);

		public abstract bool SetValue(ulong value, ulong maxValue);

	};

	/// <summary>
	/// Empty implementation of TaskbarProgress, which succeeds always, but does nothing
	/// </summary>
	public class NullTaskbarProgress : TaskbarProgress
	{
		public override bool SetState(State state) => true;
		public override bool SetValue(ulong value, ulong maxValue) => true;
	};

	/// <summary>
	/// Windows (11) implementation of TaskbarProgress
	/// </summary>
	[SupportedOSPlatform("windows")]
	public class WinTaskbarProgress : TaskbarProgress
	{

		/// <summary>
		/// The handle to the application main window
		/// </summary>
		public IntPtr Handle { get; set; } = IntPtr.Zero;

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

		/// <summary>
		///  Does a set-console-title-findwindow-by-name trick
		/// </summary>
		public static IntPtr FindConsoleWindowHandle()
		{
			string oldTitle = Console.Title;
			string newTitle = $"{oldTitle} - {System.Diagnostics.Process.GetCurrentProcess().Id}{DateTime.Now.Ticks}";

			Console.Title = newTitle;

			Thread.Sleep(40);

			string nt = Console.Title;

			if (nt != newTitle)
			{
				if (nt == oldTitle)
				{
					throw new Exception("Failed to set console title");
				}
			}

			IntPtr p = FindWindow(null, nt);

			Console.Title = oldTitle;
			return p;
		}


		[ComImport,
		 Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf"),
		 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface ITaskbarList3
		{
			#region ITaskbarList
			[PreserveSig]
			public int HrInit();
			[PreserveSig]
			public void AddTab(IntPtr hwnd);
			[PreserveSig]
			public void DeleteTab(IntPtr hwnd);
			[PreserveSig]
			public void ActivateTab(IntPtr hwnd);
			[PreserveSig]
			public void SetActiveAlt(IntPtr hwnd);
			#endregion

			#region ITaskbarList2
			[PreserveSig]
			public void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
			#endregion

			#region ITaskbarList3
			[PreserveSig]
			public int SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);
			[PreserveSig]
			public int SetProgressState(IntPtr hwnd, State state);
			#endregion
		}

		[ComImport,
		 Guid("56fdf344-fd6d-11d0-958a-006097c9a090"),
		 ClassInterface(ClassInterfaceType.None)]
		private class TaskbarInstance
		{
		}

		private ITaskbarList3? tb = InitTaskbarList3Instance();

		private static ITaskbarList3? InitTaskbarList3Instance()
		{
			try
			{
				TaskbarInstance i = new();
				ITaskbarList3? i3 = i as ITaskbarList3;
				if (i3?.HrInit() >= 0)
				{
					return i3;
				}
			}
			catch
			{
			}
			return null;
		}

		public override bool SetState(State state)
		{
			if (tb == null) return false; // not supported
			if (Handle == IntPtr.Zero) return false; // no window set
			int rv = tb.SetProgressState(Handle, state);
			return rv >= 0;
		}

		public override bool SetValue(ulong value, ulong maxValue)
		{
			if (tb == null) return false; // not supported
			if (Handle == IntPtr.Zero) return false; // no window set
			int rv = tb.SetProgressValue(Handle, value, maxValue);
			return rv >= 0;
		}
	}

}
