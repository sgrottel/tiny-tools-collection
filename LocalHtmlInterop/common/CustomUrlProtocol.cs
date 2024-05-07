using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LocalHtmlInterop
{

	/// <summary>
	/// Utility to manage the custom protocol
	/// </summary>
	/// <example>
	///   sgrlhiop:16c1d49fff02d53fbccd9147a21791d0:echo?fancy=seeing%20you%20here&tell%20me=more
	///   ^        ^                                ^    ^
	///   |        |                                |    +--  command parameters, url encoded
	///   |        |                                +-------  the command to execute
	///   |        +----------------------------------------  The callback Id (optional) to connect via websocket
	///   +-------------------------------------------------  The protocol name
	/// </example>
	static public class CustomUrlProtocol
	{
		/// <summary>
		/// The custom protocol
		/// </summary>
		public const string ProtocolSchema = "sgrlhiop";

		/// <summary>
		/// Registers an executable as handler for this custom url protocol.
		///
		/// Must be called with elevated rights.
		///
		/// Opening a link with this custom protocol works after registration in your browser.
		/// In the respective confirmation dialog, you can "always trust" the protocol handler.
		///
		/// To confirm opening URLs directly, without link, in Firefox, you can disable the confirmation dialog:
		/// about:config
		///   network.protocol-handler.expose.sgrlhiop = true
		///   network.protocol-handler.external.sgrlhiop = true
		/// </summary>
		/// <param name="exePath">The full file system path to the executable to be used as custom protocol handler</param>
		[SupportedOSPlatform("windows")]
		static public void RegisterAsHandler(string exePath)
		{
			using (var crKey = Registry.ClassesRoot.CreateSubKey(ProtocolSchema, true))
			{
				crKey.SetValue(null, $"URL:{ProtocolSchema} Protocol");
				crKey.SetValue("URL Protocol", "");
				using (var shell = crKey.CreateSubKey("shell"))
				using (var open = shell.CreateSubKey("open"))
				using (var cmd = open.CreateSubKey("command"))
					cmd.SetValue(null, "\"" + exePath + "\" \"%1\"");
			}
		}

		/// <summary>
		/// Returns the path of the handler executable registered for this custom url protocol,
		/// or null if there is no valid registration.
		/// </summary>
		/// <returns>The full file system path to the executable to be used as custom protocol handler</returns>
		static public string? GetRegisteredHandlerExe()
		{
			if (!OperatingSystem.IsWindows()) return null;

			using (var crKey = Registry.ClassesRoot.OpenSubKey(ProtocolSchema))
			{
				if (crKey == null) return null;
				if (!crKey.GetValueNames().Contains("URL Protocol")) return null; // ill-registered handler

				using (var cmdKey = crKey.OpenSubKey(@"shell\open\command"))
				{
					string? cmdLine = (string?)cmdKey?.GetValue(null);
					if (string.IsNullOrEmpty(cmdLine)) return null;

					return cmdLine.Split(
						(cmdLine[0] == '"') ? '"' : ' ',
						2,
						StringSplitOptions.RemoveEmptyEntries
						)[0];
				}
			}
		}

		/// <summary>
		/// Removes the registration of this custom url protocol.
		/// </summary>
		/// <remarks>
		/// This does not test if the registration matches the executing application.
		/// </remarks>
		[SupportedOSPlatform("windows")]
		static public void UnregisterHandler()
		{
			if (Registry.ClassesRoot.GetSubKeyNames().Contains(ProtocolSchema))
			{
				Registry.ClassesRoot.DeleteSubKeyTree(ProtocolSchema);
			}
		}

	}
}
