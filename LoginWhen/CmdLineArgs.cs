using System;
using System.CommandLine;
using System.Linq;

namespace LoginWhen;

internal class CmdLineArgs
{
    public int HistoryMaxDays { get; set; } = 7;

    public bool CollectSystemEvents { get; set; } = true;

    public bool CollectSecurityEvents { get; set; } = false;

    public bool AllowElevation { get; set; } = true;

    public bool DoJsonOutput { get; set; }

    public string? OutputPipeName { get; set; }

    public bool Parse(string[] args)
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

        RootCommand root = new(description: "Info about user session login/logout times")
        {
            historyMaxDaysOption,
            DisableCollectSystemEventsOption,
            EnableCollectSecurityEventsOption,
            DisableElevationOption,
            DoJsonOutputOption,
            OutputPipeNameOption,
        };

        var res = root.Parse(args);
        if (res.Errors.Any())
        {
            Console.Error.WriteLine("Cmd line parsing failed:\n\t" + string.Join("\n\t", res.Errors));
            return false;
        }
        if (res.Action != null)
        {
            // version or help
            res.Invoke();
            return false;
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

        return true;
    }

}
