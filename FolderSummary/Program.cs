using FolderSummary;
using System.CommandLine;
using System.Text;

internal class Program
{
	private static int Main(string[] args)
	{
		var rootCommand = new RootCommand("Folder Summary Application");
		rootCommand.TreatUnmatchedTokensAsErrors = true;

		var jsonFileArgument = new Argument<FileInfo>("json") { Description = "The summary json file" };
		var folderArgument = new Argument<DirectoryInfo>("dir") { Description = "The directory referenced" }.AcceptExistingOnly();

		var createCommand = new Command("create", description: "Creating a summary file from a folder")
		{
			jsonFileArgument,
			folderArgument
		};

		var overwriteOption = new Option<bool>("--force") { Description = "Overwrite existing an json file" };
		createCommand.Options.Add(overwriteOption);
		createCommand.Validators.Add(result =>
		{
			var json = result.GetRequiredValue(jsonFileArgument);
			if (json.Exists)
			{
				bool? o = result.GetValue(overwriteOption);
				if (o == null || !o.HasValue || o.Value == false)
				{
					result.AddError($"Cannot overwrite existing file without '--force' specified: '{json.FullName}'.");
				}
			}
		});
		createCommand.SetAction((result) =>
		{
			var data = Summary.Scan(result.GetRequiredValue(folderArgument));
			Summary.SaveJson(data, result.GetRequiredValue(jsonFileArgument));

		});
		rootCommand.Subcommands.Add(createCommand);

		var compareCommand = new Command("compare", description: "Compares a summary to a folder")
		{
			jsonFileArgument,
			folderArgument
		};
		var ignoreDateOption = new Option<bool>("--ignore-date") { Description = "Does not report differences in file dates" };
		compareCommand.Options.Add(ignoreDateOption);
		compareCommand.Validators.Add(result =>
		{
			var json = result.GetRequiredValue(jsonFileArgument);
			if (!json.Exists)
			{
				result.AddError($"File does not exist: '{json.FullName}'.");
			}
		});
		compareCommand.SetAction((result) =>
		{
			Console.OutputEncoding = new UTF8Encoding(false);

			DirectoryInfo folder = result.GetRequiredValue(folderArgument);
			FileInfo json = result.GetRequiredValue(jsonFileArgument);
			var found = Summary.Scan(folder);
			var expected = Summary.LoadJson(json);
			Comparer comp = new(json, folder);
			var ignoreDate = result.GetValue(ignoreDateOption);
			comp.IgnoreDate = ignoreDate;
			comp.Compare(expected, found);

		});
		rootCommand.Subcommands.Add(compareCommand);

		return rootCommand.Parse(args).Invoke();
	}
}