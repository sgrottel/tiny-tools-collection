using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LocalHtmlInterop
{
	public class CommandDefinitionFile
	{
		public string? comment { get; set; }

		public CommandDefinition[]? commands { get; set; }

		public static CommandDefinitionFile Load(string fullName)
		{
			using (StreamReader input = new(fullName))
			{
				var yamlDeserializer = new DeserializerBuilder()
					.WithNamingConvention(CamelCaseNamingConvention.Instance)
					.IgnoreUnmatchedProperties()
					.Build();
				var def = yamlDeserializer.Deserialize<CommandDefinitionFile>(input);
				if (def == null) throw new Exception("Deserialization returned null");

				if (def.commands != null)
				{
					string dir = Path.GetDirectoryName(fullName) ?? Environment.CurrentDirectory;
					foreach (var cmd in def.commands)
					{
						cmd.Validate(dir);
					}
				}

				return def;
			}
		}
	}
}
