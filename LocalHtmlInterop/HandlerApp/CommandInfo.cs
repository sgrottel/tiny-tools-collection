using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalHtmlInterop.Handler
{
	internal class CommandInfo
	{
		public string? Command { get; set; }
		public string? CallbackId { get; set; }
		public Dictionary<string, string>? CommandParameters { get; set; }
	}
}
