using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using static LoginWhen.EventCollector;

namespace LoginWhen;

class Program
{
    static string EventTypeName(EventType et)
    {
        switch(et)
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
        try
        {
            eventCollector.CollectSystem();
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"Failed to collect system events: {ex}\n");
        }
        try
        {
            try
            {
                eventCollector.CollectSecurity();
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"Failed to collect security events: {ex}\n");
        }

        IFormatProvider form = System.Globalization.CultureInfo.CreateSpecificCulture("de-de");
        int lld = eventCollector.Events.First().Timestamp.Day;
        foreach (var e in eventCollector.Events)
        {
            if (lld != e.Timestamp.Day)
            {
                lld = e.Timestamp.Day;
                Console.WriteLine();
            }
            Console.WriteLine(e.Timestamp.ToString("ddd, dd.MM.    HH:mm    ", form) + EventTypeName(e.Type));
        }

    }
}
