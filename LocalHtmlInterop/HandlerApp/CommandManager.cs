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
			public object ResultsLock { get; set; } = new();
			public Task CommandTask { get; set; } = Task.CompletedTask;

			public Command(CommandInfo info)
			{
				Info = info;
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
				// Commands get cleared when the closes, which is good enough for now.
				// The cleaner approach would be to remove the command explicitly:
				//   - remove commands without callback id when their threads complete
				//   - remove commands with callback id when a socket had connected, queried the result at least once, and the socket then disconnected.

				// select command processor
				if (!SelectCommandProcessor(c))
				{
					c.ResultsLock = new();
					c.Result.Output = "Command processor not found.";
					c.Result.Status = CommandStatus.Error;
					c.CommandTask = Task.CompletedTask;
					c.LogResult(Log);
					return;
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
					lock (cmd.ResultsLock)
					{
						if (cmd.Result.Status == CommandStatus.Pending)
						{
							c++;
						}
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
					lock (cmd.ResultsLock)
					{
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
			}
			return "{\"status\":\"unknown\",\"output\":\"Unknown callback id\"}";
		}

		private bool SelectCommandProcessor(Command command)
		{
			if (command.Info.Command!.Equals("echo", StringComparison.InvariantCultureIgnoreCase))
			{
				lock (command.ResultsLock)
				{
					command.Result.Status = CommandStatus.Pending;
					command.Result.Output = "";
				}

				string o = "Echo Response:\n";
				if (command.Info.CallbackId != null)
				{
					o += $"  to: {command.Info.CallbackId}\n";
				}
				if (command.Info.CommandParameters != null)
				{
					foreach (var p in command.Info.CommandParameters)
					{
						o += $"  {p.Key} = {p.Value}\n";
					}
				}

				lock (command.ResultsLock)
				{
					command.Result.Output = o;
					command.Result.Status = CommandStatus.Completed;
				}

				return true;
			}

			if (command.Info.Command!.Equals("StreamResponseDemo", StringComparison.InvariantCultureIgnoreCase))
			{
				lock (command.ResultsLock)
				{
					command.Result.Status = CommandStatus.Pending;
					command.Result.Output = "";
				}
				command.CommandTask = Task.Run(async () =>
				{
					await Task.Delay(2000);
					lock (command.ResultsLock)
					{
						command.Result.Output = "Stage 1 / 3";
					}
					await Task.Delay(2000);
					lock (command.ResultsLock)
					{
						command.Result.Output = "Stage 2 / 3";
					}
					await Task.Delay(2000);
					lock (command.ResultsLock)
					{
						command.Result.Output = "Stage 3 / 3";
					}
					await Task.Delay(2000);
					lock (command.ResultsLock)
					{
						command.Result.Output = "Completed!";
						command.Result.Status = CommandStatus.Completed;
					}
				});

				return true;
			}

			CommandDefinition? cmdDef;
			if (CommandDefinitions.TryGetValue(command.Info.Command.ToLowerInvariant(), out cmdDef))
			{
				if (cmdDef.ValidationError != null)
				{
					Log?.Write(ISimpleLog.FlagError, $"Command '{command.Info.Command}'[=>{command.Info.CallbackId}] did select an invalid command definition: {cmdDef.ValidationError}");
					return false;
				}

				try
				{
					BuildCommandDefinitionProcessor(command, cmdDef);
					lock (command.ResultsLock)
					{
						return command.Result.Status != CommandStatus.Unknown;
					}
				}
				catch (Exception ex)
				{
					lock (command.ResultsLock)
					{
						command.Result.Output = ex.ToString();
						command.Result.Status = CommandStatus.Error;
					}
				}
			}

			// else, no command processor found:
			return false;
		}

		private void BuildCommandDefinitionProcessor(Command command, CommandDefinition def)
		{
			if (def.exec == null) throw new ArgumentNullException("CommandDefinition.exec");

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
					if (command.Info.CommandParameters != null)
					{
						string? k = command.Info.CommandParameters.Keys.FirstOrDefault((k) => k.Equals(n, StringComparison.OrdinalIgnoreCase));
						if (k != null)
						{
							val = command.Info.CommandParameters[k];
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
			lock (command.ResultsLock)
			{
				command.Result.Status = CommandStatus.Pending;
			}
			command.CommandTask = Task.Run(() =>
			{
				try
				{
					Process p = new() { StartInfo = psi };

					psi.UseShellExecute = false;
					psi.StandardOutputEncoding = Encoding.UTF8;
					psi.RedirectStandardOutput = true;
					psi.StandardErrorEncoding = Encoding.UTF8;
					psi.RedirectStandardError = true;

					var lineRecieved = new DataReceivedEventHandler((object _, DataReceivedEventArgs d) =>
					{
						if (d.Data == null) return;
						lock (command.ResultsLock)
						{
							command.Result.Output += d.Data + "\n";
						}
					});
					p.OutputDataReceived += lineRecieved;
					p.ErrorDataReceived += lineRecieved;

					p.Start();

					p.BeginOutputReadLine();
					p.BeginErrorReadLine();

					p.WaitForExit();
					int exitCode = p.ExitCode;
					p.Close();

					lock (command.ResultsLock)
					{
						command.Result.ExitCode = exitCode;
						command.Result.Status = CommandStatus.Completed;
					}
				}
				catch (Exception ex)
				{
					lock (command.ResultsLock)
					{
						command.Result.Output = ex.ToString();
						command.Result.Status = CommandStatus.Error;
					}
				}
			});
		}
	}
}
