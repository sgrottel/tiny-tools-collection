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
            Logoff,
            Lock,
            Unlock
        }

        static string EventTypeName(EventType et)
        {
            switch(et)
            {
                case EventType.Logon:
                    return "[  -->] Log On";
                case EventType.Logoff:
                    return "[<--  ] Log Off";
                case EventType.Lock:
                    return "[<-   ] Lock";
                case EventType.Unlock:
                    return "[   ->] Unlock";
            }
            return "";
        }

        static void Main(string[] args)
        {
            CmdLineArgs cmdLineArgs = new();
            if (!cmdLineArgs.Parse(args)) return;

            const long SystemEventIdLogon = 7001;
            const long SystemEventIdLogoff = 7002;
            const long SecurityEventIdLock = 4800;
            const long SecurityEventIdUnlock = 4801;

            DateTime now = DateTime.Now;
            List<(DateTime time, EventType type)> events = new();

            using (EventLog systemLog = new("System"))
            {
                foreach (EventLogEntry entry in systemLog.Entries)
                {
                    if (entry.Source != "Microsoft-Windows-Winlogon") continue;

                    EventType ev;
                    if (entry.InstanceId == SystemEventIdLogon) ev = EventType.Logon;
                    else if (entry.InstanceId == SystemEventIdLogoff) ev = EventType.Logoff;
                    else continue;

                    DateTime date = entry.TimeGenerated;
                	if ((now - date).TotalDays > cmdLineArgs.HistoryMaxDays) continue;

                    events.Add(new(date, ev));
                }
            }
            // Needs read access to `REG: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EventLog\Security`
            // Added 'read' of 'Authenticated Users'
            using (EventLog securityLog = new("Security"))
            {
                // Needs further access rights
                foreach (EventLogEntry entry in securityLog.Entries)
                {
                    EventType ev;
                    if (entry.InstanceId == SecurityEventIdLock) ev = EventType.Lock;
                    else if (entry.InstanceId == SecurityEventIdUnlock) ev = EventType.Unlock;
                    else continue;

                    DateTime date = entry.TimeGenerated;
                	if ((now - date).TotalDays > cmdLineArgs.HistoryMaxDays) continue;

                    events.Add(new(date, ev));
                }
            }

            events.Sort(((DateTime time, EventType type) a, (DateTime time, EventType type) b) => { return a.time.CompareTo(b.time); });

            IFormatProvider form = System.Globalization.CultureInfo.CreateSpecificCulture("de-de");
            int lld = events[0].time.Day;
            foreach (var e in events)
            {
                if (lld != e.time.Day)
                {
                    lld = e.time.Day;
                    Console.WriteLine();
                }
                Console.WriteLine(e.time.ToString("ddd, dd.MM.    HH:mm    ", form) + EventTypeName(e.type));
            }

        }
    }
}
