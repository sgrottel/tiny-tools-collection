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
        internal const string Extension = ".bookmark";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>0 on success</returns>
        [STAThread]
        static int Main(string[] args) {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if ((args != null) && (args.Length > 0)) {
                bool noElev = false;
                bool reg = false;
                bool unreg = false;
                List<string> addBM = new List<string>();
                List<string> removeBM = new List<string>();
                List<string> openBM = new List<string>();

                for (int i = 0; i < args.Length; i++) {
                    if (args[i].Equals("-NOELEV")) {
                        noElev = true;
                    } else if (args[i].Equals("-REG")) {
                        reg = true;
                    } else if (args[i].Equals("-UNREG")) {
                        unreg = true;
                    } else if (args[i].Equals("-MARK")) {
                        i++;
                        if ((i < args.Length) && System.IO.File.Exists(args[i])) {
                            addBM.Add(args[i]);
                        }
                    } else if (args[i].Equals("-UNMARK")) {
                        i++;
                        if ((i < args.Length) && System.IO.File.Exists(args[i])) {
                            removeBM.Add(args[i]);
                        }
                    } else if (args[i].Equals("-OPEN")) {
                        i++;
                        if ((i < args.Length) && System.IO.File.Exists(args[i])) {
                            openBM.Add(args[i]);
                        }
                    } else {
                        if (System.IO.File.Exists(args[i])) {
                            openBM.Add(args[i]);
                        }
                    }
                }

                if (reg || unreg) {
                    if (reg && unreg) {
                        MessageBox.Show(Strings.RegUnregError, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return -2;
                    }
                    if ((addBM.Count > 0) || (removeBM.Count > 0) || (openBM.Count > 0)) {
                        MessageBox.Show(Strings.IgnoreFilesOnRegWarning, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        addBM.Clear();
                        removeBM.Clear();
                        openBM.Clear();
                    }
                    bool suc = false;
                    if (reg) {
                        suc = registerApplication(noElev);
                    }
                    if (unreg) {
                        suc = unregisterApplication(noElev);
                    }
                    if (!noElev) {
                        MessageBox.Show(String.Format(reg ? Strings.RegistrationResult : Strings.UnregistrationResult,
                            suc ? Strings.ResultSuccess : Strings.ResultFailed),
                            Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return suc ? 0 : -3;
                }

                /*string op;
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
                        } else if (state == 3) {
                            // remove this bookmark file
                            op = "Open Bookmark (" + arg + ")";
                            openBookmark(arg);

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
                            if (arg.Equals("-OPEN", StringComparison.CurrentCultureIgnoreCase)) {
                                // removes the file
                                state = 3;
                            }
                        }

                    } catch (Exception ex) {
                        DialogResult result = MessageBox.Show(op + " failed: " + ex.ToString(),
                            Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                        if (result == DialogResult.Cancel) break;
                    }
                }*/

                return -1;

            } else {

                if (MessageBox.Show(Strings.StartRegistryQuestion,
                        Application.ProductName, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2) 
                        == DialogResult.Yes) {

                    bool suc = false;
                    suc = registerApplication(false);

                    MessageBox.Show(String.Format(Strings.RegistrationResult, suc ? Strings.ResultSuccess : Strings.ResultFailed),
                        Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return suc ? 0 : -2;
                }

                return 0;

            }

        }

        /// <summary>
        /// Registers the application
        /// </summary>
        /// <param name="noElev">If true the application is not allowed to elevate</param>
        /// <returns>True on success</returns>
        private static bool registerApplication(bool noElev) {
            bool suc;
            if (Elevation.IsElevationRequired() && !Elevation.IsElevated() && !noElev) {
                suc = (Elevation.RestartElevated("-NOELEV -REG") == 0);
            } else {
                try {
                    suc = Registration.RegisterApplication();
                } catch {
                    suc = false;
                }
            }
            return suc;
        }

        /// <summary>
        /// Unregisters the application
        /// </summary>
        /// <param name="noElev">If true the application is not allowed to elevate</param>
        /// <returns>True on success</returns>
        private static bool unregisterApplication(bool noElev) {
            bool suc;
            if (Elevation.IsElevationRequired() && !Elevation.IsElevated() && !noElev) {
                suc = (Elevation.RestartElevated("-NOELEV -UNREG") == 0);
            } else {
                try {
                    suc = Registration.UnregisterApplication();
                } catch {
                    suc = false;
                }
            }
            return suc;
        }

        private static void openBookmark(string arg) {
            throw new NotImplementedException();
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

    }

}
