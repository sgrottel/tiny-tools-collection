using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text.Json;
using static LoginWhen.EventCollector;

namespace LoginWhen;

class Program
{
    static string EventTypeName(EventType et)
    {
        switch (et)
        {
            case EventType.Logon:
                return "[  ==>] Log On";
            case EventType.Logoff:
                return "[<==  ] Log Off";
            case EventType.Unlock:
                return "[  -->] Unlock";
            case EventType.Lock:
                return "[<--  ] Lock";
            case EventType.Startup:
                return "[  ==>] Startup";
            case EventType.Shutdown:
                return "[<==  ] Shutdown";
        }
        return "";
    }

    static void Main(string[] args)
    {
        CmdLineArgs cmdLineArgs = new();
        if (!cmdLineArgs.Parse(args)) return;

        using NamedPipeClientStream? outputPipe = cmdLineArgs.OutputPipeName != null
            ? new(".", cmdLineArgs.OutputPipeName, PipeDirection.Out)
            : null;
        outputPipe?.Connect();
        using StreamWriter? pipeWriter = outputPipe != null
            ? new(outputPipe) { AutoFlush = true }
            : null;
        TextWriter output = pipeWriter ?? Console.Out;
        TextWriter errorOutput = pipeWriter ?? Console.Error;

        EventCollector eventCollector = new()
        {
            HistoryMaxDays = cmdLineArgs.HistoryMaxDays,
        };

        List<string>? errors = null;
        if (cmdLineArgs.DoJsonOutput)
        {
            errors = new();
        }

        if (cmdLineArgs.CollectSystemEvents)
        {
            try
            {
                eventCollector.CollectSystem();
            }
            catch (Exception ex)
            {
                string err = $"Failed to collect system events: {ex}";
                if (errors == null)
                {
                    errorOutput.WriteLine($"{err}\n");
                }
                else
                {
                    errors.Add(err);
                }
            }
        }

        if (cmdLineArgs.CollectSecurityEvents)
        {
            try
            {
                try
                {
                    eventCollector.CollectSecurity();
                }
                catch (UnauthorizedAccessException)
                {
                    if (cmdLineArgs.AllowElevation)
                    {
                        // TODO: Implement
                    }

                    throw;
                }
            }
            catch (Exception ex)
            {
                string err = $"Failed to collect security events: {ex}";
                if (errors == null)
                {
                    errorOutput.WriteLine($"{err}\n");
                }
                else
                {
                    errors.Add(err);
                }
            }
        }

        if (cmdLineArgs.DoJsonOutput)
        {
            PrintJson(eventCollector.Events, errors!, output);
        }
        else
        {
            Print(eventCollector.Events, output);
        }
    }

    private static void Print(IReadOnlyCollection<Event> events, TextWriter output)
    {
        IFormatProvider form = System.Globalization.CultureInfo.CreateSpecificCulture("de-de");
        int lld = events.First().Timestamp.Day;
        foreach (var e in events)
        {
            if (lld != e.Timestamp.Day)
            {
                lld = e.Timestamp.Day;
                output.WriteLine();
            }
            output.WriteLine(e.Timestamp.ToString("ddd, dd.MM.    HH:mm    ", form) + EventTypeName(e.Type));
        }
    }

    private static void PrintJson(IReadOnlyCollection<Event> events, List<string> errors, TextWriter output)
    {
        output.WriteLine(
            JsonSerializer.Serialize(new
            {
                events,
                errors
            }));
    }
}
