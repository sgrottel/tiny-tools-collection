using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Dib {

    /// <summary>
    /// The DIB main form
    /// </summary>
    public partial class DIBForm : Form {

        /// <summary>
        /// Ctor.
        /// </summary>
        public DIBForm() {
            InitializeComponent();
            this.Icon = global::Dib.Properties.Resources.DibIconVista;

            IDM_TOGGLEAUTOARRANGE = 0;
            try {
                this.FindToggleAutoArrangeID();
            } catch {
                IDM_TOGGLEAUTOARRANGE = 0;
            }

            int r = this.labelCopyright.Right;
            System.Reflection.Assembly i = System.Reflection.Assembly.GetAssembly(this.GetType());
            foreach (Object obj in i.GetCustomAttributes(
                    typeof(System.Reflection.AssemblyCopyrightAttribute), true)) {
                System.Reflection.AssemblyCopyrightAttribute copyright 
                    = obj as System.Reflection.AssemblyCopyrightAttribute;
                if (copyright != null) {
                    this.labelCopyright.Text = "Version: " + Application.ProductVersion 
                        + "  -  " + copyright.ToString();
                }
            }
            this.labelCopyright.Left = r - this.labelCopyright.Width;
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
        /// my home website link has been clicked => open link!
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Label l = sender as Label;
            System.Diagnostics.Process.Start(l.Text);
        }

        /// <summary>
        /// auto-refresh on first shown
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void DIBForm_Shown(object sender, EventArgs e) {
            this.refreshListButton_Click(sender, e);
        }

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
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,  IntPtr windowTitle);

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

        #endregion

        /// <summary>
        /// MyIconInfo
        /// </summary>
        struct MyIconInfo {
            public String title;
            public Point position;
            public int imageId;
        }

        /// <summary>
        /// GetDesktopListViewHandle
        /// </summary>
        /// <returns>The handle of the desktop ListView</returns>
        static private IntPtr GetDesktopListViewHandle() {
            IntPtr hWnd = IntPtr.Zero;
            hWnd = FindWindow("Progman", IntPtr.Zero);
            if (hWnd != IntPtr.Zero) hWnd = FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);
            if (hWnd != IntPtr.Zero) hWnd = FindWindowEx(hWnd, IntPtr.Zero, "SysListView32", IntPtr.Zero);
            return hWnd;
        }

        /// <summary>
        /// GetDesktopIcons
        /// </summary>
        /// <returns>A list of all icons on the desktop</returns>
        static private List<MyIconInfo> GetDesktopIcons() {
            List<MyIconInfo> retval = new List<MyIconInfo>();
            MyIconInfo iconinfo;

            // enumerate all elements on the desktop including their position 
            IntPtr hWnd = GetDesktopListViewHandle();
            if (hWnd != IntPtr.Zero) {
                bool autoArrange = false;

                // we found the desktop list view control

                UInt32 style = GetWindowLong(hWnd, GWL_STYLE);
                if ((style & LVS_AUTOARRANGE) == LVS_AUTOARRANGE) {
                    if (IDM_TOGGLEAUTOARRANGE != 0) {
                        autoArrange = true;
                        IntPtr parent = GetParent(hWnd);
                        SendMessage(parent, WM_COMMAND, IDM_TOGGLEAUTOARRANGE, 0);
                    }
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

                    // fetch icons
                    int count = SendMessage(hWnd, LVM_GETITEMCOUNT, 0, 0);
                    for (int idx = 0; idx < count; idx++) {
                        String titleString = "Unknown Icon " + idx.ToString();
                        iconinfo.title = titleString;
                        iconinfo.position = Point.Empty;
                        iconinfo.imageId = 0;

                        // fetch icon title and image id
                        for (int i = 0; i < titleRequestSize; i++) title[i] = '\0';
                        lvItem.iItem = idx;
                        lvItem.iSubItem = 0;
                        lvItem.mask = ListViewItemFlags.LVIF_TEXT | ListViewItemFlags.LVIF_IMAGE;
                        lvItem.cchTextMax = (int)titleLength;
                        lvItem.pszText = (IntPtr)((int)sharedMem + Marshal.SizeOf(typeof(LVITEM)));

                        WriteProcessMemory(process, sharedMem, title, titleRequestSize, out outSize);
                        WriteProcessMemory(process, sharedMem, ref lvItem, (uint)Marshal.SizeOf(typeof(LVITEM)), out outSize);
                        if (SendMessage(hWnd, LVM_GETITEMW, idx, sharedMem) != 0) {
                            ReadProcessMemory(process, sharedMem, out lvItem, (uint)Marshal.SizeOf(typeof(LVITEM)), out outSize);
                            iconinfo.imageId = lvItem.iImage;
                            for (int i = 0; i < Marshal.SizeOf(typeof(LVITEM)); i++) title[i] = 'X';
                            StringBuilder iconTitle = new StringBuilder((int)titleRequestSize);
                            WriteProcessMemory(process, sharedMem, title, (uint)Marshal.SizeOf(typeof(LVITEM)), out outSize);
                            ReadProcessMemory(process, sharedMem, iconTitle, (int)Math.Min(titleRequestSize, iconTitle.MaxCapacity), out outSize);
                            titleString = iconTitle.ToString().Substring(Marshal.SizeOf(typeof(LVITEM)) / 2);
                        }

                        iconinfo.title = titleString;

                        // fetch icon position
                        WriteProcessMemory(process, sharedMem, ref pt, pointSize, out outSize);
                        if (SendMessage(hWnd, LVM_GETITEMPOSITION, idx, sharedMem) != 0) {
                            ReadProcessMemory(process, sharedMem, out pt, pointSize, out outSize);
                            iconinfo.position = (System.Drawing.Point)pt;
                        }

                        if (iconinfo.position != Point.Empty) {
                            retval.Add(iconinfo);
                        }
                    }

                    // free the foreign process memory 
                    VirtualFreeEx(process, sharedMem, maxMemorySize, MEM_RELEASE);
                    CloseHandle(process);

                } else {
                    MessageBox.Show("Unable to allocate virtual memory.", "Desktop Icon Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (autoArrange) {
                    IntPtr parent = GetParent(hWnd);
                    SendMessage(parent, WM_COMMAND, IDM_TOGGLEAUTOARRANGE, 0);
                }
            }

            return retval;
        }

        /// <summary>
        /// Refresh the iconListView
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void refreshListButton_Click(object sender, EventArgs e) {
            this.iconListView.BeginUpdate();

            foreach (ListViewItem item in this.iconListView.Items) {
                while (item.SubItems.Count < 3) {
                    ListViewItem.ListViewSubItem si = item.SubItems.Add("Not Found");
                    si.Tag = null;
                }
                item.SubItems[1].Text = "Not Found";
                item.SubItems[1].Tag = null;
                item.Checked = false;
            }

            List<MyIconInfo> icons = DIBForm.GetDesktopIcons();
            foreach (MyIconInfo icon in icons) {
                ListViewItem item = null;
                foreach (ListViewItem i in this.iconListView.Items) {
                    if (i.Text.Equals(icon.title, StringComparison.InvariantCultureIgnoreCase)) {
                        item = i;
                        break;
                    }
                }
                if (item == null) {
                    item = this.iconListView.Items.Add(icon.title);
                }

                while (item.SubItems.Count < 3) {
                    ListViewItem.ListViewSubItem si = item.SubItems.Add("Not Found");
                    si.Tag = null;
                }
                item.SubItems[1].Text = icon.position.ToString();
                item.SubItems[1].Tag = icon.position;

                item.Checked = true;
                item.Tag = item.Checked;
            }

            this.iconListView.EndUpdate();
        }

        /// <summary>
        /// item check lock update flag
        /// </summary>
        private bool lockItemCheckedUpdates = false;

        /// <summary>
        /// an item has been checked.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void iconListView_ItemChecked(object sender, ItemCheckedEventArgs e) {
            if (this.lockItemCheckedUpdates) return;
            this.lockItemCheckedUpdates = true;
            
            foreach (ListViewItem item in this.iconListView.Items) {
                e.Item.Tag = e.Item.Checked;
            }

            int selCnt = this.iconListView.CheckedIndices.Count;
            int allCnt = this.iconListView.Items.Count;

            this.selectCheckBox.ThreeState = ((selCnt % allCnt) != 0);

            if (selCnt == 0) {
                this.selectCheckBox.CheckState = CheckState.Unchecked;
            } else if (selCnt < allCnt) {
                this.selectCheckBox.CheckState = CheckState.Indeterminate;
            } else {
                this.selectCheckBox.CheckState = CheckState.Checked;
            }
            this.lockItemCheckedUpdates = false;
        }

        /// <summary>
        /// selection checkbox clicked
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void selectCheckBox_Click(object sender, EventArgs e) {
            if (this.lockItemCheckedUpdates) return;
            this.lockItemCheckedUpdates = true;

            switch (this.selectCheckBox.CheckState) {
                case CheckState.Checked:
                    foreach (ListViewItem item in this.iconListView.Items) {
                        item.Checked = true;
                    }
                    break;
                case CheckState.Unchecked:
                    foreach (ListViewItem item in this.iconListView.Items) {
                        item.Checked = false;
                    }
                    break;
                case CheckState.Indeterminate:
                    foreach (ListViewItem item in this.iconListView.Items) {
                        item.Checked = (bool)item.Tag;
                    }
                    break;
            }

            this.lockItemCheckedUpdates = false;
        }

        /// <summary>
        /// store button
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void storeButton_Click(object sender, EventArgs e) {
            foreach (ListViewItem item in this.iconListView.CheckedItems) {
                try {
                    Point pt = (Point)item.SubItems[1].Tag;
                    item.SubItems[2].Tag = pt;
                    item.SubItems[2].Text = pt.ToString();
                } catch { }
            }
        }

        /// <summary>
        /// store the icon positions as xml file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void saveButton_Click(object sender, EventArgs e) {
            this.saveFileDialog.FileName = this.openFileDialog.FileName;
            if (this.saveFileDialog.ShowDialog() == DialogResult.OK) {
                this.openFileDialog.FileName = this.saveFileDialog.FileName;

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(this.saveFileDialog.FileName)) {
                    writer.WriteLine("<xml type=\"dib\">");
                    foreach (ListViewItem item in this.iconListView.CheckedItems) {
                        try {
                            Point pt = (Point)item.SubItems[2].Tag;
                            writer.Write("\t");
                            writer.Write("<icon name=\"" + item.Text + "\" position=\"" + pt.X.ToString() + ";" + pt.Y.ToString() + "\"/>");
                            writer.WriteLine();
                        } catch { }
                    }
                    writer.WriteLine("</xml>");
                }
            }
        }

        /// <summary>
        /// load a desktop icon backup from xml file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void loadButton_Click(object sender, EventArgs e) {
            if (this.openFileDialog.ShowDialog() == DialogResult.OK) {

                String error = null;
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^-?\d+;-?\d+$");
                List<String> names = new List<String>();
                List<Point> points = new List<Point>();

                foreach (ListViewItem item in this.iconListView.Items) {
                    while (item.SubItems.Count < 3) {
                        ListViewItem.ListViewSubItem si = item.SubItems.Add("Not Found");
                        si.Tag = null;
                    }
                    item.SubItems[2].Text = "Not Found";
                    item.SubItems[2].Tag = null;
                    item.Checked = false;
                }

                using (System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(this.openFileDialog.FileName)) {
                    while (reader.Read()) {
                        if (reader.NodeType != System.Xml.XmlNodeType.Element) continue; // only element nodes are considdered useful
                        if (!reader.HasAttributes) continue; // elements without attributes are ignored
                        if (reader.Depth == 0) {
                            if (!reader.Name.Equals("xml", StringComparison.InvariantCultureIgnoreCase)) {
                                error = "Invalid File: Base tag must be \"xml\"";
                                break;
                            }
                            String type = reader.GetAttribute("type");
                            if (type == null || !type.Equals("dib", StringComparison.InvariantCultureIgnoreCase)) {
                                error = "Invalid File: Xml-File must be of type \"dib\"";
                                break;
                            }
                        } else if (reader.Depth == 1) {
                            if (!reader.Name.Equals("icon", StringComparison.InvariantCultureIgnoreCase)) continue; // only icon elements are considdered useful
                            String name = reader.GetAttribute("name");
                            String pos = reader.GetAttribute("position");
                            if (name == null || pos == null) {
                                error = "Invalid File: Invalid Icon Tag found";
                                break;
                            }

                            Point pt = new Point();
                            try {
                                if (regex.IsMatch(pos)) {
                                    String[] strings = pos.Split(';');
                                    pt.X = int.Parse(strings[0]);
                                    pt.Y = int.Parse(strings[1]);
                                }
                            } catch { 
                                continue; // unable to parse icon position
                            }

                            names.Add(name);
                            points.Add(pt);

                        }
                    }

                    if (error != null) {
                        error = "Error [Line " + reader.LineNumber.ToString() + "]: " + error;
                        MessageBox.Show(error, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                // no error so now set the loaded icon positions 
                if (error == null) {
                    int len = names.Count;
                    for (int i = 0; i < len; i++) {
                        String name = names[i];
                        Point pt = points[i];
                        ListViewItem item = null;

                        item = null;
                        foreach (ListViewItem tlvi in this.iconListView.Items) {
                            if (tlvi.Text.Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
                                item = tlvi;
                                break;
                            }
                        }
                        if (item == null) {
                            item = this.iconListView.Items.Add(name);
                        }

                        while (item.SubItems.Count < 3) {
                            ListViewItem.ListViewSubItem si = item.SubItems.Add("Not Found");
                            si.Tag = null;
                        }
                        item.SubItems[2].Text = pt.ToString();
                        item.SubItems[2].Tag = pt;

                        item.Checked = true;
                        item.Tag = true;
                    }

                }

            }
        }

        /// <summary>
        /// restore the positions of the selected icons
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The arguments of the event</param>
        private void restoreButton_Click(object sender, EventArgs e) {
            IntPtr hWnd = GetDesktopListViewHandle();
            if (hWnd == IntPtr.Zero) {
                MessageBox.Show("Unable to access Desktop", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (this.iconListView.CheckedItems.Count == 0) {
                MessageBox.Show("You must select (check) the Icons you want to restore.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // get current desktop icons
            List<MyIconInfo> icons = DIBForm.GetDesktopIcons();

            // get the info whether to use the icon grid!
            bool useGrid = false;
            int extStyle = SendMessage(hWnd, LVM_GETEXTENDEDLISTVIEWSTYLE, 0, 0);
            if ((extStyle & LVS_EX_SNAPTOGRID) != 0) {
                useGrid = true;
            }

            // check the selected icons and unselect those without restorable position (warn! deselect)
            int goodcount = 0;
            int badcount = 0;
            foreach (ListViewItem i in this.iconListView.CheckedItems) {
                try {
                    Point p = (Point)i.SubItems[2].Tag;
                    goodcount++;
                } catch {
                    badcount++;
                }
            }
            if (badcount > 0) {
                if (goodcount == 0) {
                    MessageBox.Show("None of the selected Icons has a restorable position.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                } else {
                    if (MessageBox.Show("Some of the selected Icons do not have a restorable position.\nDo you want to deselect these icons and continue restoring the positions of the remaining selection?",
                        this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                        foreach (ListViewItem i in this.iconListView.CheckedItems) {
                            try {
                                Point p = (Point)i.SubItems[2].Tag;
                            } catch {
                                i.Checked = false;
                            }
                        }

                    } else {
                        return;
                    }
                }
            }

            // check that the targeted positions are visible one a monitor (warn! deselect)
            {
                bool allVisible = true;
                List<MyIconInfo> after = icons;
                int i;
                foreach (ListViewItem it in this.iconListView.CheckedItems) {
                    for (i = 0; i < after.Count; i++) {
                        if (after[i].title.Equals(it.SubItems[0].Text, StringComparison.InvariantCultureIgnoreCase)) {
                            MyIconInfo tmp = after[i];
                            tmp.position = (Point)it.SubItems[2].Tag;
                            after[i] = tmp;
                            break;
                        }
                    }
                }

                foreach (MyIconInfo ic in after) {
                    bool vis = false;
                    foreach (Screen s in Screen.AllScreens) {
                        if (s.Bounds.Contains(ic.position)) {
                            vis = true;
                            break;
                        }
                    }
                    if (!vis) allVisible = false;
                }

                if (!allVisible) {
                    if (MessageBox.Show("Warning: The targeted position for at least one icon is not placed on the visible desktop.\nDo you want to continue?",
                           this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) {
                        return;
                    }
                }
            }

            // check if targeted positions are must be changed due to the grid option (warn! continue)
            if (useGrid) {
                DialogResult rs = MessageBox.Show("Warning: The snap to grid option is activated. This may be incompatible with the icon positions to be restored.\nDo you want to deactivate the snap to grid option?",
                    this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);
                if (rs == DialogResult.Yes) {
                    SendMessage(hWnd, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_SNAPTOGRID, 0);
                    rs = MessageBox.Show("Do you want to reactivate the grid option after the icon positions are restored? (This may alter the icon positions in an unwanted way.)",
                        this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (rs != DialogResult.Yes) {
                        useGrid = false;
                    }
                } else if (rs == DialogResult.No) {
                    useGrid = false; // continue without action
                } else {
                    return;
                }
            }

            // check that none of the icons overlap (error! return)
            {
                List<MyIconInfo> after = icons;
                bool overlapp = false;

                int i, j, c;
                float dx, dy, minD;
                c = after.Count;
                minD = (float)Math.Max(SystemInformation.IconSize.Width, SystemInformation.IconSize.Height);
                minD *= minD * 2.0f;

                foreach (ListViewItem it in this.iconListView.CheckedItems) {
                    for (i = 0; i < c; i++) {
                        if (after[i].title.Equals(it.SubItems[0].Text, StringComparison.InvariantCultureIgnoreCase)) {
                            MyIconInfo tmp = after[i];
                            tmp.position = (Point)it.SubItems[2].Tag;
                            after[i] = tmp;
                            break;
                        }
                    }
                }

                for (i = 0; i < c; i++) {
                    for (j = i + 1; j < c; j++) {
                        dx = (float)after[i].position.X - (float)after[j].position.X;
                        dy = (float)after[i].position.Y - (float)after[j].position.Y;
                        dx = dx * dx + dy * dy;
                        if (dx < minD) {
                            overlapp = true;
                        }
                    }
                }

                if (overlapp) {
                    if (MessageBox.Show("Warning: If you continue some icons may overlap each other.\nDo you want to continue?",
                           this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) {
                        return;
                    }
                }

            }

            // precheck: disable auto align (warn! change)
            UInt32 style = GetWindowLong(hWnd, GWL_STYLE);
            if ((style & LVS_AUTOARRANGE) == LVS_AUTOARRANGE) {
                if (IDM_TOGGLEAUTOARRANGE != 0) {
                    if (MessageBox.Show("Problem: Desktop icon auto arrange is activated. If you continue DIB will deactivate this option.\n" +
                            "Do you want to deactivate auto arrange and continue?", this.Text,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                        IntPtr parent = GetParent(hWnd);
                        SendMessage(parent, WM_COMMAND, IDM_TOGGLEAUTOARRANGE, 0);
                    } else {
                        return;
                    }
                } else {
                    MessageBox.Show("Auto arrange is activated for the desktop icons. You must deactivate this option to proceed",
                        this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // move icons
            uint processID = 0;
            uint threadID = GetWindowThreadProcessId(hWnd, out processID);

            uint pointSize = 8;
            uint titleLength = 1024;
            uint titleRequestSize = titleLength + (uint)Marshal.SizeOf(typeof(LVITEM));

            uint maxMemorySize = Math.Max(pointSize, titleRequestSize);

            IntPtr process = OpenProcess(ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite, false, processID);
            IntPtr sharedMem = VirtualAllocEx(process, IntPtr.Zero, maxMemorySize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
            int count = SendMessage(hWnd, LVM_GETITEMCOUNT, 0, 0);
            if (sharedMem != IntPtr.Zero) {
                MyIconInfo iconinfo;
                foreach (ListViewItem i in this.iconListView.CheckedItems) {
                    //POINT pt = new POINT();
                    char[] title = new char[titleRequestSize];
                    LVITEM lvItem = new LVITEM();
                    IntPtr outSize;

                    // fetch icons
                    for (int idx = 0; idx < count; idx++) {
                        String titleString = "Unknown Icon " + idx.ToString();
                        iconinfo.title = titleString;
                        iconinfo.position = Point.Empty;
                        iconinfo.imageId = 0;

                        // fetch icon title and image id
                        for (int id = 0; id < titleRequestSize; id++) title[id] = '\0';
                        lvItem.iItem = idx;
                        lvItem.iSubItem = 0;
                        lvItem.mask = ListViewItemFlags.LVIF_TEXT | ListViewItemFlags.LVIF_IMAGE;
                        lvItem.cchTextMax = (int)titleLength;
                        lvItem.pszText = (IntPtr)((int)sharedMem + Marshal.SizeOf(typeof(LVITEM)));

                        WriteProcessMemory(process, sharedMem, title, titleRequestSize, out outSize);
                        WriteProcessMemory(process, sharedMem, ref lvItem, (uint)Marshal.SizeOf(typeof(LVITEM)), out outSize);
                        if (SendMessage(hWnd, LVM_GETITEMW, idx, sharedMem) != 0) {
                            ReadProcessMemory(process, sharedMem, out lvItem, (uint)Marshal.SizeOf(typeof(LVITEM)), out outSize);
                            iconinfo.imageId = lvItem.iImage;
                            for (int id = 0; id < Marshal.SizeOf(typeof(LVITEM)); id++) title[id] = 'X';
                            StringBuilder iconTitle = new StringBuilder((int)titleRequestSize);
                            WriteProcessMemory(process, sharedMem, title, (uint)Marshal.SizeOf(typeof(LVITEM)), out outSize);
                            ReadProcessMemory(process, sharedMem, iconTitle, (int)Math.Min(titleRequestSize, iconTitle.MaxCapacity), out outSize);
                            titleString = iconTitle.ToString().Substring(Marshal.SizeOf(typeof(LVITEM)) / 2);
                        }

                        if (titleString.Equals(i.SubItems[0].Text, StringComparison.InvariantCultureIgnoreCase)) {
                            // found!
                            Point p = (Point)i.SubItems[2].Tag;
                            IntPtr lparam = (IntPtr)((p.Y << 16) | (p.X & 0xffff));
                            SendMessage(hWnd, LVM_SETITEMPOSITION, idx, lparam);
                            break;
                        }
                    }
                }

                // free the foreign process memory
                VirtualFreeEx(process, sharedMem, maxMemorySize, MEM_RELEASE);
                CloseHandle(process);
            } else {
                MessageBox.Show("Unable to allocate virtual memory.", "Desktop Icon Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // reactivate icon grid
            if (useGrid) {
                SendMessage(hWnd, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_SNAPTOGRID, LVS_EX_SNAPTOGRID);
            }

            // refresh screen coordinates
            this.refreshListButton_Click(null, null);

            // inform the windows system about the changes
            ChangeDisplaySettings(IntPtr.Zero, 0);
            // TODO: how?

        }

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(IntPtr devMode, int flags);

        /// <summary>
        /// finds the correct value for IDM_TOGGLEAUTOARRANGE
        /// </summary>
        private void FindToggleAutoArrangeID() {
            // This is very unofficial!
            // The value of 'IDM_TOGGLEAUTOARRANGE' seams to be OS-Version dependent
            //  Win98:  7041
            //  WinNT4: 7031
            //  Else:   7051
            if (System.Environment.OSVersion.Platform == PlatformID.Win32Windows) {
                IDM_TOGGLEAUTOARRANGE = 0x7041;
            } else if (System.Environment.OSVersion.Platform == PlatformID.Win32NT) {
                if (System.Environment.OSVersion.Version.Major == 4) {
                    IDM_TOGGLEAUTOARRANGE = 0x7031;
                } else {
                    IDM_TOGGLEAUTOARRANGE = 0x7051;
                }
            } else {
                throw new Exception("Unsupported Operating System");
            }
        }
    }
}