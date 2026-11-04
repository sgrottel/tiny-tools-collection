using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static LoginWhen.EventCollector;

namespace LoginWhen;

internal static class ElevationUtility
{
    public const int CommunicationTimeoutMilliseconds = 30000;

    public static bool IsElevated
    {
        get
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    public static async Task<Event[]> CollectSecurityEventsAsync(int historyMaxDays)
    {
        if (IsElevated)
        {
            throw new InvalidOperationException("The process is already elevated.");
        }

        string pipeName = $"LoginWhen-{Guid.NewGuid():N}";
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        PipeSecurity security = new();
        security.SetAccessRuleProtection(true, false);
        security.AddAccessRule(new PipeAccessRule(
            identity.User ?? throw new InvalidOperationException("Cannot determine the current user."),
            PipeAccessRights.FullControl, AccessControlType.Allow));
        security.AddAccessRule(new PipeAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
            PipeAccessRights.FullControl, AccessControlType.Allow));
        using NamedPipeServerStream pipe = NamedPipeServerStreamAcl.Create(
            pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous, 0, 0, security, HandleInheritability.None, 0);

        string processPath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Cannot determine the executable path.");
        ProcessStartInfo startInfo = new(processPath)
        {
            UseShellExecute = true,
            Verb = "runas",
            WindowStyle = ProcessWindowStyle.Hidden,
        };
        if (string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.OrdinalIgnoreCase))
        {
            startInfo.ArgumentList.Add(Path.GetFullPath(Environment.GetCommandLineArgs()[0]));
        }
        startInfo.ArgumentList.Add("--days");
        startInfo.ArgumentList.Add(historyMaxDays.ToString(CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add("--no-system-events");
        startInfo.ArgumentList.Add("--with-security-events");
        startInfo.ArgumentList.Add("--no-elevation");
        startInfo.ArgumentList.Add("--json");
        startInfo.ArgumentList.Add("--output-pipe");
        startInfo.ArgumentList.Add(pipeName);

        using CancellationTokenSource cancellation = new();
        Task connection = pipe.WaitForConnectionAsync(cancellation.Token);
        try
        {
            using Process worker = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Failed to start the elevated worker.");
            cancellation.CancelAfter(CommunicationTimeoutMilliseconds);
            Task exit = worker.WaitForExitAsync(cancellation.Token);
            if (await Task.WhenAny(connection, exit) == exit && !connection.IsCompletedSuccessfully)
            {
                await exit;
                throw new IOException($"The elevated worker exited before connecting (exit code {worker.ExitCode}).");
            }
            await connection;

            using StreamReader reader = new(pipe);
            string json = await reader.ReadToEndAsync(cancellation.Token);
            await exit;
            if (worker.ExitCode != 0)
            {
                throw new IOException($"The elevated worker failed (exit code {worker.ExitCode}).");
            }

            using JsonDocument result = JsonDocument.Parse(json);
            string[] errors = result.RootElement.GetProperty("errors").Deserialize<string[]>()
                ?? throw new InvalidDataException("The elevated worker did not return errors.");
            if (errors.Length != 0)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            }
            return result.RootElement.GetProperty("events").Deserialize<Event[]>()
                ?? throw new InvalidDataException("The elevated worker did not return events.");
        }
        catch (OperationCanceledException ex) when (cancellation.IsCancellationRequested)
        {
            throw new TimeoutException("Timed out waiting for the elevated worker.", ex);
        }
        finally
        {
            cancellation.Cancel();
        }
    }
}
