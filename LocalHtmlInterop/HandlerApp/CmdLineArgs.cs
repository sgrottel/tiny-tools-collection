using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LocalHtmlInterop.Handler
{
	internal class CmdLineArgs
	{
		public enum Operation {
			None,
			InteropCall,
			RegisterHandler,
			UnregisterHandler
		};

		public Operation AppOperation { get; private set; } = Operation.None;

		public string? CallbackId { get; private set; }

		public string? Command { get; private set; }

		public Dictionary<string, string>? CommandParameters { get; private set; }

		public bool Parse(string[] args)
		{
			if (args.Length == 1)
			{
				if (args[0].StartsWith($"{CustomUrlProtocol.ProtocolSchema}:", StringComparison.InvariantCultureIgnoreCase))
				{
					AppOperation = Operation.InteropCall;

					var seg = args[0].Split(':', 3);
					if (seg.Length != 3) throw new ArgumentException("Custom url call malformed. Unexpected structure separators ':'");
					System.Diagnostics.Debug.Assert(string.Equals(seg[0], CustomUrlProtocol.ProtocolSchema, StringComparison.InvariantCultureIgnoreCase));

					CallbackId = seg[1];

					seg = seg[2].Split('?', 2);
					if (seg.Length <= 0) throw new ArgumentException("Custom url call malformed. Failed to identify command.");
					Command = seg[0];

					CommandParameters = new();
					if (seg.Length == 2)
					{
						foreach (var p in seg[1].Split('&'))
						{
							seg = p.Split("=", 2);
							if (seg.Length != 2) continue;
							CommandParameters[HttpUtility.UrlDecode(seg[0])] = HttpUtility.UrlDecode(seg[1]);
						}
					}

					return true;
				}
			}

			if (args.Length == 2)
			{
				if (args[0].Equals("-register", StringComparison.InvariantCultureIgnoreCase))
				{
					AppOperation = Operation.RegisterHandler;
					CallbackId = args[1];
					return true;
				}

				if (args[0].Equals("-unregister", StringComparison.InvariantCultureIgnoreCase))
				{
					AppOperation = Operation.UnregisterHandler;
					CallbackId = args[1];
					return true;
				}
			}

			// Arg did not trigger any operation
			return false;
		}

	}
}
