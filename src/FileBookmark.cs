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

                int rv = 0;
#if DEBUG
                System.Text.StringBuilder err = new System.Text.StringBuilder();
#endif

                foreach (string file in addBM) {
                    try {
                        if (!Bookmark.Mark(file)) rv = -1;
#if DEBUG
                    } catch (Exception ex) {
                        err.AppendFormat("Cannot mark \"{0}\": {1}", file, ex);
#else
                    } catch {
#endif
                    }
                }

                foreach (string file in removeBM) {
                    try {
                        if (!Bookmark.Unmark(file)) rv = -4;
#if DEBUG
                    } catch (Exception ex) {
                        err.AppendFormat("Cannot unmark \"{0}\": {1}", file, ex);
#else
                    } catch {
#endif
                    }
                }

                foreach (string file in openBM) {
                    try {
                        if (System.IO.File.Exists(file)) {
                            if (!Bookmark.Open(file)) rv = -5;
                        } else if (System.IO.Path.GetExtension(file).Equals(Program.Extension, StringComparison.CurrentCultureIgnoreCase)) {
                            string f = file.Substring(0, file.Length - Program.Extension.Length);
                            if (System.IO.File.Exists(f)) {
                                if (!Bookmark.Open(f)) rv = -6;
                            }
                        }
#if DEBUG
                    } catch (Exception ex) {
                        err.AppendFormat("Cannot open \"{0}\": {1}", file, ex);
#else
                    } catch {
#endif
                    }
                }

#if DEBUG
                if (err.Length > 0) {
                    MessageBox.Show(err.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
#endif

                return rv;

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

    }

}
