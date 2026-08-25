using System.CommandLine;

namespace Redate;

class CmdLineParser
{

	public Program.RunMode RunMode { get; private set; } = Program.RunMode.None;

	public string RedateFile { get; private set; } = null;

	public string[] SourceDirs { get; private set; } = null;

	public bool ForceFileDateUpdate { get; private set; } = false;

	public CmdLineParser(string[] args)
	{
		Argument<string> redateFileNameArgument = new(name: "file.redate");
		Argument<string[]> inputPathsArgument = new(name: "input");

		Command initCommand = new(name: "init", description: "Specify redate file to be created, and one or multiple input source directories.")
		{
			redateFileNameArgument,
			inputPathsArgument,
		};
		initCommand.SetAction((pr) =>
		{
			RedateFile = pr.GetRequiredValue(redateFileNameArgument);
			SourceDirs = pr.GetRequiredValue(inputPathsArgument);
			RunMode = Program.RunMode.Init;
		});

		Option<bool> forceFileDateUpdateOption = new(name: "-forcefiledateupdate")
		{
			Description = "Activates the legacy behavior: The field `FileDate` will be updated, even if the remaining content stays the same"
		};

		Command runCommand = new(name: "run", description: "Updated file dates and redate file content")
		{
			redateFileNameArgument,
			forceFileDateUpdateOption
		};
		runCommand.SetAction((pr) =>
		{
			RedateFile = pr.GetRequiredValue(redateFileNameArgument);
			ForceFileDateUpdate = pr.GetValue(forceFileDateUpdateOption);
			RunMode = Program.RunMode.Run;
		});

		Command regCommand = new(name: "reg", description: "Registers redate file type in windows registry. This must likely be run with elevated priviliges.");
		regCommand.SetAction((pr) =>
		{
			RunMode = Program.RunMode.FileReg;
		});

		Command unregCommand = new(name: "unreg", description: "Unregisters redate file type from the windows registry. This must likely be run with elevated priviliges.");
		unregCommand.SetAction((pr) =>
		{
			RunMode = Program.RunMode.FileUnreg;
		});

		RootCommand root = new(description: "Rewrite dates of files")
		{
			initCommand,
			runCommand,
			regCommand,
			unregCommand,
		};

		var parseResult = root.Parse(args, configuration: new ParserConfiguration() { EnablePosixBundling = false });
		parseResult.Invoke(configuration: new InvocationConfiguration() { EnableDefaultExceptionHandler = false });
	}
}
