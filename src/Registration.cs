using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;

namespace SG.FileBookmark {

    /// <summary>
    /// Class to handle registration functions
    /// </summary>
    internal static class Registration {

        /// <summary>
        /// Registers this application in the windows registry
        /// </summary>
        /// <returns>True on success</returns>
        internal static bool RegisterApplication() {
            if (!Elevation.IsElevated()) {
                throw new Exception("Rights elevation required");
            }

            RegistryKey file = null;
            RegistryKey shell = null;
            RegistryKey verb = null;
            RegistryKey command = null;

            file = Registry.ClassesRoot.OpenSubKey("*", true);
            try {
                shell = file.CreateSubKey("shell");
                verb = shell.CreateSubKey("bookmark");
                verb.SetValue(string.Empty, Strings.CommandBookmark);
                command = verb.CreateSubKey("command");
                command.SetValue(string.Empty, string.Format("\"{0}\" -MARK \"%1\"", Application.ExecutablePath));
                command.Close();
                verb.Close();
                verb = shell.CreateSubKey("bookmarkopen");
                verb.SetValue(string.Empty, Strings.CommandBookmarkAndOpen);
                command = verb.CreateSubKey("command");
                command.SetValue(string.Empty, string.Format("\"{0}\" -MARK \"%1\" -OPEN \"%1\"", Application.ExecutablePath));
                command.Close();
                verb.Close();

            } finally {
                if (command != null) { command.Close(); command = null; }
                if (verb != null) { verb.Close(); verb = null; }
                if (shell != null) { shell.Close(); shell = null; }
                if (file != null) { file.Close(); file = null; }
            }

            file = Registry.ClassesRoot.CreateSubKey(Program.Extension);
            try {
                file.SetValue(string.Empty, Application.ProductName + Program.Extension);
            } finally {
                if (command != null) { command.Close(); command = null; }
                if (verb != null) { verb.Close(); verb = null; }
                if (shell != null) { shell.Close(); shell = null; }
                if (file != null) { file.Close(); file = null; }
            }

            file = Registry.ClassesRoot.CreateSubKey(Application.ProductName + Program.Extension);
            try {
                file.SetValue(string.Empty, Strings.FileTypeName);

                try {
                    shell = file.CreateSubKey("DefaultIcon");
                    shell.SetValue(string.Empty, Application.ExecutablePath);
                } finally {
                    if (shell != null) { shell.Close(); shell = null; }
                }

                shell = file.CreateSubKey("shell");
                shell.SetValue(string.Empty, "open");

                verb = shell.CreateSubKey("open");
                verb.SetValue(string.Empty, Strings.CommandOpenFile);
                command = verb.CreateSubKey("command");
                command.SetValue(string.Empty, string.Format("\"{0}\" -OPEN \"%1\"", Application.ExecutablePath));
                command.Close();
                verb.Close();

                verb = shell.CreateSubKey("remove");
                verb.SetValue(string.Empty, Strings.CommandUnBookmark);
                command = verb.CreateSubKey("command");
                command.SetValue(string.Empty, string.Format("\"{0}\" -UNMARK \"%1\"", Application.ExecutablePath));
                command.Close();
                verb.Close();

            } finally {
                if (command != null) { command.Close(); command = null; }
                if (verb != null) { verb.Close(); verb = null; }
                if (shell != null) { shell.Close(); shell = null; }
                if (file != null) { file.Close(); file = null; }
            }

            return true;

        }

        /// <summary>
        /// Removes the registry entries of this application from the windows registry
        /// </summary>
        /// <returns>True on success</returns>
        internal static bool UnregisterApplication() {
            if (!Elevation.IsElevated()) {
                throw new Exception("Rights elevation required");
            }

            RegistryKey file = null;
            RegistryKey shell = null;

            file = Registry.ClassesRoot.OpenSubKey("*", true);
            try {
                shell = file.OpenSubKey("shell");
                if (shell.GetSubKeyNames().Contains("bookmark")) {
                    shell.DeleteSubKeyTree("bookmark");
                }
                if (shell.GetSubKeyNames().Contains("bookmarkopen")) {
                    shell.DeleteSubKeyTree("bookmarkopen");
                }

            } finally {
                if (shell != null) { shell.Close(); shell = null; }
                if (file != null) { file.Close(); file = null; }
            }

            file = Registry.ClassesRoot.OpenSubKey(Program.Extension, true);
            try {
                if (file != null) {
                    file.Close();
                    file = null;
                    Registry.ClassesRoot.DeleteSubKeyTree(Program.Extension);
                }
            } finally {
                if (file != null) { file.Close(); file = null; }
            }

            file = Registry.ClassesRoot.OpenSubKey(Application.ProductName + Program.Extension, true);
            try {
                if (file != null) {
                    file.Close();
                    file = null;
                    Registry.ClassesRoot.DeleteSubKeyTree(Application.ProductName + Program.Extension);
                }
            } finally {
                if (file != null) { file.Close(); file = null; }
            }

            return false;
        }

    }

}
