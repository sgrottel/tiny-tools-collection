using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace FileBookmark {

    /// <summary>
    /// Class holding bookmark related functions
    /// </summary>
    static internal class Bookmark {

        /// <summary>
        /// Marks the file 'path' with a bookmark and removes all bookmarks from all other files in the directory
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>True on success</returns>
        internal static bool Mark(string path) {
            if (!System.IO.File.Exists(path)) {
                throw new Exception("File does not seem to exist");
            }

            if (System.IO.Path.GetExtension(path).Equals(Program.Extension, StringComparison.CurrentCultureIgnoreCase)) {
                throw new Exception("You cannot bookmark a bookmark file");
            }
            if (System.IO.File.Exists(path + Program.Extension)) {
                // file already bookmarked
                return true;
            }

            // first remove previous bookmark
            string dir = System.IO.Path.GetDirectoryName(path);
            string[] files = System.IO.Directory.GetFiles(dir, "*" + Program.Extension);

            if ((files == null) || (files.Length == 0)) {
                // no other bookmarks, so we are good!
            } else {
                // remove other bookmarks ... next program version may be able to handle multiple bookmarks with a directory
                foreach (string file in files) {
                    try {
                        Unmark(file);
                    } catch (Exception ex) {
                        DialogResult result = MessageBox.Show(string.Format(Strings.BookmarkRemoveError, file, ex.ToString()),
                            Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                        if (result == DialogResult.Cancel) {
                            return false;
                        }
                    }
                }
            }

            System.IO.File.Create(path + Program.Extension).Close();

            return System.IO.File.Exists(path + Program.Extension);
        }

        /// <summary>
        /// Removes the bookmark from the file 'path'
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>True on success</returns>
        internal static bool Unmark(string path) {
            if (!System.IO.File.Exists(path)) {
                return true; // nothing there to delete
            }

            if (!System.IO.Path.GetExtension(path).Equals(Program.Extension, StringComparison.CurrentCultureIgnoreCase)) {
                if (System.IO.File.Exists(path + Program.Extension)) {
                    return Unmark(path + Program.Extension); // remove bookmark
                } else {
                    return true; // file not bookmarked whatsoever
                }
            }

            System.IO.FileInfo info = new System.IO.FileInfo(path);
            if (info == null) {
                return false; // Unable to access file
            }
            if (info.Length != 0) {
                throw new Exception("File is not empty. Does not seem to be a valid File Bookmark");
            }

            System.IO.File.Delete(path);

            return !System.IO.File.Exists(path);
        }

        /// <summary>
        /// Opens the file (bookmark)
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>True on success</returns>
        internal static bool Open(string path) {

            if (System.IO.Path.GetExtension(path).Equals(Program.Extension, StringComparison.CurrentCultureIgnoreCase)) {
                path = path.Substring(0, path.Length - Program.Extension.Length);
            }

            if (System.IO.File.Exists(path)) {
                try {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = path;
                    psi.UseShellExecute = true;
                    Process p = Process.Start(psi);
                    return (p != null);
                } catch {
                }
            }

            return false;
        }

    }

}
