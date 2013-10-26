using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SG.FileBookmark {

    /// <summary>
    /// Utility class for managing long file names
    /// </summary>
    internal static class IOUtility {

        #region Simple String Operations

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

        #endregion

        #region PInvoke

        [Flags]
        private enum FileAccess : uint {
            //
            // Standart Section
            //

            AccessSystemSecurity = 0x1000000,   // AccessSystemAcl access type
            MaximumAllowed = 0x2000000,     // MaximumAllowed access type

            Delete = 0x10000,
            ReadControl = 0x20000,
            WriteDAC = 0x40000,
            WriteOwner = 0x80000,
            Synchronize = 0x100000,

            StandardRightsRequired = 0xF0000,
            StandardRightsRead = ReadControl,
            StandardRightsWrite = ReadControl,
            StandardRightsExecute = ReadControl,
            StandardRightsAll = 0x1F0000,
            SpecificRightsAll = 0xFFFF,

            FILE_READ_DATA = 0x0001,        // file & pipe
            FILE_LIST_DIRECTORY = 0x0001,       // directory
            FILE_WRITE_DATA = 0x0002,       // file & pipe
            FILE_ADD_FILE = 0x0002,         // directory
            FILE_APPEND_DATA = 0x0004,      // file
            FILE_ADD_SUBDIRECTORY = 0x0004,     // directory
            FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
            FILE_READ_EA = 0x0008,          // file & directory
            FILE_WRITE_EA = 0x0010,         // file & directory
            FILE_EXECUTE = 0x0020,          // file
            FILE_TRAVERSE = 0x0020,         // directory
            FILE_DELETE_CHILD = 0x0040,     // directory
            FILE_READ_ATTRIBUTES = 0x0080,      // all
            FILE_WRITE_ATTRIBUTES = 0x0100,     // all

            //
            // Generic Section
            //

            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,

            SPECIFIC_RIGHTS_ALL = 0x00FFFF,
            FILE_ALL_ACCESS =
            StandardRightsRequired |
            Synchronize |
            0x1FF,

            FILE_GENERIC_READ =
            StandardRightsRead |
            FILE_READ_DATA |
            FILE_READ_ATTRIBUTES |
            FILE_READ_EA |
            Synchronize,

            FILE_GENERIC_WRITE =
            StandardRightsWrite |
            FILE_WRITE_DATA |
            FILE_WRITE_ATTRIBUTES |
            FILE_WRITE_EA |
            FILE_APPEND_DATA |
            Synchronize,

            FILE_GENERIC_EXECUTE =
            StandardRightsExecute |
              FILE_READ_ATTRIBUTES |
              FILE_EXECUTE |
              Synchronize
        }

        [Flags]
        private enum FileShare : uint {
            /// <summary>
            ///
            /// </summary>
            None = 0x00000000,
            /// <summary>
            /// Enables subsequent open operations on an object to request read access.
            /// Otherwise, other processes cannot open the object if they request read access.
            /// If this flag is not specified, but the object has been opened for read access, the function fails.
            /// </summary>
            Read = 0x00000001,
            /// <summary>
            /// Enables subsequent open operations on an object to request write access.
            /// Otherwise, other processes cannot open the object if they request write access.
            /// If this flag is not specified, but the object has been opened for write access, the function fails.
            /// </summary>
            Write = 0x00000002,
            /// <summary>
            /// Enables subsequent open operations on an object to request delete access.
            /// Otherwise, other processes cannot open the object if they request delete access.
            /// If this flag is not specified, but the object has been opened for delete access, the function fails.
            /// </summary>
            Delete = 0x00000004
        }

        private enum CreationDisposition : uint {
            /// <summary>
            /// Creates a new file. The function fails if a specified file exists.
            /// </summary>
            New = 1,
            /// <summary>
            /// Creates a new file, always.
            /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes,
            /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
            /// </summary>
            CreateAlways = 2,
            /// <summary>
            /// Opens a file. The function fails if the file does not exist.
            /// </summary>
            OpenExisting = 3,
            /// <summary>
            /// Opens a file, always.
            /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
            /// </summary>
            OpenAlways = 4,
            /// <summary>
            /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
            /// The calling process must open the file with the GENERIC_WRITE access right.
            /// </summary>
            TruncateExisting = 5
        }

        [Flags]
        private enum FileAttributes : uint {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct BY_HANDLE_FILE_INFORMATION {
            public uint FileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        private enum FINDEX_SEARCH_OPS {
            FindExSearchNameMatch = 0,
            FindExSearchLimitToDirectories = 1,
            FindExSearchLimitToDevices = 2
        }

        private enum FINDEX_INFO_LEVELS {
            FindExInfoStandard = 0,
            FindExInfoBasic = 1
        }

        // The CharSet must match the CharSet of the corresponding PInvoke signature
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WIN32_FIND_DATA {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        // dwAdditionalFlags:
        private const int FIND_FIRST_EX_CASE_SENSITIVE = 1;
        private const int FIND_FIRST_EX_LARGE_FETCH = 2;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFileW([MarshalAs(UnmanagedType.LPWStr)]string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateFileW(
             [MarshalAs(UnmanagedType.LPWStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes,
             [MarshalAs(UnmanagedType.U4)] CreationDisposition creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(IntPtr hFile,
           out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindFirstFileEx(
            string lpFileName,
            FINDEX_INFO_LEVELS fInfoLevelId,
            out WIN32_FIND_DATA lpFindFileData,
            FINDEX_SEARCH_OPS fSearchOp,
            IntPtr lpSearchFilter,
            int dwAdditionalFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA
           lpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        #endregion

        /// <summary>
        /// Tests if a file exists
        /// </summary>
        /// <param name="path">The path to the file to test</param>
        /// <returns>True if the file exists</returns>
        internal static bool FileExists(string path) {
            IntPtr hFile = CreateFileW(@"\\?\" + path, FileAccess.GenericRead, FileShare.Read, IntPtr.Zero, CreationDisposition.OpenExisting, FileAttributes.Normal, IntPtr.Zero);
            if (hFile.ToInt32() == -1) return false;
            CloseHandle(hFile);
            return true;
        }

        /// <summary>
        /// Answer the size of a file
        /// </summary>
        /// <param name="path">The path to the file to anwer the size of</param>
        /// <returns>The size of the file in bytes</returns>
        internal static long FileSize(string path) {
            IntPtr hFile = CreateFileW(@"\\?\" + path, FileAccess.GenericRead, FileShare.Read, IntPtr.Zero, CreationDisposition.OpenExisting, FileAttributes.Normal, IntPtr.Zero);
            if (hFile.ToInt32() == -1) {
                int lastError = Marshal.GetLastWin32Error();
                throw new Exception("Unable to access file '" + path + "': LastError=" + lastError.ToString());
            }
            BY_HANDLE_FILE_INFORMATION info;
            if (!GetFileInformationByHandle(hFile, out info)) {
                int lastError = Marshal.GetLastWin32Error();
                CloseHandle(hFile);
                throw new Exception("Unable to get info of file '" + path + "': LastError=" + lastError.ToString());
            }
            CloseHandle(hFile);

            return (long)((((ulong)info.FileSizeHigh) << 32) + (ulong)info.FileSizeLow);
        }

        /// <summary>
        /// Gets all files with the directory 'directory' matching the glob
        /// pattern 'pattern'
        /// </summary>
        /// <param name="directory">The directory to search in</param>
        /// <param name="pattern">The glob pattern</param>
        /// <returns>All found files with full paths</returns>
        internal static string[] GetFiles(string directory, string pattern) {
            List<String> files = new List<string>();

            if (!directory.EndsWith("\\")) directory = directory + "\\";
            string path = @"\\?\" + directory + pattern;

            WIN32_FIND_DATA findData;

            FINDEX_INFO_LEVELS findInfoLevel = FINDEX_INFO_LEVELS.FindExInfoStandard;
            int additionalFlags = 0;
            if (Environment.OSVersion.Version.Major >= 6) {
                findInfoLevel = FINDEX_INFO_LEVELS.FindExInfoBasic;
                additionalFlags = FIND_FIRST_EX_LARGE_FETCH;
            }

            IntPtr hFile = FindFirstFileEx(path,
                findInfoLevel, out findData, FINDEX_SEARCH_OPS.FindExSearchNameMatch,
                IntPtr.Zero, additionalFlags);
            int error = Marshal.GetLastWin32Error();

            if (hFile.ToInt32() != -1) {
                do {
                    if ((findData.dwFileAttributes & (uint)FileAttributes.Directory) != (uint)FileAttributes.Directory) {
                        files.Add(directory + findData.cFileName);
                    }
                } while (FindNextFile(hFile, out findData));

                FindClose(hFile);
            }

            return files.ToArray();
        }

        /// <summary>
        /// Creates a new, empty file at 'path' not overwriting existing files
        /// </summary>
        /// <param name="path">The path to the file to be created</param>
        internal static void CreateFile(string path) {
            IntPtr hFile = CreateFileW(@"\\?\" + path, FileAccess.GenericWrite, FileShare.None, IntPtr.Zero, CreationDisposition.New, FileAttributes.Normal, IntPtr.Zero);
            if (hFile.ToInt32() == -1) {
                int lastError = Marshal.GetLastWin32Error();
                throw new Exception("Unable to create file '" + path + "': LastError=" + lastError.ToString());
            } else {
                CloseHandle(hFile);
            }
        }

        /// <summary>
        /// Deletes the file path points to
        /// </summary>
        /// <param name="path">The path to the file to delete</param>
        internal static void DeleteFile(string path) {
            if (DeleteFileW(@"\\?\" + path) == false) {
                int lastError = Marshal.GetLastWin32Error();
                throw new Exception("Cannto delete '" + path +"': LastError=" + lastError.ToString());
            }
        }

    }

}
