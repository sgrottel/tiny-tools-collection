using FolderSummary;
using System.CommandLine;

internal class Program
{
	private static int Main(string[] args)
	{
		var rootCommand = new RootCommand("Folder Summary Application");
		rootCommand.TreatUnmatchedTokensAsErrors = true;

		var jsonFileArgument = new Argument<FileInfo>("json", description: "The summary json file");
		var folderArgument = new Argument<DirectoryInfo>("dir", description: "The directory referenced").ExistingOnly();

		var createCommand = new Command("create", description: "Creating a summary file from a folder");
		createCommand.AddArgument(jsonFileArgument);
		createCommand.AddArgument(folderArgument);
		var overwriteOption = new Option<bool>("--force", description: "Overwrite existing an json file");
		createCommand.AddOption(overwriteOption);
		createCommand.AddValidator(result =>
		{
			var json = result.GetValueForArgument(jsonFileArgument);
			if (json.Exists)
			{
				bool? o = result.GetValueForOption(overwriteOption);
				if (o == null || !o.HasValue || o.Value == false)
				{
					result.ErrorMessage = $"Cannot overwrite existing file without '--force' specified: '{json.FullName}'.";
				}
			}
		});
		createCommand.SetHandler((json, folder) =>
		{
			var data = Summary.Scan(folder);
			Summary.SaveJson(data, json);

		}, jsonFileArgument, folderArgument);
		rootCommand.AddCommand(createCommand);

		var compareCommand = new Command("compare", description: "Compares a summary to a folder");
		compareCommand.AddArgument(jsonFileArgument);
		compareCommand.AddArgument(folderArgument);
		var ignoreDateOption = new Option<bool>("--ignore-date", description: "Does not report differences in file dates");
		compareCommand.AddOption(ignoreDateOption);
		compareCommand.AddValidator(result =>
		{
			var json = result.GetValueForArgument(jsonFileArgument);
			if (!json.Exists)
			{
				result.ErrorMessage = $"File does not exist: '{json.FullName}'.";
			}
		});
		compareCommand.SetHandler((json, folder, ignoreDate) =>
		{
			var found = Summary.Scan(folder);
			var expected = Summary.LoadJson(json);

			throw new NotImplementedException();

		}, jsonFileArgument, folderArgument, ignoreDateOption);
		rootCommand.AddCommand(compareCommand);

		return rootCommand.Invoke(args);
	}
}