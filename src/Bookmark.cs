using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace SG.FileBookmark {

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
            if (!IOUtility.FileExists(path)) {
                throw new Exception("File does not seem to exist");
            }

            if (IOUtility.GetFileExtension(path).Equals(Program.Extension, StringComparison.CurrentCultureIgnoreCase)) {
                throw new Exception("You cannot bookmark a bookmark file");
            }
            if (IOUtility.FileExists(path + Program.Extension)) {
                // file already bookmarked
                return true;
            }

            // first remove previous bookmark
            string dir = IOUtility.GetDirectoryName(path);
            string[] files = IOUtility.GetFiles(dir, "*" + Program.Extension);

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

            IOUtility.CreateFile(path + Program.Extension);

            return IOUtility.FileExists(path + Program.Extension);
        }

        /// <summary>
        /// Removes the bookmark from the file 'path'
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>True on success</returns>
        internal static bool Unmark(string path) {
            if (!IOUtility.FileExists(path)) {
                return true; // nothing there to delete
            }

            if (!IOUtility.GetFileExtension(path).Equals(Program.Extension, StringComparison.CurrentCultureIgnoreCase)) {
                if (IOUtility.FileExists(path + Program.Extension)) {
                    return Unmark(path + Program.Extension); // remove bookmark
                } else {
                    return true; // file not bookmarked whatsoever
                }
            }

            if (IOUtility.FileSize(path) != 0) {
                throw new Exception("File is not empty. Does not seem to be a valid File Bookmark");
            }

            IOUtility.DeleteFile(path);

            return !IOUtility.FileExists(path);
        }

        /// <summary>
        /// Opens the file (bookmark)
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>True on success</returns>
        internal static bool Open(string path) {

            if (IOUtility.GetFileExtension(path).Equals(Program.Extension, StringComparison.CurrentCultureIgnoreCase)) {
                path = path.Substring(0, path.Length - Program.Extension.Length);
            }

            if (IOUtility.FileExists(path)) {
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
