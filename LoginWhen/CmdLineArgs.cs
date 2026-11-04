using System;
using System.CommandLine;
using System.Linq;

namespace LoginWhen;

internal class CmdLineArgs
{
    public enum Operation {
        Normal,
        None,
        LogLock,
        LogUnlock,
        RegisterLogTasks,
    }

    public int HistoryMaxDays { get; set; } = 7;

    public bool CollectSystemEvents { get; set; } = true;

    public bool CollectSecurityEvents { get; set; } = false;

    public bool AllowElevation { get; set; } = true;

    public bool DoJsonOutput { get; set; }

    public string? OutputPipeName { get; set; }

    public Operation Parse(string[] args)
    {
        Option<int> historyMaxDaysOption = new(name: "--days")
        {
            Description = "Number of days to show",
        };
        Option<bool> DisableCollectSystemEventsOption = new(name: "--no-system-events")
        {
            Description = "Do not collect system events",
            Hidden = true,
        };
        Option<bool> EnableCollectSecurityEventsOption = new(name: "--with-security-events")
        {
            Description = "Collect security events",
            Hidden = true,
        };
        Option<bool> DisableElevationOption = new(name: "--no-elevation")
        {
            Description = "Do not automatically rerun the tool with elevated access rights",
        };
        Option<bool> DoJsonOutputOption = new(name: "--json")
        {
            Description = "Print result as json",
            Hidden = true,
        };
        Option<string> OutputPipeNameOption = new(name: "--output-pipe")
        {
            Description = "Write output to the named pipe",
            Hidden = true,
        };

        Operation selectedOperation = Operation.None;

        Command logLockCommand = new(name: "log-lock", description: "Logs a lock event")
        {
            Hidden = true,
        };
        logLockCommand.SetAction(_ => { selectedOperation = Operation.LogLock; });

        Command logUnlockCommand = new(name: "log-unlock", description: "Logs an unlock event")
        {
            Hidden = true,
        };
        logUnlockCommand.SetAction(_ => { selectedOperation = Operation.LogUnlock; });

        Command registerLogTasksCommand = new(name: "register-log-tasks", description: "Registers the log commands with the windows task scheduler");
        registerLogTasksCommand.SetAction(_ => { selectedOperation = Operation.RegisterLogTasks; });

        RootCommand root = new(description: "Info about user session login/logout times")
        {
            historyMaxDaysOption,
            DisableCollectSystemEventsOption,
            EnableCollectSecurityEventsOption,
            DisableElevationOption,
            DoJsonOutputOption,
            OutputPipeNameOption,
            logLockCommand,
            logUnlockCommand,
            registerLogTasksCommand,
        };

        var res = root.Parse(args);
        if (res.Errors.Any())
        {
            Console.Error.WriteLine("Cmd line parsing failed:\n\t" + string.Join("\n\t", res.Errors));
            return Operation.None;
        }
        if (res.Action != null)
        {
            // version, help, or log actions
            res.Invoke();

            return selectedOperation;
        }

        var hmdr = res.GetResult(historyMaxDaysOption);
        if (hmdr != null && !hmdr.Errors.Any())
        {
            HistoryMaxDays = res.GetValue(historyMaxDaysOption);
        }

        CollectSystemEvents = !res.GetValue(DisableCollectSystemEventsOption);
        CollectSecurityEvents = res.GetValue(EnableCollectSecurityEventsOption);
        AllowElevation = !res.GetValue(DisableElevationOption);

        DoJsonOutput = res.GetValue(DoJsonOutputOption);
        OutputPipeName = res.GetValue(OutputPipeNameOption);

        return Operation.Normal;
    }

}
