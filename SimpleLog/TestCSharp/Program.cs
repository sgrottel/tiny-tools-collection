using SGrottel;
using System.Reflection;

internal class Program
{
	private static void Main(string[] args)
	{
		var asm = Assembly.GetExecutingAssembly();

		string logDir = Path.Combine(Path.GetDirectoryName(asm.Location) ?? ".", "log");
		string logName = Path.GetFileNameWithoutExtension(asm.Location);

		EchoingSimpleLog log = new(logDir, logName, 4);

		SimpleLog.Write(log, "Started {0:u}", DateTime.Now);

		SimpleLog.Write(log, "Default Directory: {0}", SimpleLog.GetDefaultDirectory());
		SimpleLog.Write(log, "Default Name: {0}", SimpleLog.GetDefaultName());
		SimpleLog.Write(log, "Default Retention: {0}", SimpleLog.GetDefaultRetention());

		log.Write("And now for something completely different:");
		SimpleLog.Error(log, "An Error");
		SimpleLog.Warning(log, "A Warning");
		SimpleLog.Write(log, "And a normal Message");

		SimpleLog.Write(log, "Formatting away: {0} {1} {2} {3} {4}", new object[]{ "The", "quick", "Fox", "doesn't", "care!"});

		log.Write("Done.");
	}
}
