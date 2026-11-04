using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using static LoginWhen.EventCollector;

namespace LoginWhen;

internal class LockLog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter<EventType>() },
    };

    private readonly string _filePath;
    private readonly string _mutexName;

    public LockLog(string? filePath = null)
    {
        _filePath = Path.GetFullPath(filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LoginWhen", "events.jsonl"));
        _mutexName = @"Global\LoginWhen-LockLog-" + Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(_filePath.ToUpperInvariant())));
    }

    public void LogLock()
    {
        Write(EventType.Lock);
    }

    public void LogUnlock()
    {
        Write(EventType.Unlock);
    }

    public IReadOnlyCollection<Event> Query(int historyMaxDays)
    {
        List<Event> events = new();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        WithLock(() =>
        {
            if (!File.Exists(_filePath)) return;

            foreach (string line in File.ReadLines(_filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                Event entry = JsonSerializer.Deserialize<Event>(line, JsonOptions)
                    ?? throw new InvalidDataException("The lock log contains an invalid event.");
                if (entry.Type is not (EventType.Lock or EventType.Unlock))
                {
                    throw new InvalidDataException("The lock log contains an unsupported event type.");
                }

                if ((now - entry.Timestamp).TotalDays > historyMaxDays) continue;

                events.Add(entry);
            }
        });
        SortEvents(events);
        return events;
    }

    public static IReadOnlyCollection<Event> MergeEvents(IReadOnlyCollection<Event> events, IReadOnlyCollection<Event> lockEvents)
    {
        List<Event> merged = new(events);
        merged.AddRange(lockEvents.Where(logEvent => !events.Any(e =>
            e.Type == logEvent.Type && (e.Timestamp - logEvent.Timestamp).Duration() <= TimeSpan.FromSeconds(1))));
        SortEvents(merged);
        return merged;
    }

    private void Write(EventType type)
    {
        Event entry = new(DateTimeOffset.UtcNow, type);
        WithLock(() =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.AppendAllText(_filePath, JsonSerializer.Serialize(entry, JsonOptions) + Environment.NewLine);
        });
    }

    private void WithLock(Action action)
    {
        using Mutex mutex = new(false, _mutexName);
        try
        {
            mutex.WaitOne();
        }
        catch (AbandonedMutexException)
        {
            // An abandoned mutex is still acquired by this thread.
        }

        try
        {
            action();
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }
}
