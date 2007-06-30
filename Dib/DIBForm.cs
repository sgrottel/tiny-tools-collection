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
            // TODO: Find value of IDM_TOGGLEAUTOARRANGE
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
        const uint LVM_GETIMAGELIST = (LVM_FIRST + 2);
        const uint LVM_GETITEMCOUNT = (LVM_FIRST + 4);
        const uint LVM_GETITEMW = (LVM_FIRST + 75);
        const uint LVM_GETITEMTEXTW = (LVM_FIRST + 115);
        const uint LVM_SETITEMPOSITION = (LVM_FIRST + 15);
        const uint LVM_GETITEMPOSITION = (LVM_FIRST + 16);
        const uint LVM_SETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 54);
        const uint LVM_GETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 55);

        const int LVS_EX_SNAPTOGRID = 0x00080000;

        /** GetWindowLong constants */
        const int GWL_STYLE = (-16);
        const int LVSIL_SMALL = 1;

        /** ListView Style constants */
        const UInt32 LVS_AUTOARRANGE = 0x0100;

        /** windows messages */
        const uint WM_COMMAND = 0x0111;

        /** special command message id */
        // const int IDM_TOGGLEAUTOARRANGE = 0x7041;
        const int IDM_TOGGLEAUTOARRANGE = 0x7051;

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
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [MarshalAs(UnmanagedType.LPTStr)]StringBuilder buf, int nSize, out IntPtr lpNumberOfBytesRead);
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

        /** MyIconInfo */
        struct MyIconInfo {
            public String title;
            public Point position;
            public int imageId;
        }

        /** GetDesktopListViewHandle */
        static private IntPtr GetDesktopListViewHandle() {
            IntPtr hWnd = IntPtr.Zero;
            hWnd = FindWindow("Progman", IntPtr.Zero);
            if (hWnd != IntPtr.Zero) hWnd = FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);
            if (hWnd != IntPtr.Zero) hWnd = FindWindowEx(hWnd, IntPtr.Zero, "SysListView32", IntPtr.Zero);
            return hWnd;
        }

        /** GetDesktopIcons */
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

        /** Refresh the iconListView */
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

        /** item check lock update flag */
        private bool lockItemCheckedUpdates = false;

        /** an item has been checked. */
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

        /** selection checkbox clicked */
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

        /** store button */
        private void storeButton_Click(object sender, EventArgs e) {
            foreach (ListViewItem item in this.iconListView.CheckedItems) {
                try {
                    Point pt = (Point)item.SubItems[1].Tag;
                    item.SubItems[2].Tag = pt;
                    item.SubItems[2].Text = pt.ToString();
                } catch { }
            }
        }

        /** store the icon positions as xml file */
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

        /** load a desktop icon backup from xml file */
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

        /** restore the positions of the selected icons */
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

            /** get current desktop icons */
            List<MyIconInfo> icons = DIBForm.GetDesktopIcons();

            /** get the info whether to use the icon grid! */
            bool useGrid = false;
            int extStyle = SendMessage(hWnd, LVM_GETEXTENDEDLISTVIEWSTYLE, 0, 0);
            if ((extStyle & LVS_EX_SNAPTOGRID) != 0) {
                useGrid = true;
            }

            /** check the selected icons and unselect those without restorable position (warn! deselect) */
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

            /** check that the targeted positions are visible one a monitor (warn! deselect) */
            // TODO: Implement

            /** check if targeted positions are must be changed due to the grid option (warn! continue) */
            // TODO: Implement

            /** check that none of the selected icons overlap (error! return) */
            // TODO: Implement

            /** precheck: disable auto align (warn! change) */
            UInt32 style = GetWindowLong(hWnd, GWL_STYLE);
            if ((style & LVS_AUTOARRANGE) == LVS_AUTOARRANGE) {
                if (MessageBox.Show("Problem: Desktop Icon Auto Arrange is activated. If you continue DIB will deactivate this option.\n" +
                        "Do You want to deactivate Auto Arrange and continue?", this.Text,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                    IntPtr parent = GetParent(hWnd);
                    SendMessage(parent, WM_COMMAND, IDM_TOGGLEAUTOARRANGE, 0);
                } else {
                    return;
                }
            }

            /** check if it is necessary to move a not selected icon (warn! continue) */
            // TODO: Implement

            /** deactivate raster */
            if (useGrid) {
                SendMessage(hWnd, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_SNAPTOGRID, 0);
            }

            /** move icons */
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
                            /** found! */
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

            /** reactivate icon grid */
            if (useGrid) {
                SendMessage(hWnd, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_SNAPTOGRID, LVS_EX_SNAPTOGRID);
            }

            /** refresh screen coordinates */
            this.refreshListButton_Click(null, null);
        }
    }
}