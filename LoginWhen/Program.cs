using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using static LoginWhen.EventCollector;

namespace LoginWhen;


[SupportedOSPlatform("Windows")]
class Program
{
    static string EventTypeName(EventType et)
    {
        switch(et)
        {
            case EventType.Logon:
                return "[  -->] Log On";
            case EventType.Logoff:
                return "[<--  ] Log Off";
        }
        return "";
    }

    static void Main(string[] args)
    {
        CmdLineArgs cmdLineArgs = new();
        if (!cmdLineArgs.Parse(args)) return;

        EventCollector eventCollector = new();
        List<KeyValuePair<DateTime, EventType>> events = eventCollector.Collect(cmdLineArgs.HistoryMaxDays);

        IFormatProvider form = System.Globalization.CultureInfo.CreateSpecificCulture("de-de");
        int lld = events[0].Key.Day;
        foreach (var e in events)
        {
            if (lld != e.Key.Day)
            {
                lld = e.Key.Day;
                Console.WriteLine();
            }
            Console.WriteLine(e.Key.ToString("ddd, dd.MM.    HH:mm    ", form) + EventTypeName(e.Value));
        }

    }
}
