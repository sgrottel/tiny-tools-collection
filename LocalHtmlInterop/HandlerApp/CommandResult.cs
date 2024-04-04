using System.Text.Json.Serialization;

namespace LocalHtmlInterop.Handler
{
	internal class CommandResult
	{
		public CommandStatus Status { get; set; } = CommandStatus.Unknown;

		[JsonPropertyName("exitcode")]
		public int? ExitCode { get; set; }

		public string? Output { get; set; }
	}
}
