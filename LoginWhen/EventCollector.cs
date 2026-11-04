using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace LoginWhen;

internal class EventCollector
{
    public enum EventType
    {
        Logon,
        Logoff,
        Lock,
        Unlock,
        Startup,
        Shutdown
    }

    public record Event(DateTime Timestamp, EventType Type);

    public List<Event> Collect(int historyMaxDays)
    {
        DateTime now = DateTime.Now;
        using EventLog systemLog = new("System");
        using EventLog securityLog = new("Security");

        List<Event> events = new();

        foreach (EventLogEntry entry in systemLog.Entries)
        {
            EventType ev;
            if (entry.Source == "Microsoft-Windows-Winlogon")
            {
                if (entry.InstanceId == 7001) ev = EventType.Logon;
                else if (entry.InstanceId == 7002) ev = EventType.Logoff;
                else continue;
            }
            else if (entry.Source == "Microsoft-Windows-Kernel-General")
            {
                if (entry.InstanceId == 12) ev = EventType.Startup;
                else if (entry.InstanceId == 13) ev = EventType.Shutdown;
                else continue;
            }
            else continue;

            DateTime date = entry.TimeGenerated;

            if ((now - date).TotalDays > historyMaxDays) continue;

            events.Add(new(date, ev));
        }

        foreach (EventLogEntry entry in securityLog.Entries)
        {
            if (entry.Source != "Microsoft-Windows-Security-Auditing") continue;

            EventType ev;
            if (entry.InstanceId == 4800) ev = EventType.Lock;
            else if (entry.InstanceId == 4801) ev = EventType.Unlock;
            else continue;

            DateTime date = entry.TimeGenerated;

            if ((now - date).TotalDays > historyMaxDays) continue;

            events.Add(new(date, ev));
        }

        events.Sort((Event a, Event b) => { return a.Timestamp.CompareTo(b.Timestamp); });

        return events;
    }
}
