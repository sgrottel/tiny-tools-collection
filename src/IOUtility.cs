using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SG.FileBookmark {

    /// <summary>
    /// Utility class for managing long file names
    /// </summary>
    internal static class IOUtility {

        internal static bool FileExists(string path) {
            return System.IO.File.Exists(path);
        }

        internal static string GetFileExtension(string path) {
            return System.IO.Path.GetExtension(path);
        }

        internal static string GetDirectoryName(string path) {
            return System.IO.Path.GetDirectoryName(path);
        }

        internal static string[] GetFiles(string directory, string pattern) {
            return System.IO.Directory.GetFiles(directory, pattern);
        }

        internal static void CreateFile(string path) {
            System.IO.File.Create(path).Close();
        }

        internal static long FileSize(string path) {
            System.IO.FileInfo info = new System.IO.FileInfo(path);
            if (info == null) {
                throw new Exception("Unable to access file");
            }
            return info.Length;
        }

        internal static void DeleteFile(string path) {
            System.IO.File.Delete(path);
        }

    }

}
