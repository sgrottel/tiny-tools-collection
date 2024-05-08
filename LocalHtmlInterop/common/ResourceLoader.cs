using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LocalHtmlInterop
{
	public static class ResourceLoader
	{
		public enum Id
		{
			CallbackReceiver_JavaScript,
			CallbackReceiver_Mini_JavaScript
		};

		static public string LoadString(Id id)
		{
			string resourceName;
			switch (id)
			{
				case Id.CallbackReceiver_JavaScript:
					resourceName = "LocalHtmlInterop.Resources.CallbackReceiver.js";
					break;
				case Id.CallbackReceiver_Mini_JavaScript:
					resourceName = "LocalHtmlInterop.Resources.CallbackReceiver_min.js";
					break;
				default:
					throw new ArgumentException($"Using ResourceLoader.Id = {id} is not implemented");
			}

			var assembly = Assembly.GetExecutingAssembly();
			using (Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception("resource not found"))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd().Trim();
			}
		}

	}
}
