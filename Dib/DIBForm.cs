using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Versioning;
using System.Reflection;
using System.IO;

namespace Dib {

    /// <summary>
    /// The DIB main form
    /// </summary>
    [SupportedOSPlatform("windows")]
    public partial class DIBForm : Form {

        #region Interop definitions

        /// <summary>
        /// FindWindow Signatur 1/2
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// FindWindow Signatur 2/2
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, IntPtr lpWindowName);

        /// <summary>
        /// FindWindowEx Signatur 1/2
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        /// <summary>
        /// FindWindowEx Signatur 2/2
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

        /// <summary>
        /// GetWindowLong Signatur
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// SendMessage Signatur 1/3
        /// </summary>
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        /// <summary>
        /// SendMessage Signatur 2/3
        /// </summary>
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);

        /// <summary>
        /// SendMessage Signatur 3/3
        /// </summary>
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, ref LVITEM lParam);

        /// <summary>
        /// GetParent Signatur
        /// </summary>
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(IntPtr devMode, int flags);

        /// <summary>
        /// An application-defined callback function used with the
        /// EnumChildWindows function.
        /// </summary>
        /// <param name="hWnd">A handle to a child window of the parent window
        /// specified in EnumChildWindows.</param>
        /// <param name="lParam">The application-defined value given in
        /// EnumChildWindows.</param>
        /// <returns>To continue enumeration, the callback function must
        /// return TRUE; to stop enumeration, it must return FALSE.</returns>
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>
        /// Enumerates the child windows that belong to the specified parent
        /// window by passing the handle to each child window, in turn, to an
        /// application-defined callback function.
        /// </summary>
        /// <param name="hwndParent">A handle to the parent window whose child
        /// windows are to be enumerated.</param>
        /// <param name="lpEnumFunc">A pointer to an application-defined
        /// callback function.</param>
        /// <param name="lParam">An application-defined value to be passed to
        /// the callback function.</param>
        /// <returns>The return value is not used.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hwndParent,
            EnumWindowsProc lpEnumFunc, IntPtr lParam);

        /// <summary>
        /// Retrieves the name of the class to which the specified window
        /// belongs.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the
        /// class to which the window belongs.</param>
        /// <param name="lpClassName">The class name string.</param>
        /// <param name="nMaxCount">The length of the lpClassName buffer, in
        /// characters.</param>
        /// <returns>If the function succeeds, the return value is the number
        /// of characters copied to the buffer, not including the terminating
        /// null character.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        /// <summary>
        /// Retrieves a string that specifies the window type.
        /// </summary>
        /// <remarks>
        /// RealGetWindowClass versus GetClassName 
        /// The difference between RealGetWindowClass and GetClassName is that
        /// RealGetWindowClass retrieves the name of the base class for
        /// superclassed windows.
        /// See http://blogs.msdn.com/b/oldnewthing/archive/2010/12/31/10110524.aspx
        /// for more information.
        /// </remarks>
        /// <param name="hwnd">A handle to the window whose type will be
        /// retrieved.</param>
        /// <param name="pszType">A pointer to a string that receives the
        /// window type.</param>
        /// <param name="cchType">The length, in characters, of the buffer
        /// pointed to by the pszType parameter.</param>
        /// <returns>If the function succeeds, the return value is the number
        /// of characters copied to the specified buffer.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern uint RealGetWindowClass(IntPtr hwnd, [Out] StringBuilder pszType, uint cchType);

        #region ListView Messages
        /// <summary>
        /// ListView Messages
        /// </summary>
        const uint LVM_FIRST = 0x1000;
        const uint LVM_GETIMAGELIST = (LVM_FIRST + 2);
        const uint LVM_GETITEMCOUNT = (LVM_FIRST + 4);
        const uint LVM_GETITEMW = (LVM_FIRST + 75);
        const uint LVM_GETITEMTEXTW = (LVM_FIRST + 115);
        const uint LVM_SETITEMPOSITION = (LVM_FIRST + 15);
        const uint LVM_GETITEMPOSITION = (LVM_FIRST + 16);
        const uint LVM_SETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 54);
        const uint LVM_GETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 55);

        const int LVS_EX_SNAPTOGRID = 0x00080000;

        #endregion

        /// <summary>
        /// GetWindowLong constants
        /// </summary>
        const int GWL_STYLE = (-16);
        const int LVSIL_SMALL = 1;

        /// <summary>
        /// ListView Style constants
        /// </summary>
        const UInt32 LVS_AUTOARRANGE = 0x0100;

        /// <summary>
        /// windows messages
        /// </summary>
        const uint WM_COMMAND = 0x0111;

        /// <summary>
        /// special command message id
        /// </summary>
        static int IDM_TOGGLEAUTOARRANGE = 0;

        /// <summary>
        /// ProcessAccessFlags
        /// </summary>
        [Flags]
        enum ProcessAccessFlags : uint {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        /// <summary>
        /// OpenProcess Signatur
        /// </summary>
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        /// <summary>
        /// GetWindowThreadProcessId Signatur
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// VirtualAllocEx Signatur
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        /// <summary>
        /// VirtualFreeEx Signatur
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        /// <summary>
        /// POINT
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int X;
            public int Y;

            public POINT(int x, int y) {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p) {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p) {
                return new POINT(p.X, p.Y);
            }
        }

        /// <summary>
        /// virtual memory constants
        /// </summary>
        const uint MEM_COMMIT = 0x1000;
        const uint MEM_RESERVE = 0x2000;
        const uint MEM_RELEASE = 0x8000;

        const uint PAGE_READWRITE = 0x4;

        /// <summary>
        /// CloseHandle Signatur
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);

        /// <summary>
        /// WriteProcessMemory Signatur 1/4
        /// </summary>
        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        /// <summary>
        /// WriteProcessMemory Signatur 2/4
        /// </summary>
        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, char[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        /// <summary>
        /// WriteProcessMemory Signatur 3/4
        /// </summary>
        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref POINT lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        /// <summary>
        /// WriteProcessMemory Signatur 4/4
        /// </summary>
        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref LVITEM lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        /// <summary>
        /// ReadProcessMemory Signatur 1/4
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesRead);

        /// <summary>
        /// ReadProcessMemory Signatur 2/4
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [MarshalAs(UnmanagedType.LPTStr)]StringBuilder buf, int nSize, out IntPtr lpNumberOfBytesRead);

        /// <summary>
        /// ReadProcessMemory Signatur 3/4
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out POINT lpBuffer, uint nSize, out IntPtr lpNumberOfBytesRead);

        /// <summary>
        /// ReadProcessMemory Signatur 4/4
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out LVITEM lpBuffer, uint nSize, out IntPtr lpNumberOfBytesRead);

        /// <summary>
        /// ListViewItemFlags
        /// </summary>
        public enum ListViewItemFlags {
            LVIF_TEXT = 0x0001,
            LVIF_IMAGE = 0x0002,
            LVIF_PARAM = 0x0004,
            LVIF_STATE = 0x0008,
            LVIF_INDENT = 0x0010,
            LVIF_NORECOMPUTE = 0x0800
        }

        /// <summary>
        /// LVITEM
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct LVITEM {
            public ListViewItemFlags mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public int lParam;
            public int iIndent;
        }

        /// <summary>
        /// This function converts the client coordinates of a specified point to screen coordinates.
        /// </summary>
        /// <param name="hWnd">Handle to the window whose client area is used for the conversion.</param>
        /// <param name="lpPoint">Long pointer to a POINT structure that contains the client coordinates to be converted.</param>
        /// <returns>Nonzero indicates success. Zero indicates failure.</returns>
        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        /// <summary>
        /// This function converts the screen coordinates of a specified point on the screen to client coordinates.
        /// </summary>
        /// <param name="hWnd">Handle to the window whose client area will be used for the conversion.</param>
        /// <param name="lpPoint">Long pointer to a POINT structure that contains the screen coordinates to be converted.</param>
        /// <returns>Nonzero indicates success. Zero indicates failure.</returns>
        [DllImport("user32.dll")]
        static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        #endregion

        #region private attributes

        /// <summary>
        /// The registry key path to store the filename in
        /// </summary>
        private const string filenameRegKeyPath
            = @"HKEY_CURRENT_USER\Software\SGrottel\Dib";

        /// <summary>
        /// The registry key to store the filename in
        /// </summary>
        private const string filenameRegKeyName = @"lastfilename";

        /// <summary>
        /// The filename used for the last file operation
        /// </summary>
        private string filename;

        #endregion

        #region utility types

        /// <summary>
        /// MyIconInfo
        /// </summary>
        public struct MyIconInfo {

            /// <summary>
            /// The title of the icon
            /// </summary>
            [XmlAttribute()]
            public String title;

            /// <summary>
            /// The x coordinate of the icon
            /// </summary>
            [XmlAttribute()]
            public int x;

            /// <summary>
            /// The y coordinate of the icon
            /// </summary>
            [XmlAttribute()]
            public int y;

        }

        /// <summary>
        /// Just to get a nice name
        /// </summary>
        [XmlRoot("dibxml")]
        public class IconCollection : List<MyIconInfo> { };

        #endregion

        /// <summary>
        /// Ctor.
        /// </summary>
        public DIBForm() {
            InitializeComponent();
            try
            {
                string? filepath = Assembly.GetExecutingAssembly().Location;
                if ( string.IsNullOrEmpty(filepath) || !File.Exists(filepath))
                {
                    filepath = Path.Combine(AppContext.BaseDirectory, "Dib.exe");
                }
                Icon = Icon.ExtractAssociatedIcon(filepath);
            }
            catch
            {
            }
            try {
                this.filename = Microsoft.Win32.Registry.GetValue(
                    filenameRegKeyPath, filenameRegKeyName, string.Empty)
                    as string;
            } catch {
                this.filename = string.Empty;
            }

            this.SetToggleAutoArrangeMessageID();
        }

        #region private utility methods

        /// <summary>
        /// Set the IDM_TOGGLEAUTOARRANGE attribute depending on the current
        /// operating system.
        /// This is a real problem and I would like to have a better solution!
        /// </summary>
        private void SetToggleAutoArrangeMessageID() {
            IDM_TOGGLEAUTOARRANGE = 0;
            try {
                // This is very unofficial!
                // The value of 'IDM_TOGGLEAUTOARRANGE' seems to be
                // OS-Version dependent:
                //  Win98:  7041
                //  WinNT4: 7031
                //  Else:   7051 (I hope this will still work on Windows 7)
                if (System.Environment.OSVersion.Platform
                        == PlatformID.Win32Windows) {
                    IDM_TOGGLEAUTOARRANGE = 0x7041;
                } else if (System.Environment.OSVersion.Platform
                        == PlatformID.Win32NT) {
                    if (System.Environment.OSVersion.Version.Major == 4) {
                        IDM_TOGGLEAUTOARRANGE = 0x7031;
                    } else {
                        IDM_TOGGLEAUTOARRANGE = 0x7051;
                    }
                } else {
                    throw new Exception("Unsupported Operating System");
                }
            } catch {
                IDM_TOGGLEAUTOARRANGE = 0;
            }
        }

        /// <summary>
        /// Collect all windows returned by EnumChildWindows
        /// </summary>
        /// <param name="hWnd">The new window</param>
        /// <param name="lParam">The list of windows</param>
        /// <returns>true</returns>
        private static bool collectChildren(IntPtr hWnd, IntPtr lParam) {
            GCHandle hList = GCHandle.FromIntPtr(lParam);
            List<IntPtr> list = hList.Target as List<IntPtr>;
            if (list != null) {
                list.Add(hWnd);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Collects all child windows of the specified window
        /// </summary>
        /// <param name="hWnd">The window</param>
        /// <param name="wnds">The list to receive all child windows.
        /// The list will be cleared before the operation starts</param>
        private static void getChildWindows(IntPtr hWnd,
                ref List<IntPtr> wnds) {
            wnds.Clear();
            GCHandle listHandle = GCHandle.Alloc(wnds);
            try {
                EnumWindowsProc callback = new EnumWindowsProc(collectChildren);
                EnumChildWindows(hWnd, callback, GCHandle.ToIntPtr(listHandle));
            } finally {
                if (listHandle.IsAllocated) {
                    listHandle.Free();
                }
            }
        }

        /// <summary>
        /// Answer the (real) class for a window
        /// </summary>
        /// <param name="hWnd">The window</param>
        /// <returns>The class name</returns>
        public static string getWindowClass(IntPtr hWnd) {
            StringBuilder pszType = new StringBuilder();
            pszType.Capacity = 255;
            RealGetWindowClass(hWnd, pszType, (UInt32)pszType.Capacity);
            return pszType.ToString();
        }

        /// <summary>
        /// Finds the handle to the list view control of the desktop
        /// </summary>
        /// <returns>The list view control of the desktop</returns>
        private IntPtr FindDesktopListView() {
            // In fact, I don't known what I am doing here! I used 'Spy++' and
            // this is what I found to be the best way of getting the handle.
            IntPtr hWnd = IntPtr.Zero;

            List<IntPtr> windows1 = new List<IntPtr>();
            List<IntPtr> windows2 = new List<IntPtr>();
            List<IntPtr> childWindows = new List<IntPtr>();

            // Strange Win7-Behaviour: Desktop is not child of Progman/Shell?
            // Why? What? WTF?
            // Therefore:
            //  Find "SysListView32", which is child of "SHELLDLL_DefView",
            //  which is either child of "WorkerW"-Top-Level-Window
            //  or child of "Progman"-Top-Level-Window

            getChildWindows(IntPtr.Zero, ref windows1);
            foreach (IntPtr w in windows1) {
                string cn = getWindowClass(w);
                if (cn.Equals("Progman")) windows2.Add(w); // this is what 'GetShellWindow' would have returned
                if (cn.Equals("WorkerW")) windows2.Add(w);
            }

            windows1.Clear();
            foreach (IntPtr w in windows2) {
                getChildWindows(w, ref childWindows);
                foreach (IntPtr c in childWindows) {
                    string cn = getWindowClass(c);
                    if (cn.Equals("SHELLDLL_DefView")) windows1.Add(c);
                }
            }

            windows2.Clear();
            foreach (IntPtr w in windows1) {
                getChildWindows(w, ref childWindows);
                foreach (IntPtr c in childWindows) {
                    string cn = getWindowClass(c);
                    if (cn.Equals("SysListView32")) windows2.Add(c);
                }
            }

            if (windows2.Count >= 1) {
                hWnd = windows2[0];
            }

            return hWnd;
        }

        /// <summary>
        /// Gets the icons on the desktop
        /// </summary>
        /// <param name="hWnd">Handle to the list view control of the desktop</param>
        /// <returns>All the icons on the desktop</returns>
        private IconCollection GetDesktopIcons(IntPtr hWnd) {
            IconCollection icons = new IconCollection();
            MyIconInfo iconinfo;

            // We will have to switch AutoArrange off, so test if it is on,
            // so that we can restore the option later
            bool autoArrange = false;
            UInt32 style = GetWindowLong(hWnd, GWL_STYLE);
            if ((style & LVS_AUTOARRANGE) == LVS_AUTOARRANGE) {
                if (IDM_TOGGLEAUTOARRANGE != 0) {
                    autoArrange = true;
                    IntPtr parent = GetParent(hWnd);
                    SendMessage(parent, WM_COMMAND, IDM_TOGGLEAUTOARRANGE, 0);
                }
            }

            // We will have to use memory within the desktops explorer process
            // to receive the icon information (IPC is always a pain).
            //
            // The shared memory has to hold a LVITEM structue and a string.
            // I place both objects into the same block of memory (first the
            // structure, then the string). However, since I have not found
            // the Read/WriteProcessMemory functions, with offsets, I will
            // have to offset the data manually.
            //
            // Setting up the memory for IPC
            uint processID = 0;
            uint threadID = GetWindowThreadProcessId(hWnd, out processID);
            uint pointSize = 8;
            uint titleLength = 1024;
            uint titleRequestSize = titleLength
                + (uint)Marshal.SizeOf(typeof(LVITEM));
            uint maxMemorySize = Math.Max(pointSize, titleRequestSize);
            IntPtr process = OpenProcess(ProcessAccessFlags.VMOperation
                | ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite,
                false, processID);
            IntPtr sharedMem = VirtualAllocEx(process, IntPtr.Zero,
                maxMemorySize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

            if (sharedMem == IntPtr.Zero) {
                throw new Exception("Unable to setup shared memory to receive "
                    + "desktop icon positions");
            }

            // create the object to receive the icon informations
            POINT pt = new POINT();
            char[] title = new char[titleRequestSize];
            LVITEM lvItem = new LVITEM();
            IntPtr outSize;

            // fetch icons
            int count = SendMessage(hWnd, LVM_GETITEMCOUNT, 0, 0);
            for (int idx = 0; idx < count; idx++) {
                iconinfo.title = string.Empty;
                iconinfo.x = iconinfo.y = 0;

                // fetching the icons title
                // building the request
                for (int i = 0; i < titleRequestSize; i++) {
                    title[i] = '\0'; // hurgh!
                }
                lvItem.iItem = idx;
                lvItem.iSubItem = 0;
                lvItem.mask = ListViewItemFlags.LVIF_TEXT;
                lvItem.cchTextMax = (int)titleLength;
                lvItem.pszText = (IntPtr)((int)sharedMem
                    + Marshal.SizeOf(typeof(LVITEM)));
                // sending the request
                WriteProcessMemory(process, sharedMem, title,
                    titleRequestSize, out outSize);
                WriteProcessMemory(process, sharedMem, ref lvItem,
                    (uint)Marshal.SizeOf(typeof(LVITEM)), out outSize);
                if (SendMessage(hWnd, LVM_GETITEMW, idx, sharedMem) != 0) {
                    // receiving the answer
                    ReadProcessMemory(process, sharedMem, out lvItem,
                        (uint)Marshal.SizeOf(typeof(LVITEM)), out outSize);
                    // I am X-ing out the structure to be able to read the
                    // string without offset, by simply reading everything and
                    // later skipping the Xs from the structure
                    for (int i = 0; i < Marshal.SizeOf(typeof(LVITEM)); i++) {
                        title[i] = 'X';
                    }
                    StringBuilder iconTitle
                        = new StringBuilder((int)titleRequestSize);
                    WriteProcessMemory(process, sharedMem, title,
                        (uint)Marshal.SizeOf(typeof(LVITEM)), out outSize);
                    ReadProcessMemory(process, sharedMem, iconTitle,
                        (int)Math.Min(titleRequestSize,
                        iconTitle.MaxCapacity), out outSize);
                    iconinfo.title = iconTitle.ToString().Substring(
                        Marshal.SizeOf(typeof(LVITEM)) / 2);
                }

                // fetch the icons position
                // similar as fetching the text, but much easier since
                // LVM_GETITEMPOSITION is specialized and has simpler
                // parameters
                WriteProcessMemory(process, sharedMem, ref pt, pointSize,
                    out outSize);
                if (SendMessage(hWnd, LVM_GETITEMPOSITION, idx, sharedMem)
                        != 0) {
                    ReadProcessMemory(process, sharedMem, out pt, pointSize,
                        out outSize);

                    // FIX: use better values if the desktop spans into the negative region
                    ClientToScreen(hWnd, ref pt);

                    iconinfo.x = pt.X;
                    iconinfo.y = pt.Y;
                }

                // if we got all our informations, we want to store the found
                // icon. Note that checking the positions to zero is NOT very
                // good, since in theory an icon could be place there, but in
                // practice this seems to be a usable solution
                if (!string.IsNullOrEmpty(iconinfo.title)
                        && ((iconinfo.x != 0) || (iconinfo.y != 0))) {
                    icons.Add(iconinfo);
                }
            }

            // free the foreign process memory 
            VirtualFreeEx(process, sharedMem, maxMemorySize, MEM_RELEASE);
            CloseHandle(process);

            // reset the autoArrange option if we changed it
            if (autoArrange) {
                IntPtr parent = GetParent(hWnd);
                SendMessage(parent, WM_COMMAND, IDM_TOGGLEAUTOARRANGE, 0);
            }

            // Well, that was not that bad at all
            return icons;
        }

        #endregion

        #region event handlers

        /// <summary>
        /// Store the last used filename as option to the registry
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void DIBForm_FormClosing(object sender,
                FormClosingEventArgs e) {
            try {
                Microsoft.Win32.Registry.SetValue(
                    filenameRegKeyPath, filenameRegKeyName, this.filename,
                    Microsoft.Win32.RegistryValueKind.String);
            } catch {
            }
        }

        /// <summary>
        /// exit button click event handler.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void exitButton_Click(object sender, EventArgs e) {
            this.Close();
        }

        /// <summary>
        /// The link label has been clicked
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void linkLabel1_LinkClicked(object sender,
                LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo() {
                    FileName = linkLabel1.Text,
                    UseShellExecute = true
                });
        }

        /// <summary>
        /// Sets the current action
        /// </summary>
        /// <param name="p">The name of the current action</param>
        private void startAction(string p) {
            this.loadButton.Enabled = false;
            this.saveButton.Enabled = false;
            this.exitButton.Enabled = false;
            this.statusLabel.Text = p;
        }

        /// <summary>
        /// Ends the current action
        /// </summary>
        /// <param name="abort">True if the action was aborted</param>
        private void endAction(bool abort) {
            this.statusLabel.Text = this.statusLabel.Text + " - "
                + (abort ? "aborted" : "completed");
            this.loadButton.Enabled = true;
            this.saveButton.Enabled = true;
            this.exitButton.Enabled = true;
        }

        /// <summary>
        /// Save all the desktop icons to a file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void saveButton_Click(object sender, EventArgs e) {
            this.startAction("Saving Desktop Icons' Positions");
            try {
                this.saveFileDialog.FileName = this.filename;
                this.saveFileDialog.InitialDirectory
                    = System.IO.Path.GetDirectoryName(this.filename);
            } catch {
            }
            if (this.saveFileDialog.ShowDialog() != DialogResult.OK) {
                this.endAction(true);
                return;
            }

            try {

                IntPtr hWnd = FindDesktopListView();
                if (hWnd == IntPtr.Zero) {
                    throw new Exception(
                        "Unable to find the desktops listview control");
                }
                IconCollection icons = this.GetDesktopIcons(hWnd);

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                using (XmlWriter writer = XmlWriter.Create(
                        this.saveFileDialog.FileName, settings)) {
                    XmlSerializer xser
                        = new XmlSerializer(typeof(IconCollection));
                    xser.Serialize(writer, icons);
                }

                this.filename = this.saveFileDialog.FileName;
                this.endAction(false);

            } catch (Exception ex) {
                MessageBox.Show("Failed: " + ex.ToString(),
                    "Desktop Icon Backup", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.endAction(true);
            }
        }

        /// <summary>
        /// Restores the desktop icon positions from an xml file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void loadButton_Click(object sender, EventArgs e) {
            this.startAction("Loading Desktop Icons' Positions");
            try {
                this.openFileDialog.FileName = this.filename;
                this.openFileDialog.InitialDirectory
                    = System.IO.Path.GetDirectoryName(this.filename);
            } catch {
            }
            if (this.openFileDialog.ShowDialog() != DialogResult.OK) {
                this.endAction(true);
                return;
            }

            try {

                IconCollection icons = null;
                using (XmlReader reader = XmlReader.Create(
                        this.openFileDialog.FileName)) {
                    XmlSerializer xser
                        = new XmlSerializer(typeof(IconCollection));
                    icons = (IconCollection)xser.Deserialize(reader);
                }
                if (icons == null) {
                    throw new Exception("Failed to load dibxml icon file");
                }
                IntPtr hWnd = FindDesktopListView();
                if (hWnd == IntPtr.Zero) {
                    throw new Exception(
                        "Unable to find the desktops listview control");
                }
                IconCollection dskicons = this.GetDesktopIcons(hWnd);

                // remove all entries from icons which are not present in
                // dskicons
                for (int i = icons.Count - 1; i >= 0; i--) {
                    bool found = false;
                    foreach (MyIconInfo mii in dskicons) {
                        if (mii.title.Equals(icons[i].title)) {
                            found = true;
                            dskicons.Remove(mii);
                            break;
                        }
                    }
                    if (found) continue;
                    icons.RemoveAt(i);
                }
                if (dskicons.Count > 0) {
                    if (MessageBox.Show("There are desktop icons for which "
                        + "no position was stored. These icons will not be "
                        + "moved and might overlap with other icons.\nDo you "
                        + "want continue?", "Desktop Icon Backup",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2)
                            != DialogResult.Yes) {
                        this.endAction(true);
                        return;
                    }
                }

                // get the info whether to use the icon grid!
                bool useGrid = false;
                int extStyle = SendMessage(hWnd, LVM_GETEXTENDEDLISTVIEWSTYLE, 0, 0);
                if ((extStyle & LVS_EX_SNAPTOGRID) != 0) {
                    useGrid = true;
                }

                // check that the targeted positions are visible one a monitor (warn! deselect)
                bool allVisible = true;
                foreach (MyIconInfo mii in icons) {
                    bool vis = false;
                    foreach (Screen s in Screen.AllScreens) {
                        if (s.Bounds.Contains(mii.x, mii.y)) {
                            vis = true;
                            break;
                        }
                    }
                    if (!vis) {
                        allVisible = false;
                        break;
                    }
                }
                if (!allVisible) {
                    if (MessageBox.Show(
                            "Warning: The position for at least one icon is "
                            + "not placed on the visible desktop.\nDo you "
                            + "want to continue?", "Desktop Icon Backup",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2)
                                != DialogResult.Yes) {
                        this.endAction(true);
                        return;
                    }
                }

                // check if targeted positions are must be changed due to the grid option (warn! continue)
                if (useGrid) {
                    DialogResult rs = MessageBox.Show(
                        "Warning: The snap to grid option is activated. This "
                        + "may be incompatible with the icon positions to be "
                        + "restored.\nDo you want to deactivate the snap to "
                        + "grid option?", "Desktop Icon Backup",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button3);
                    if (rs == DialogResult.Yes) {
                        SendMessage(hWnd, LVM_SETEXTENDEDLISTVIEWSTYLE,
                            LVS_EX_SNAPTOGRID, 0);
                        rs = MessageBox.Show("Do you want to reactivate the "
                            + "grid option after the icon positions are "
                            + "restored? (This may alter the icon positions "
                            + "in an unwanted way.)", "Desktop Icon Backup",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (rs != DialogResult.Yes) {
                            useGrid = false;
                        }
                    } else if (rs == DialogResult.No) {
                        useGrid = false; // continue without action
                    } else {
                        this.endAction(true);
                        return;
                    }
                }

                // check that none of the icons overlap (error! return)
                int c = icons.Count;
                float dx, dy, minD;
                minD = (float)Math.Max(SystemInformation.IconSize.Width,
                    SystemInformation.IconSize.Height);
                minD *= minD * 2.0f;
                bool overlapp = false;
                for (int i = 0; i < c; i++) {
                    for (int j = i + 1; j < c; j++) {
                        dx = (float)icons[i].x - (float)icons[j].x;
                        dy = (float)icons[i].y - (float)icons[j].y;
                        dx = dx * dx + dy * dy;
                        if (dx < minD) {
                            overlapp = true;
                        }
                    }
                }
                if (overlapp) {
                    if (MessageBox.Show("Warning: If you continue some icons "
                            + "may overlap each other.\nDo you want to "
                            + "continue?", "Desktop Icon Backup",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2)
                                != DialogResult.Yes) {
                        this.endAction(true);
                        return;
                    }
                }

                // precheck: disable auto align (warn! change)
                UInt32 style = GetWindowLong(hWnd, GWL_STYLE);
                if ((style & LVS_AUTOARRANGE) == LVS_AUTOARRANGE) {
                    if (IDM_TOGGLEAUTOARRANGE != 0) {
                        if (MessageBox.Show("Problem: Desktop icon auto "
                                + "arrange is activated. If you continue DIB "
                                + "will deactivate this option.\nDo you want "
                                + "to deactivate auto arrange and continue?",
                                "Desktop Icon Backup",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button2)
                                    == DialogResult.Yes) {
                            IntPtr parent = GetParent(hWnd);
                            SendMessage(parent, WM_COMMAND,
                                IDM_TOGGLEAUTOARRANGE, 0);
                        } else {
                            this.endAction(true);
                            return;
                        }
                    } else {
                        MessageBox.Show("Auto arrange is activated for the "
                            + "desktop icons. You must deactivate this "
                            + "option.", "Desktop Icon Backup",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // Okey, I admit: I am just to lazy to implement retry
                        this.endAction(true);
                        return;
                    }
                }

                // move icons
                // Again, we need memory inside the desktop explorers process
                uint processID = 0;
                uint threadID = GetWindowThreadProcessId(hWnd, out processID);
                uint pointSize = 8;
                uint titleLength = 1024;
                uint titleRequestSize = titleLength
                    + (uint)Marshal.SizeOf(typeof(LVITEM));
                uint maxMemorySize = Math.Max(pointSize, titleRequestSize);
                IntPtr process = OpenProcess(ProcessAccessFlags.VMOperation
                    | ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite,
                    false, processID);
                IntPtr sharedMem = VirtualAllocEx(process, IntPtr.Zero,
                    maxMemorySize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
                int count = SendMessage(hWnd, LVM_GETITEMCOUNT, 0, 0);
                if (sharedMem == IntPtr.Zero) {
                    throw new Exception("Unable to allocate virtual memory.");
                }
                // now search the icon index for each icon, based on its title
                MyIconInfo iconinfo = new MyIconInfo();
                foreach (MyIconInfo icon in icons) {
                    //POINT pt = new POINT();
                    char[] title = new char[titleRequestSize];
                    LVITEM lvItem = new LVITEM();
                    IntPtr outSize;
                    // fetch icons
                    for (int idx = 0; idx < count; idx++) {
                        iconinfo.title = String.Empty;
                        iconinfo.x = iconinfo.y = 0;

                        // fetch icon title and image id
                        for (int id = 0; id < titleRequestSize; id++) {
                            title[id] = '\0';
                        }
                        lvItem.iItem = idx;
                        lvItem.iSubItem = 0;
                        lvItem.mask = ListViewItemFlags.LVIF_TEXT;
                        lvItem.cchTextMax = (int)titleLength;
                        lvItem.pszText = (IntPtr)((int)sharedMem
                            + Marshal.SizeOf(typeof(LVITEM)));

                        WriteProcessMemory(process, sharedMem, title,
                            titleRequestSize, out outSize);
                        WriteProcessMemory(process, sharedMem, ref lvItem,
                            (uint)Marshal.SizeOf(typeof(LVITEM)), out outSize);
                        if (SendMessage(hWnd, LVM_GETITEMW, idx, sharedMem)
                                != 0) {
                            ReadProcessMemory(process, sharedMem, out lvItem,
                                (uint)Marshal.SizeOf(typeof(LVITEM)),
                                out outSize);
                            for (int id = 0;
                                    id < Marshal.SizeOf(typeof(LVITEM));
                                    id++) {
                                title[id] = 'X';
                            }
                            StringBuilder iconTitle
                                = new StringBuilder((int)titleRequestSize);
                            WriteProcessMemory(process, sharedMem, title,
                                (uint)Marshal.SizeOf(typeof(LVITEM)),
                                out outSize);
                            ReadProcessMemory(process, sharedMem, iconTitle,
                                (int)Math.Min(titleRequestSize,
                                iconTitle.MaxCapacity), out outSize);
                            iconinfo.title = iconTitle.ToString().Substring(
                                Marshal.SizeOf(typeof(LVITEM)) / 2);
                        }

                        if (iconinfo.title.Equals(icon.title,
                                StringComparison.CurrentCultureIgnoreCase)) {
                            // found! => move!

                            // FIX: we store global coordinates, which now need to be converted to local coordinates
                            POINT pt = new POINT();
                            pt.X = icon.x;
                            pt.Y = icon.y;
                            ScreenToClient(hWnd, ref pt);

                            IntPtr lparam = (IntPtr)((pt.Y << 16)
                                | (pt.X & 0xffff));
                            SendMessage(hWnd, LVM_SETITEMPOSITION, idx,
                                lparam);
                            break;
                        }
                    }
                }

                // free the foreign process memory
                VirtualFreeEx(process, sharedMem, maxMemorySize, MEM_RELEASE);
                CloseHandle(process);

                // reactivate icon grid
                if (useGrid) {
                    SendMessage(hWnd, LVM_SETEXTENDEDLISTVIEWSTYLE,
                        LVS_EX_SNAPTOGRID, LVS_EX_SNAPTOGRID);
                }

                // inform the windows system about the changes
                ChangeDisplaySettings(IntPtr.Zero, 0);

                this.filename = this.openFileDialog.FileName;
                this.endAction(false);

            } catch (Exception ex) {
                MessageBox.Show("Failed: " + ex.ToString(),
                    "Desktop Icon Backup", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.endAction(true);
            }
        }

        #endregion

    }
}
