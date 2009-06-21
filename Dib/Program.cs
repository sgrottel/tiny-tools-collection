using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Dib {

    static class Program {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0) {
                bool quiet = false;
                int pos = 0;
                string filename = string.Empty;
                int action = 0;

                if (args[0].Equals("-q", StringComparison.CurrentCultureIgnoreCase)) {
                    quiet = true;
                    pos = 1;
                }
                if (args[pos].Equals("--store", StringComparison.CurrentCultureIgnoreCase)) {
                    action = 1;

                } else if (args[pos].Equals("--restore", StringComparison.CurrentCultureIgnoreCase)) {
                    action = 2;

                } else {
                    MessageBox.Show("Unknown command line parameter " + args[0] + "\n\n"
                        + "You can use the following command line parameters:\n\n"
                        + "Dib.exe --store [filename] [-q]\n"
                        + "Dib.exe --restore [filename] [-q]\n\n"
                        + "Calling with one of these parameters, the tool will store the "
                        + "Desktop Icons positions to a file or restore them from the "
                        + "settings of the file. If the filename is not specified a "
                        + "file dialog window will be shown.\n\n"
                        + "You can use the option \"-q\" to prevent any Dialog windows "
                        + "from open. Instead the operation will be aborted if any "
                        + "problem is encountered", "Desktop Icon Backup",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (action > 0) {
                    pos++;
                    if (!quiet && (args.Length == pos + 1)
                            && (args[pos].Equals("-q", StringComparison.CurrentCultureIgnoreCase))) {
                        quiet = true;
                        pos++;
                    }
                    if (args.Length > pos) {
                        filename = args[pos];
                        pos++;
                    }
                    if ((args.Length > pos) && !quiet) {
                        for (; pos < args.Length; pos++) {
                            if (args[pos].Equals("-q", StringComparison.CurrentCultureIgnoreCase)) {
                                quiet = true;
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(filename)) {
                        action += 2;
                    }

                    DIBForm form = new DIBForm();
                    switch (action) {
                        case 1: // store to filename
                            form.saveToFile(filename, quiet);
                            form.DIBForm_FormClosing(null, null);
                            return;
                        case 2: // restore from filename
                            form.loadFromFile(filename, quiet);
                            form.DIBForm_FormClosing(null, null);
                            return;
                        case 3: // store to file
                            form.saveButton_Click(null, null);
                            form.DIBForm_FormClosing(null, null);
                            return;
                        case 4: // restore to file
                            form.loadButton_Click(null, null);
                            form.DIBForm_FormClosing(null, null);
                            return;
                    }
                }
            }

            Application.Run(new DIBForm());
        }

    }

}
