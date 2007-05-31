using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Dib {

    /**
     * The DIB main form
     */
    public partial class DIBForm : Form {

        /** ctor */
        public DIBForm() {
            InitializeComponent();
            this.Icon = global::Dib.Properties.Resources.DibIcon;
        }

        /** exit button click event handler */
        private void exitButton_Click(object sender, EventArgs e) {
            this.Close();
        }

        /** my home website link has been clicked => open link! */
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Label l = sender as Label;
            System.Diagnostics.Process.Start(l.Text);
        }

        /** auto-refresh on first shown */
        private void DIBForm_Shown(object sender, EventArgs e) {
            this.refreshListButton_Click(sender, e);
        }

        /** FindWindow Signatur 1/2 */
        // For Windows Mobile, replace user32.dll with coredll.dll
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /** FindWindow Signatur 2/2 */
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, IntPtr lpWindowName);

        /** FindWindowEx Signatur 1/2 */
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        /** FindWindowEx Signatur 2/2 */
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,  IntPtr windowTitle);

        /** SendMessage Signature 1/2 */
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        /** GetWindowLong */
        [DllImport("user32.dll", SetLastError = true)]
        static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);
        
        /** SendMessage Signature 2/2 */
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, ref LVITEM lParam);

        /** GetParent */
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        static extern IntPtr GetParent(IntPtr hWnd);

        /** ListView Messages */
        const uint LVM_FIRST = 0x1000; // ListView messages
        const uint LVM_GETITEMCOUNT = (LVM_FIRST + 4);
        const uint LVM_GETITEMTEXTW = (LVM_FIRST + 115);
        const uint LVM_GETITEMPOSITION = (LVM_FIRST + 16);

        /** GetWindowLong constants */
        const int GWL_STYLE = (-16);

        /** ListView Style constants */
        const UInt32 LVS_AUTOARRANGE = 0x0100;

        /** windows messages */
        const uint WM_COMMAND = 0x0111;

        /** special command message id */
        const int IDM_TOGGLEAUTOARRANGE = 0x7041;

        /** ProcessAccessFlags */
        [Flags] enum ProcessAccessFlags : uint {
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

        /** OpenProcess */
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        /** GetWindowThreadProcessId */
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /** VirtualAllocEx */
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        /** VirtualFreeEx */
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        /** POINT */
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

        /** virtual memory constants */
        const uint MEM_COMMIT = 0x1000;
        const uint MEM_RESERVE = 0x2000;
        const uint MEM_RELEASE = 0x8000;

        const uint PAGE_READWRITE = 0x4;

        /** CloseHandle */
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);

        /** WriteProcessMemory */
        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, char[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref POINT lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref LVITEM lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        /** ReadProcessMemory */
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out char[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out POINT lpBuffer, uint nSize, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out LVITEM lpBuffer, uint nSize, out IntPtr lpNumberOfBytesRead);

        /** ListViewItemFlags */
        public enum ListViewItemFlags {
            LVIF_TEXT = 0x0001,
            LVIF_IMAGE = 0x0002,
            LVIF_PARAM = 0x0004,
            LVIF_STATE = 0x0008,
            LVIF_INDENT = 0x0010,
            LVIF_NORECOMPUTE = 0x0800
        }

        /** LVITEM */
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

        /** Refresh the iconListView */
        private void refreshListButton_Click(object sender, EventArgs e) {
            this.iconListView.BeginUpdate();
            this.iconListView.Items.Clear();

            // enumerate all elements on the desktop including their position 
            IntPtr hWnd = IntPtr.Zero;
            hWnd = FindWindow("Progman", IntPtr.Zero);
            if (hWnd != IntPtr.Zero) hWnd = FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);
            if (hWnd != IntPtr.Zero) hWnd = FindWindowEx(hWnd, IntPtr.Zero, "SysListView32", IntPtr.Zero);
            if (hWnd != IntPtr.Zero) {
                bool autoArrange = false;

                // we found the desktop list view control

                UInt32 style = GetWindowLong(hWnd, GWL_STYLE);
                if ((style & LVS_AUTOARRANGE) == LVS_AUTOARRANGE) {
                    autoArrange = true;
                    IntPtr parent = GetParent(hWnd);
                    SendMessage(parent, WM_COMMAND, IDM_TOGGLEAUTOARRANGE, 0);
                }

                uint processID = 0;
                uint threadID = GetWindowThreadProcessId(hWnd, out processID);
                
                uint pointSize = 8;
                uint titleLength = 1024;
                uint titleRequestSize = titleLength + (uint)Marshal.SizeOf(typeof(LVITEM));
                
                uint maxMemorySize = Math.Max(pointSize, titleRequestSize);

                IntPtr process = OpenProcess(ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite, false, processID);
                IntPtr sharedMem = VirtualAllocEx(process, IntPtr.Zero, maxMemorySize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

                if (sharedMem != IntPtr.Zero) {
                    POINT pt = new POINT();
                    char[] title = new char[titleRequestSize];
                    LVITEM lvItem = new LVITEM();
                    IntPtr outSize;

                    int count = SendMessage(hWnd, LVM_GETITEMCOUNT, 0, 0);
                    for (int idx = 0; idx < count; idx++) {
                        ListViewItem item = this.iconListView.Items.Add(idx.ToString());
                        item.SubItems.Add("Not Found");
                        item.SubItems.Add("Not Found");

                        // fetch icon title
                        for (int i = 0; i < titleRequestSize; i++) title[i] = '\0';
                        lvItem.iItem = idx;
                        lvItem.iSubItem = 0;
                        lvItem.cchTextMax = (int)titleLength;
                        lvItem.pszText = (IntPtr)((int)sharedMem + Marshal.SizeOf(typeof(LVITEM)));

                        WriteProcessMemory(process, sharedMem, title, titleRequestSize, out outSize);
                        WriteProcessMemory(process, sharedMem, ref lvItem, (uint)Marshal.SizeOf(typeof(LVITEM)), out outSize);
                        if (SendMessage(hWnd, LVM_GETITEMTEXTW, idx, sharedMem) != 0) {
                            ReadProcessMemory(process, sharedMem, out lvItem, (uint)Marshal.SizeOf(typeof(LVITEM)), out outSize);
                            byte[] outdata = new byte[titleRequestSize];
                            ReadProcessMemory(process, sharedMem, out outdata, titleRequestSize, out outSize); // Tut nicht ... :-/
                            item.SubItems[0].Text = new String(title);
                        }

                        // fetch icon position
                        WriteProcessMemory(process, sharedMem, ref pt, pointSize, out outSize);
                        if (SendMessage(hWnd, LVM_GETITEMPOSITION, idx, sharedMem) != 0) {
                            ReadProcessMemory(process, sharedMem, out pt, pointSize, out outSize);
                            item.SubItems[1].Tag = (System.Drawing.Point)pt;
                            item.SubItems[1].Text = pt.X.ToString() + "; " + pt.Y.ToString();
                        }

                    }

                    // free the foreign process memory 
                    VirtualFreeEx(process, sharedMem, maxMemorySize, MEM_RELEASE);
                    CloseHandle(process);

                } else {
                    MessageBox.Show("Unable to allocate virtual memory.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (autoArrange) {
                    IntPtr parent = GetParent(hWnd);
                    SendMessage(parent, WM_COMMAND, IDM_TOGGLEAUTOARRANGE, 0);
                }
            }

            this.iconListView.EndUpdate();
        }
    }
}