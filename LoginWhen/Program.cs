using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace LoginWhen
{

    [SupportedOSPlatform("Windows")]
    class Program
    {
        enum EventType
        {
            Logon,
            Logoff
        }

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
            DateTime now = DateTime.Now;
            EventLog systemLog = new("System");

            List<KeyValuePair<DateTime, EventType>> events = new();

            foreach (EventLogEntry entry in systemLog.Entries)
            {
                if (entry.Source != "Microsoft-Windows-Winlogon") continue;

                EventType ev;
                if (entry.InstanceId == 7001) ev = EventType.Logon;
                else if (entry.InstanceId == 7002) ev = EventType.Logoff;
                else continue;

                DateTime date = entry.TimeGenerated;

                if ((now - date).TotalDays > 14) continue;

                events.Add(new(date, ev));
            }

            events.Sort((KeyValuePair<DateTime, EventType> a, KeyValuePair<DateTime, EventType> b) => { return -a.Key.CompareTo(b.Key); });

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
}
