using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FileBookmark {

    static class Program {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The command line arguments</param>
        [STAThread]
        static void Main(string[] args) {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int state = 0;
            foreach (string arg in args) {
                if (state == 1) {
                    // mark this file

                    // TODO: Implement

                    state = 0;
                } else if (state == 2) {
                    // remove this bookmark file

                    // TODO: Implement

                    state = 0;
                } else {
                    if (arg.Equals("-REG", StringComparison.CurrentCultureIgnoreCase)) {
                        // registers this application

                        // TODO: Implement

                    }
                    if (arg.Equals("-UNREG", StringComparison.CurrentCultureIgnoreCase)) {
                        // un-registers this application

                        // TODO: Implement

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
            }

            //Application.Run(new Form1());

        }

    }

}
