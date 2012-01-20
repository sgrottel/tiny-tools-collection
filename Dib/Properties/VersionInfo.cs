namespace Dib.Properties {

    /// <summary>
    /// static class holding build version information about the application
    /// </summary>
    internal static partial class VersionInfo {

        /// <summary>
        /// Answer a version string only storing the major and the minor version number
        /// </summary>
        internal static string SmallVersionString {
            get {
                System.Version ver = new System.Version(VersionString);
                ver = new System.Version(ver.Major, ver.Minor);
                return ver.ToString();
            }
        }

    };

}
