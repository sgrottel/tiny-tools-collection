using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SG.FileBookmark {

    /// <summary>
    /// Utility class for managing long file names
    /// </summary>
    internal static class IOUtility {

        /// <summary>
        /// Answer the file name extension of 'path', i.e. the string portion
        /// including and after the last period
        /// </summary>
        /// <param name="path">The input path</param>
        /// <returns>The file name extension</returns>
        internal static string GetFileExtension(string path) {
            int lastDot = path.LastIndexOf('.');
            return (lastDot >= 0) ? path.Substring(lastDot) : string.Empty;
        }

        /// <summary>
        /// Answer the directory name containing the element described by
        /// 'path', i.e. the string portion before the last directory
        /// separator
        /// </summary>
        /// <param name="path">The input path</param>
        /// <returns>The directory name containing the element of 'path'
        /// </returns>
        internal static string GetDirectoryName(string path) {
            int lastSlash = path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
            if (lastSlash < 0) {
                lastSlash = path.LastIndexOf(System.IO.Path.AltDirectorySeparatorChar);
                if (lastSlash < 0) {
                    return string.Empty;
                }
            }
            return path.Substring(0, lastSlash);
        }

        internal static bool FileExists(string path) {
            return System.IO.File.Exists(path);
        }

        internal static long FileSize(string path) {
            System.IO.FileInfo info = new System.IO.FileInfo(path);
            if (info == null) {
                throw new Exception("Unable to access file");
            }
            return info.Length;
        }

        internal static string[] GetFiles(string directory, string pattern) {
            return System.IO.Directory.GetFiles(directory, pattern);
        }

        internal static void CreateFile(string path) {
            System.IO.File.Create(path).Close();
        }

        internal static void DeleteFile(string path) {
            System.IO.File.Delete(path);
        }

    }

}
