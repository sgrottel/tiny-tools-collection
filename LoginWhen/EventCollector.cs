using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

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

    private List<Event> _events = new();

    public required int HistoryMaxDays { get; set; }

    public IReadOnlyCollection<Event> Events { get => _events; }

    public void Clear()
    {
        _events.Clear();
    }

    private string TimeFilter {
        get {
            return FormattableString.Invariant($"TimeCreated[timediff(@SystemTime) <= {(long)HistoryMaxDays * 86400000}]");
        }
    }

    public void CollectSystem()
    {
        EventLogQuery systemQuery = new("System", PathType.LogName, $"""
            *[System[
                ((Provider[@Name='Microsoft-Windows-Winlogon'] and (EventID=7001 or EventID=7002))
                or (Provider[@Name='Microsoft-Windows-Kernel-General'] and (EventID=12 or EventID=13)))
                and {TimeFilter}
            ]]
            """);
        using EventLogReader systemLog = new(systemQuery);

        while (true)
        {
            using EventRecord entry = systemLog.ReadEvent();
            if (entry == null) break;

            EventType ev;
            if (entry.ProviderName == "Microsoft-Windows-Winlogon")
            {
                if (entry.Id == 7001) ev = EventType.Logon;
                else if (entry.Id == 7002) ev = EventType.Logoff;
                else continue;
            }
            else if (entry.ProviderName == "Microsoft-Windows-Kernel-General")
            {
                if (entry.Id == 12) ev = EventType.Startup;
                else if (entry.Id == 13) ev = EventType.Shutdown;
                else continue;
            }
            else continue;

            if (entry.TimeCreated is not DateTime date) continue;

            _events.Add(new(date.ToLocalTime(), ev));
        }
        SortEvents(_events);
    }

    public void CollectSecurity()
    {
        EventLogQuery securityQuery = new("Security", PathType.LogName, $"""
            *[System[
                Provider[@Name='Microsoft-Windows-Security-Auditing']
                and (EventID=4800 or EventID=4801)
                and {TimeFilter}
            ]]
            """);
        using EventLogReader securityLog = new(securityQuery);

        while (true)
        {
            using EventRecord entry = securityLog.ReadEvent();
            if (entry == null) break;

            EventType ev;
            if (entry.Id == 4800) ev = EventType.Lock;
            else if (entry.Id == 4801) ev = EventType.Unlock;
            else continue;

            if (entry.TimeCreated is not DateTime date) continue;

            _events.Add(new(date.ToLocalTime(), ev));
        }
        SortEvents(_events);
    }

    public static void SortEvents(List<Event> events)
    {
        events.Sort((Event a, Event b) => { return a.Timestamp.CompareTo(b.Timestamp); });
    }
}
