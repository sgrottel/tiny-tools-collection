using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace LoginWhen;

[SupportedOSPlatform("Windows")]
internal class EventCollector
{
    public enum EventType
    {
        Logon,
        Logoff
    }

    public List<KeyValuePair<DateTime, EventType>> Collect(int historyMaxDays)
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

            if ((now - date).TotalDays > historyMaxDays) continue;

            events.Add(new(date, ev));
        }

        events.Sort((KeyValuePair<DateTime, EventType> a, KeyValuePair<DateTime, EventType> b) => { return a.Key.CompareTo(b.Key); });

        return events;
    }
}
