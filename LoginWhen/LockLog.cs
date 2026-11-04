using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
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
    private const long MaxFileSizeBytes = 1024 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter<EventType>() },
    };

    private readonly string _filePath;
    private readonly string _previousFilePath;
    private readonly string _mutexName;
    private readonly SecurityIdentifier _userSid;

    public LockLog(string? filePath = null, string? userSid = null)
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        _userSid = new SecurityIdentifier(userSid ?? identity.User?.Value
            ?? throw new InvalidOperationException("Cannot determine the current user."));
        _filePath = Path.GetFullPath(filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LoginWhen", "events.jsonl"));
        _previousFilePath = Path.ChangeExtension(_filePath, "previous" + Path.GetExtension(_filePath));
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

    public void LogSecurityEvents()
    {
        EventCollector collector = new() { HistoryMaxDays = int.MaxValue };
        collector.CollectSecurityLogons(_userSid.Value);
        AppendNewEvents(collector.Events);
    }

    private void AppendNewEvents(IReadOnlyCollection<Event> events)
    {
        WithLock(() =>
        {
            List<Event> existing = ReadEvents();
            foreach (Event entry in MergeEvents(existing, events).Except(existing))
            {
                Append(entry);
            }
        });
    }

    public static void RegisterLogTasks(TextWriter output, string? userSid = null, string? userName = null, string? logFilePath = null)
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        userSid = new SecurityIdentifier(userSid ?? identity.User?.Value
            ?? throw new InvalidOperationException("Cannot determine the current user.")).Value;
        userName ??= identity.Name;
        logFilePath = Path.GetFullPath(logFilePath ?? new LockLog()._filePath);
        if (!ElevationUtility.IsElevated)
        {
            output.Write(ElevationUtility.RegisterLogTasksAsync(userSid, userName, logFilePath).GetAwaiter().GetResult());
            return;
        }

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
        RegisterLogTasks(scheduler, userSid, userName, logFilePath, executablePath, applicationPath, output);
    }

    public IReadOnlyCollection<Event> Query(int historyMaxDays)
    {
        List<Event> events = new();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        WithLock(() => events.AddRange(ReadEvents().Where(entry => (now - entry.Timestamp).TotalDays <= historyMaxDays)));
        SortEvents(events);
        return events;
    }

    private List<Event> ReadEvents()
    {
        List<Event> events = new();
        foreach (string filePath in new[] { _previousFilePath, _filePath })
        {
            if (!File.Exists(filePath)) continue;

            foreach (string line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                Event entry = JsonSerializer.Deserialize<Event>(line, JsonOptions)
                    ?? throw new InvalidDataException("The lock log contains an invalid event.");
                if (entry.Type is not (EventType.Lock or EventType.Unlock or EventType.Logon or EventType.Logoff))
                {
                    throw new InvalidDataException("The lock log contains an unsupported event type.");
                }

                events.Add(entry);
            }
        }
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

    private static void RegisterLogTasks(dynamic scheduler, string userSid, string userId, string logFilePath, string executablePath, string applicationPath, TextWriter output)
    {
        const int TaskCreateOrUpdate = 6;
        const int TaskLogonInteractiveToken = 3;
        const int TaskLogonServiceAccount = 5;
        const int TaskRunLevelLeastPrivilege = 0;
        const int TaskRunLevelHighest = 1;
        const int TaskTriggerLogon = 9;
        const int TaskTriggerSessionStateChange = 11;
        const int TaskSessionLock = 7;
        const int TaskSessionUnlock = 8;
        const int TaskActionExec = 0;
        const int TaskInstancesParallel = 0;

        dynamic folder = scheduler.GetFolder(@"\");
        string argumentPrefix = string.Equals(executablePath, applicationPath, StringComparison.OrdinalIgnoreCase)
            ? ""
            : $"\"{applicationPath}\" ";

        foreach (var (name, command, stateChange) in new[]
        {
            ("Lock", "log-lock", TaskSessionLock),
            ("Unlock", "log-unlock", TaskSessionUnlock),
            ("Security", $"log-security-events --user-sid \"{userSid}\" --log-file \"{logFilePath}\"", 0),
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
            bool securityTask = name == "Security";
            string taskUserId = securityTask ? @"NT AUTHORITY\SYSTEM" : userId;
            int logonType = securityTask ? TaskLogonServiceAccount : TaskLogonInteractiveToken;
            task.Principal.UserId = taskUserId;
            task.Principal.LogonType = logonType;
            task.Principal.RunLevel = securityTask ? TaskRunLevelHighest : TaskRunLevelLeastPrivilege;
            task.Settings.Enabled = true;
            task.Settings.DisallowStartIfOnBatteries = false;
            task.Settings.StopIfGoingOnBatteries = false;
            task.Settings.MultipleInstances = TaskInstancesParallel;

            dynamic trigger = task.Triggers.Create(securityTask ? TaskTriggerLogon : TaskTriggerSessionStateChange);
            trigger.UserId = userId;
            if (!securityTask) trigger.StateChange = stateChange;
            trigger.Enabled = true;

            dynamic action = task.Actions.Create(TaskActionExec);
            action.Path = executablePath;
            action.Arguments = argumentPrefix + command;
            action.WorkingDirectory = Path.GetDirectoryName(applicationPath);

            folder.RegisterTaskDefinition(taskName, task, TaskCreateOrUpdate, taskUserId, null, logonType, null);
            output.WriteLine($"{(exists ? "Replaced" : "Created")} task '\\{taskName}': \"{executablePath}\" {action.Arguments}");
        }
    }

    private void Write(EventType type)
    {
        Event entry = new(DateTimeOffset.UtcNow, type);
        WithLock(() => Append(entry));
    }

    private void Append(Event entry)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        FileInfo file = new(_filePath);
        if (file.Exists && file.Length >= MaxFileSizeBytes)
        {
            File.Move(_filePath, _previousFilePath, overwrite: true);
        }
        File.AppendAllText(_filePath, JsonSerializer.Serialize(entry, JsonOptions) + Environment.NewLine);
    }

    private void WithLock(Action action)
    {
        MutexSecurity security = new();
        security.AddAccessRule(new MutexAccessRule(_userSid, MutexRights.FullControl, AccessControlType.Allow));
        security.AddAccessRule(new MutexAccessRule(
            new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), MutexRights.FullControl, AccessControlType.Allow));
        security.AddAccessRule(new MutexAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), MutexRights.FullControl, AccessControlType.Allow));
        using Mutex mutex = MutexAcl.Create(false, _mutexName, out _, security);
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
