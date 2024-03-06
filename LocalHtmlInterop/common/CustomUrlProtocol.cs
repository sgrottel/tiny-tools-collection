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
		/// <param name="execPath">The full file system path to the executable to be used as custom protocol handler</param>
		[SupportedOSPlatform("windows")]
		static public void RegisterAsHandler(string execPath)
		{
			var crKey = Registry.ClassesRoot.CreateSubKey(ProtocolSchema, true);
			crKey.SetValue(null, $"URL:{ProtocolSchema} Protocol");
			crKey.SetValue("URL Protocol", "");
			var shell = crKey.CreateSubKey("shell");
			var open = shell.CreateSubKey("open");
			var cmd = open.CreateSubKey("command");
			cmd.SetValue(null, "\"" + execPath + "\" \"%1\"");
		}

	}
}
