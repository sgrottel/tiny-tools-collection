using System;
using System.Collections.Generic;
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

        EventCollector eventCollector = new();
        List<Event> events = eventCollector.Collect(cmdLineArgs.HistoryMaxDays);

        IFormatProvider form = System.Globalization.CultureInfo.CreateSpecificCulture("de-de");
        int lld = events[0].Timestamp.Day;
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
}
