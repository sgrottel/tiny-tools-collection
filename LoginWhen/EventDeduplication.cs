using System;
using System.Collections.Generic;
using static LoginWhen.EventCollector;

namespace LoginWhen;

internal class EventDeduplication
{
    public TimeSpan FilterTimeSpan { get; set; } = TimeSpan.FromSeconds(30);

    public IReadOnlyCollection<Event> Filter(IReadOnlyCollection<Event> events)
    {
        List<Event> filtered = new();
        Event? lastLogonEvent = null;
        foreach (Event entry in events)
        {
            if (entry.Type is EventType.Startup or EventType.Shutdown)
            {
                lastLogonEvent = null;
            }
            else if (entry.Type is EventType.Logon or EventType.Logoff)
            {
                if (lastLogonEvent != null
                    && entry.Type == lastLogonEvent.Type
                    && entry.Timestamp - lastLogonEvent.Timestamp <= FilterTimeSpan)
                {
                    continue;
                }
                lastLogonEvent = entry;
            }
            filtered.Add(entry);
        }
        return filtered;
    }
}
