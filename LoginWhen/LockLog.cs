using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
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

    public static void RegisterLogTasks(TextWriter output)
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        string executablePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Cannot determine the executable path.");
        string applicationPath = string.Equals(Path.GetFileNameWithoutExtension(executablePath), "dotnet", StringComparison.OrdinalIgnoreCase)
            ? Path.GetFullPath(Environment.GetCommandLineArgs()[0])
            : executablePath;

        Type schedulerType = Type.GetTypeFromProgID("Schedule.Service")
            ?? throw new InvalidOperationException("Windows Task Scheduler is not available.");
        dynamic scheduler = Activator.CreateInstance(schedulerType)
            ?? throw new InvalidOperationException("Cannot create the Task Scheduler service.");
        scheduler.Connect();
        RegisterLogTasks(scheduler, identity, executablePath, applicationPath, output);
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

    private static void RegisterLogTasks(dynamic scheduler, WindowsIdentity identity, string executablePath, string applicationPath, TextWriter output)
    {
        const int TaskCreateOrUpdate = 6;
        const int TaskLogonInteractiveToken = 3;
        const int TaskRunLevelLeastPrivilege = 0;
        const int TaskTriggerSessionStateChange = 11;
        const int TaskSessionLock = 7;
        const int TaskSessionUnlock = 8;
        const int TaskActionExec = 0;
        const int TaskInstancesParallel = 0;

        string userSid = identity.User?.Value
            ?? throw new InvalidOperationException("Cannot determine the current user.");
        string userId = identity.Name;
        dynamic folder = scheduler.GetFolder(@"\");
        string argumentPrefix = string.Equals(executablePath, applicationPath, StringComparison.OrdinalIgnoreCase)
            ? ""
            : $"\"{applicationPath}\" ";

        foreach (var (name, command, stateChange) in new[]
        {
            ("Lock", "log-lock", TaskSessionLock),
            ("Unlock", "log-unlock", TaskSessionUnlock),
        })
        {
            string taskName = $"LoginWhen-{name}-{userSid}";
            bool exists = false;
            try
            {
                folder.GetTask(taskName);
                exists = true;
            }
            catch (Exception ex) when (ex.HResult == unchecked((int)0x80070002))
            {
                // The task does not exist yet.
            }

            dynamic task = scheduler.NewTask(0);
            task.RegistrationInfo.Description = $"Records workstation {name.ToLowerInvariant()} events for LoginWhen.";
            task.Principal.UserId = userId;
            task.Principal.LogonType = TaskLogonInteractiveToken;
            task.Principal.RunLevel = TaskRunLevelLeastPrivilege;
            task.Settings.Enabled = true;
            task.Settings.DisallowStartIfOnBatteries = false;
            task.Settings.StopIfGoingOnBatteries = false;
            task.Settings.MultipleInstances = TaskInstancesParallel;

            dynamic trigger = task.Triggers.Create(TaskTriggerSessionStateChange);
            trigger.UserId = userId;
            trigger.StateChange = stateChange;
            trigger.Enabled = true;

            dynamic action = task.Actions.Create(TaskActionExec);
            action.Path = executablePath;
            action.Arguments = argumentPrefix + command;
            action.WorkingDirectory = Path.GetDirectoryName(applicationPath);

            folder.RegisterTaskDefinition(taskName, task, TaskCreateOrUpdate, userId, null, TaskLogonInteractiveToken, null);
            output.WriteLine($"{(exists ? "Replaced" : "Created")} task '\\{taskName}': \"{executablePath}\" {action.Arguments}");
        }
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
