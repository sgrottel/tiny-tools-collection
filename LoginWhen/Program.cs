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
        CmdLineArgs.Operation op = cmdLineArgs.Parse(args);

        using NamedPipeClientStream? outputPipe = cmdLineArgs.OutputPipeName != null
            ? new(".", cmdLineArgs.OutputPipeName, PipeDirection.Out)
            : null;
        outputPipe?.Connect(ElevationUtility.CommunicationTimeoutMilliseconds);
        using StreamWriter? pipeWriter = outputPipe != null
            ? new(outputPipe) { AutoFlush = true }
            : null;
        TextWriter output = pipeWriter ?? Console.Out;
        TextWriter errorOutput = pipeWriter ?? Console.Error;

        try
        {
            switch (op)
            {
                case CmdLineArgs.Operation.Normal:
                    RunNormal(cmdLineArgs, output, errorOutput);
                    break;

                case CmdLineArgs.Operation.LogLock:
                    new LockLog().LogLock();
                    break;

                case CmdLineArgs.Operation.LogUnlock:
                    new LockLog().LogUnlock();
                    break;

                case CmdLineArgs.Operation.None:
                    break;

                default:
                    throw new InvalidOperationException($"Operation {op} not implemented");
            }
        }
        catch (Exception ex)
        {
            string err = $"Failed with exception: {ex}";
            if (cmdLineArgs.DoJsonOutput)
            {
                errorOutput.WriteLine(
                    JsonSerializer.Serialize(new
                    {
                        errors = new string[] { err },
                    }));
            }
            else
            {
                errorOutput.WriteLine(err);
            }
        }
    }

    private static void RunNormal(CmdLineArgs cmdLineArgs, TextWriter output, TextWriter errorOutput)
    {
        EventCollector eventCollector = new()
        {
            HistoryMaxDays = cmdLineArgs.HistoryMaxDays,
        };
        IReadOnlyCollection<Event> events = eventCollector.Events;

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
                when (cmdLineArgs.AllowElevation && !ElevationUtility.IsElevated)
                {
                    var securityEvents = ElevationUtility.CollectSecurityEventsAsync(cmdLineArgs.HistoryMaxDays).Result;

                    List<Event> allEvents = new();
                    events = allEvents;
                    allEvents.AddRange(eventCollector.Events);
                    allEvents.AddRange(securityEvents);
                    SortEvents(allEvents);
                    events = allEvents;
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

        try
        {
            events = LockLog.MergeEvents(events, new LockLog().Query(cmdLineArgs.HistoryMaxDays));
        }
        catch (Exception ex)
        {
            string err = $"Failed to collect lock log events: {ex}";
            if (errors == null)
            {
                errorOutput.WriteLine($"{err}\n");
            }
            else
            {
                errors.Add(err);
            }
        }

        if (cmdLineArgs.DoJsonOutput)
        {
            PrintJson(events, errors!, output);
        }
        else
        {
            Print(events, output);
        }
    }

    private static void Print(IReadOnlyCollection<Event> events, TextWriter output)
    {
        if (events.Count == 0) return;

        IFormatProvider form = System.Globalization.CultureInfo.CreateSpecificCulture("de-de");
        int lld = events.First().Timestamp.ToLocalTime().Day;
        foreach (var e in events)
        {
            DateTimeOffset timestamp = e.Timestamp.ToLocalTime();
            if (lld != timestamp.Day)
            {
                lld = timestamp.Day;
                output.WriteLine();
            }
            output.WriteLine(timestamp.ToString("ddd, dd.MM.    HH:mm    ", form) + EventTypeName(e.Type));
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
