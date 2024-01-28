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
	///   sgrlhiop:TODO
	/// </example>
	static public class CustomUrlProtocol
	{
		/// <summary>
		/// The custom protocol
		/// </summary>
		const string protocolSchema = "sgrlhiop";

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
			var crKey = Registry.ClassesRoot.CreateSubKey(protocolSchema, true);
			crKey.SetValue(null, $"URL:{protocolSchema} Protocol");
			crKey.SetValue("URL Protocol", "");
			var shell = crKey.CreateSubKey("shell");
			var open = shell.CreateSubKey("open");
			var cmd = open.CreateSubKey("command");
			cmd.SetValue(null, "\"" + execPath + "\" \"%1\"");
		}

	}
}
