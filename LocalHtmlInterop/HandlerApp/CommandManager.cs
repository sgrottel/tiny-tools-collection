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

		private class Command
		{
			public CommandInfo Info { get; }
			public CommandResult Result { get; set; } = new();

			public Command(CommandInfo info)
			{
				Info = info;
				Result.Status = CommandStatus.Pending;
			}
		};

		private object commandsLock = new();
		private List<Command> commands = new();

		internal void Push(CommandInfo cmd)
		{
			lock (commandsLock)
			{
				Log?.Write($"Pushed call:\n\t{cmd.Command}\n\t=>{cmd.CallbackId}\n\t#{cmd.CommandParameters?.Count ?? 0} Parameters");

				Command c = new(cmd);
				commands.Add(c);

				// select command processor
				Task<CommandResult>? commandProcessor = null;

				if (!string.IsNullOrWhiteSpace(cmd.Command))
				{
					commandProcessor = SelectCommandProcessor(cmd);
				}

				if (commandProcessor != null)
				{
					Task t = commandProcessor
						.ContinueWith(
						(res) =>
						{
							if (res.IsCompletedSuccessfully)
							{
								if (res.Result.Status == CommandStatus.Unknown || res.Result.Status == CommandStatus.Pending)
								{
									res.Result.Status = ((res.Result.ExitCode ?? -1) == 0) ? CommandStatus.Completed : CommandStatus.Error;
								}

								c.Result = res.Result;
							}
							else
							{
								c.Result.Output = res.Exception?.ToString() ?? "Unknown error";
								c.Result.Status = CommandStatus.Error;
							}
						});
					if (commandProcessor.Status == TaskStatus.Created)
					{
						commandProcessor.Start();
					}
				}
				else
				{
					c.Result.Output = "Command processor not found.";
					c.Result.Status = CommandStatus.Error;
				}
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

		private Task<CommandResult>? SelectCommandProcessor(CommandInfo command)
		{
			if (command.Command!.Equals("echo", StringComparison.InvariantCultureIgnoreCase))
			{
				string o = "Echo Response:\n";
				if (command.CallbackId != null)
				{
					o += $"  to: {command.CallbackId}\n";
				}
				if (command.CommandParameters != null)
				{
					foreach (var p in command.CommandParameters)
					{
						o += $"  {p.Key} = {p.Value}\n";
					}
				}

				return Task.FromResult(new CommandResult()
				{
					Output = o,
					Status = CommandStatus.Completed
				});
			}

			// TODO: Implement
			//var t = new Task<CommandResult>(() =>
			//{
			//	throw new NotImplementedException();
			//});
			//return t;

			// if no command processor found:
			return null;
		}

	}
}
