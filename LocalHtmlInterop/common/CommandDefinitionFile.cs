using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

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
				return yamlDeserializer.Deserialize<CommandDefinitionFile>(input);
			}
		}
	}
}
