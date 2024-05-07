using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace LocalHtmlInterop
{
	public class CommandDefinition
	{
		public string? name { get; set; }

		public class Executable
		{
			public string? path { get; set; }

			[YamlIgnore]
			public string? FullPath { get; private set; }

			internal void Validate(string basePath)
			{
				if (string.IsNullOrWhiteSpace(path))
				{
					FullPath = null;
					return;
				}

				FullPath = FindExecutable.FindExecutable.FullPath(path, additionalPaths: [basePath]);
				if (string.IsNullOrEmpty(FullPath))
				{
					FullPath = null;
					return;
				}
			}

			public static implicit operator Executable(string p) => new() { path = p };

		}

		public Executable? exec { get; set; }

		public string? workdir { get; set; }

		[YamlIgnore]
		public string? WorkingDirectory { get; set; }

		public class Argument
		{
			public string? value { get; set; }

			public string? param { get; set; }

			public class Parameter
			{
				public string? name { get; set; }

				public bool? required { get; set; }

				public static implicit operator Parameter(string n) => new() { name = n };

			}

			[YamlMember(Alias = "params")]
			public Parameter[]? parameters {get; set;}

			public bool? required { get; set; }

			public static implicit operator Argument(string v) => new() { value = v };

			private static Regex valueParamRefRegEx = new(@"\{\{\s*([^\}]+)\s*\}\}");

			public string? Validate()
			{
				if (value == null)
				{
					if (param == null)
					{
						if (parameters == null || parameters.Length != 1)
						{
							return "Without explicit `value`, exactly one `param` must be specified";
						}
					}
					else
					{
						if (parameters != null && parameters.Length != 0)
						{
							return "Without explicit `value`, exactly one `param` must be specified";
						}
					}
					// value is empty, one param is give == ok
					return null;
				}

				List<Parameter> ps = new();
				if (param != null) ps.Add(param);
				if (parameters != null) ps.AddRange(parameters);

				HashSet<string> refedParams = new();

				MatchCollection ms = valueParamRefRegEx.Matches(value);
				foreach (Match m in ms)
				{
					if (!m.Success)
					{
						return "Failed to evaluate parameter reference regex on `value`";
					}

					string pr = m.Groups[1].Value;

					Parameter? p = ps.Find((p) => p.name?.Equals(pr, StringComparison.InvariantCultureIgnoreCase) ?? false);
					if (p == null)
					{
						return $"Parameter `{pr}` referenced but not listed";
					}

					refedParams.Add(pr.ToLowerInvariant());
				}

				foreach (var p in ps)
				{
					if (p.name == null)
					{
						return "You cannot list a parameter without a name";
					}

					if (refedParams.Contains(p.name.ToLowerInvariant())) continue;

					return $"Parameter `{p.name}` listed but not referenced";
				}

				return null;
			}

			public string Interpolate(Dictionary<string, string> parameters)
			{
				return valueParamRefRegEx.Replace(
					value ?? string.Empty,
					(m) =>
					{
						return parameters[m.Groups[1].Value];
					});
			}
		}

		public Argument[]? args { get; set; }

		[YamlIgnore]
		public string? ValidationError { get; private set; }

		public bool Validate(string basePath)
		{
			ValidationError = null;

			if (string.IsNullOrWhiteSpace(name))
			{
				ValidationError ??= "Command must specify its `name`";
			}

			exec?.Validate(basePath);

			if (exec?.FullPath == null)
			{
				ValidationError ??= $"Executable `exec:` {exec?.path} not found";
			}

			WorkingDirectory = evaluateWorkDir(basePath);
			if (!Directory.Exists(WorkingDirectory))
			{
				ValidationError ??= $"Working Directory {workdir} not found.\n\t(resolved to: {WorkingDirectory})";
			}

			if (args?.Length > 0)
			{
				foreach (var a in args)
				{
					var valErr = a.Validate();
					if (!string.IsNullOrWhiteSpace(valErr))
					{
						ValidationError ??= $"Invalid argument {a}: {valErr}";
					}
				}
			}

			return ValidationError == null;
		}

		private string? evaluateWorkDir(string basePath)
		{
			if (workdir == null)
			{
				return Environment.CurrentDirectory;
			}
			if (Path.IsPathFullyQualified(workdir))
			{
				return workdir;
			}
			else
			{
				return Path.GetFullPath(Path.Combine(basePath, workdir));
			}
		}

	}
}
