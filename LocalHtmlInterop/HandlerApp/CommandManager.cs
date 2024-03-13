using SGrottel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LocalHtmlInterop.Handler
{
	internal class CommandManager
	{
		public ISimpleLog? Log { get; set; }

		public enum CommandStatus
		{
			Unknown,
			Pending,
			Error,
			Completed
		}

		public class CommandResult
		{
			public CommandStatus Status { get; set; } = CommandStatus.Unknown;

			[JsonPropertyName("exitcode")]
			public int? ExitCode { get; set; }

			public string? Output { get; set; }
		};

		private class Command
		{
			public CommandInfo Info { get; }
			public CommandResult Result { get; } = new();

			public Command(CommandInfo info)
			{
				Info = info;

				// TODO: Implement
				Result.Status = CommandStatus.Error;
				Result.Output = $"{new NotImplementedException()}";
			}

		};

		private object commandsLock = new();
		private List<Command> commands = new();

		internal void Push(CommandInfo cmd)
		{
			lock (commandsLock)
			{
				Log?.Write($"Pushed call:\n\t{cmd.Command}\n\t=>{cmd.CallbackId}\n\t#{cmd.CommandParameters?.Count ?? 0} Parameters");
				commands.Add(new(cmd));

				// TODO: Implement

			}
		}

		internal int CoundRunningCommands()
		{
			lock (commandsLock)
			{
				int c = 0;
				foreach (Command cmd in commands)
				{
					if (cmd.Result.Status == CommandStatus.Pending)
					{
						c++;
					}
				}
				return c;
			}
		}

		internal string GetCallbackResponse(string id)
		{
			lock (commandsLock)
			{
				foreach (Command cmd in commands)
				{
					if (cmd.Info.CallbackId != id) continue;
					return JsonSerializer.Serialize(
						cmd.Result,
						new JsonSerializerOptions()
						{
							WriteIndented = false,
							PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
							DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
							Converters =
							{
								new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
							}
						});
				}
			}
			return "{\"status\":\"unknown\",\"output\":\"Unknown callback id\"}";
		}
	}
}
