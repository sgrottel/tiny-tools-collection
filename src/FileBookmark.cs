using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace FileBookmark {

    static class Program {

        /// <summary>
        /// The file name extension
        /// </summary>
        private const string Extension = ".bookmark";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The command line arguments</param>
        [STAThread]
        static void Main(string[] args) {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if ((args != null) && (args.Length > 0)) {
                string op;
                int state = 0;
                foreach (string arg in args) {
                    op = "Unknown Operation";
                    try {

                        if (state == 1) {
                            // mark this file
                            op = "Bookmarking file (" + arg + ")";
                            addBookmark(arg);

                            state = 0;
                        } else if (state == 2) {
                            // remove this bookmark file
                            op = "Removing Bookmark (" + arg + ")";
                            removeBookmark(arg);

                            state = 0;
                        } else {
                            if (arg.Equals("-REG", StringComparison.CurrentCultureIgnoreCase)) {
                                op = "Registering Application";
                                // registers this application
                                if (Elevation.IsElevated()) {
                                    registerApplication();

                                } else {
                                    Elevation.RestartElevated("-REG");
                                }

                            }
                            if (arg.Equals("-UNREG", StringComparison.CurrentCultureIgnoreCase)) {
                                op = "Unregistering Application";
                                // un-registers this application
                                if (Elevation.IsElevated()) {
                                    unregisterApplication();

                                } else {
                                    Elevation.RestartElevated("-UNREG");
                                }

                            }
                            if (arg.Equals("-MARK", StringComparison.CurrentCultureIgnoreCase)) {
                                // mark the file
                                state = 1;
                            }
                            if (arg.Equals("-REMOVE", StringComparison.CurrentCultureIgnoreCase)) {
                                // removes the file
                                state = 2;
                            }
                        }

                    } catch (Exception ex) {
                        DialogResult result = MessageBox.Show(op + " failed: " + ex.ToString(),
                            Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                        if (result == DialogResult.Cancel) break;
                    }
                }

            } else {

                if (MessageBox.Show("This application is not to be called directly. Do you want to (re-)register the application for use from the Explorer context menu?",
                        Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                    if (Elevation.IsElevated()) {
                        registerApplication();

                    } else {
                        Elevation.RestartElevated("-REG");
                    }
                }

            }

        }

        /// <summary>
        /// Bookmarks a file moving another bookmark within this directory
        /// </summary>
        /// <param name="filename">The file of the bookmark file</param>
        private static void addBookmark(string filename) {
            if (!File.Exists(filename)) {
                throw new Exception("File does not seem to exist");
            }
            if (Path.GetExtension(filename).Equals(Extension, StringComparison.CurrentCultureIgnoreCase)) {
                throw new Exception("You cannot bookmark a bookmark file");
            }
            if (File.Exists(filename + Extension)) {
                // file already bookmarked
                return;
            }

            string path = Path.GetDirectoryName(filename);
            string[] files = Directory.GetFiles(path, "*" + Extension);

            if ((files == null) || (files.Length == 0)) {
                // no other bookmarks, so we are good!
            } else {
                // remove other bookmarks ... next program version may be able to handle multiple bookmarks with a directory
                foreach (string file in files) {
                    try {
                        removeBookmark(file);
                    } catch (Exception ex) {
                        DialogResult result = MessageBox.Show("Removing Bookmark (" + file + ") failed: " + ex.ToString(),
                            Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                        if (result == DialogResult.Cancel) return;
                    }
                }
            }

            File.Create(filename + Extension).Close();

        }

        /// <summary>
        /// Removes a bookmark file
        /// </summary>
        /// <param name="filename">The file of the bookmark file</param>
        private static void removeBookmark(string filename) {
            if (!File.Exists(filename)) {
                throw new Exception("File does not seem to exist");
            }

            if (!Path.GetExtension(filename).Equals(Extension, StringComparison.CurrentCultureIgnoreCase)) {
                if (File.Exists(filename + Extension)) {
                    removeBookmark(filename + Extension); // remove bookmark
                    return;
                } else {
                    throw new Exception("File does not seem to be bookmarked");
                }
            }

            FileInfo info = new FileInfo(filename);
            if (info == null) {
                throw new Exception("Unable to access file");
            }
            if (info.Length != 0) {
                throw new Exception("File is not empty. Does not seem to be a valid File Bookmark");
            }

            File.Delete(filename);

        }

        /// <summary>
        /// Registers this application in the windows registry
        /// </summary>
        private static void registerApplication() {
            if (!Elevation.IsElevated()) {
                throw new Exception("Rights elevation required");
            }

            RegistryKey file = null;
            RegistryKey shell = null;
            RegistryKey verb = null;
            RegistryKey command = null;

            file = Registry.ClassesRoot.OpenSubKey("*", true);
            try {
                shell = file.CreateSubKey("shell");
                verb = shell.CreateSubKey("bookmark");
                verb.SetValue(string.Empty, "Set File Bookmark");
                command = verb.CreateSubKey("command");
                command.SetValue(string.Empty, string.Format("\"{0}\" -mark \"%1\"", Application.ExecutablePath));

            } finally {
                if (command != null) { command.Close(); command = null; }
                if (verb != null) { verb.Close(); verb = null; }
                if (shell != null) { shell.Close(); shell = null; }
                if (file != null) { file.Close(); file = null; }
            }

            file = Registry.ClassesRoot.CreateSubKey(Extension);
            try {
                file.SetValue(string.Empty, Application.ProductName + Extension);
            } finally {
                if (command != null) { command.Close(); command = null; }
                if (verb != null) { verb.Close(); verb = null; }
                if (shell != null) { shell.Close(); shell = null; }
                if (file != null) { file.Close(); file = null; }
            }

            file = Registry.ClassesRoot.CreateSubKey(Application.ProductName + Extension);
            try {
                file.SetValue(string.Empty, "File Bookmark");

                try {
                    shell = file.CreateSubKey("DefaultIcon");
                    shell.SetValue(string.Empty, Application.ExecutablePath);
                } finally {
                    if (shell != null) { shell.Close(); shell = null; }
                }

                shell = file.CreateSubKey("shell");
                verb = shell.CreateSubKey("remove");
                verb.SetValue(string.Empty, "Remove File Bookmark");
                command = verb.CreateSubKey("command");
                command.SetValue(string.Empty, string.Format("\"{0}\" -remove \"%1\"", Application.ExecutablePath));

            } finally {
                if (command != null) { command.Close(); command = null; }
                if (verb != null) { verb.Close(); verb = null; }
                if (shell != null) { shell.Close(); shell = null; }
                if (file != null) { file.Close(); file = null; }
            }

            MessageBox.Show("File Bookmark application (" + Application.ExecutablePath + ") sucessfully registered",
                Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        /// <summary>
        /// Removes the registry entries of this application from the windows registry
        /// </summary>
        private static void unregisterApplication() {
            if (!Elevation.IsElevated()) {
                throw new Exception("Rights elevation required");
            }

            // TODO: Implement

        }

    }

}
