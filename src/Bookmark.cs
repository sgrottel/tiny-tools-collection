using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
            MessageBox.Show(string.Format("Mark \"{0}\" not implemented", path));
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the bookmark from the file 'path'
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>True on success</returns>
        internal static bool Unmark(string path) {
            MessageBox.Show(string.Format("Unmark \"{0}\" not implemented", path));
            throw new NotImplementedException();
        }

        /// <summary>
        /// Opens the file (bookmark)
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>True on success</returns>
        internal static bool Open(string path) {
            MessageBox.Show(string.Format("Open \"{0}\" not implemented", path));
            throw new NotImplementedException();
        }
/*
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
*/
    }

}
