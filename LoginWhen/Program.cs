using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
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
                    Console.Error.WriteLine($"{err}\n");
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
                    Console.Error.WriteLine($"{err}\n");
                }
                else
                {
                    errors.Add(err);
                }
            }
        }

        if (cmdLineArgs.DoJsonOutput)
        {
            PrintJson(eventCollector.Events, errors);
        }
        else
        {
            Print(eventCollector.Events);
        }
    }

    private static void Print(IReadOnlyCollection<Event> events)
    {
        IFormatProvider form = System.Globalization.CultureInfo.CreateSpecificCulture("de-de");
        int lld = events.First().Timestamp.Day;
        foreach (var e in events)
        {
            if (lld != e.Timestamp.Day)
            {
                lld = e.Timestamp.Day;
                Console.WriteLine();
            }
            Console.WriteLine(e.Timestamp.ToString("ddd, dd.MM.    HH:mm    ", form) + EventTypeName(e.Type));
        }
    }

    private static void PrintJson(IReadOnlyCollection<Event> events, List<string> errors)
    {
        Console.WriteLine(
            JsonSerializer.Serialize(new
            {
                events,
                errors
            }));
    }
}
