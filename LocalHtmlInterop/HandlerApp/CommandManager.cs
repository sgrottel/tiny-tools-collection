using SGrottel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		public Dictionary<string, CommandDefinition> CommandDefinitions { get; set; } = new();

		private class Command
		{
			public CommandInfo Info { get; }
			public CommandResult Result { get; set; } = new();

			public Command(CommandInfo info)
			{
				Info = info;
				Result.Status = CommandStatus.Pending;
			}

			internal void LogResult(ISimpleLog? log)
			{
				log?.Write($"Command '{Info.Command}'[=>{Info.CallbackId}]\n\tcompleted as '{Result.Status}'.");
				log?.Write($"\t{Result.Output?.Split('\n', 2)[0]}");
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
					commandProcessor
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

							c.LogResult(Log);
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

					c.LogResult(Log);
				}
			}
		}

		internal int CountRunningCommands()
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

			CommandDefinition? cmdDef;
			if (CommandDefinitions.TryGetValue(command.Command.ToLowerInvariant(), out cmdDef))
			{
				if (cmdDef.ValidationError != null)
				{
					Log?.Write(ISimpleLog.FlagError, $"Command '{command.Command}'[=>{command.CallbackId}] did select an invalid command definition: {cmdDef.ValidationError}");
					return null;
				}

				try
				{
					return BuildCommandDefinitionProcessor(command, cmdDef);
				}
				catch (Exception ex)
				{
					return Task.FromResult(new CommandResult()
					{
						Output = ex.ToString(),
						Status = CommandStatus.Error
					});
				}
			}
			// else, no command processor found:
			return null;
		}

		private Task<CommandResult>? BuildCommandDefinitionProcessor(CommandInfo cmd, CommandDefinition def)
		{
			if (def.exec == null) return null;

			// match parameters
			Dictionary<string, string> parameters = new();
			if (def.args != null)
			{
				var loadParam = (string? n, bool? req) => {
					n = n?.Trim();
					if (string.IsNullOrEmpty(n)) return;

					if (parameters.Keys.FirstOrDefault((k) => k.Equals(n, StringComparison.OrdinalIgnoreCase)) != null)
					{
						// param already loaded
						return;
					}

					if (req == null) req = false;

					string? val = null;
					if (cmd.CommandParameters != null)
					{
						string? k = cmd.CommandParameters.Keys.FirstOrDefault((k) => k.Equals(n, StringComparison.OrdinalIgnoreCase));
						if (k != null)
						{
							val = cmd.CommandParameters[k];
						}
					}

					if (val == null)
					{
						if (req.Value) throw new ArgumentNullException($"Required argument {n} not provided");
						return;
					}

					parameters.Add(n, val);
				};

				foreach (var arg in def.args)
				{
					loadParam(arg.param, arg.required);
					if (arg.parameters != null)
					{
						foreach (var p in arg.parameters)
						{
							loadParam(p.name, p.required ?? arg.required);
						}
					}
				}
			}

			ProcessStartInfo psi = new()
			{
				CreateNoWindow = true,
				FileName = def.exec.FullPath,
				WorkingDirectory = def.WorkingDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
			};
			// build command argument list
			if (def.args != null)
			{
				foreach (var arg in def.args)
				{
					psi.ArgumentList.Add(arg.Interpolate(parameters));
				}
			}

			// if preparation succeeds, build task which runs process, collects output, builds result
			return Task.Run(() =>
			{
				Process p = new() { StartInfo = psi };

				psi.UseShellExecute = false;
				psi.StandardOutputEncoding = Encoding.UTF8;
				psi.RedirectStandardOutput = true;
				psi.StandardErrorEncoding = Encoding.UTF8;
				psi.RedirectStandardError = true;

				StringBuilder sb = new();
				var lineRecieved = new DataReceivedEventHandler((object _, DataReceivedEventArgs d) =>
				{
					if (d.Data == null) return;
					sb.Append($"{d.Data}\n");
				});
				p.OutputDataReceived += lineRecieved;
				p.ErrorDataReceived += lineRecieved;

				p.Start();
				p.BeginOutputReadLine();
				p.BeginErrorReadLine();

				p.WaitForExit();
				int exitCode = p.ExitCode;
				p.Close();

				return new CommandResult()
				{
					ExitCode = exitCode,
					Output = sb.ToString(),
					Status = CommandStatus.Completed
				};
			});
		}
	}
}
