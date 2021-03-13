using System;

namespace Redate
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Redate");
			CmdLineParser cmd = new CmdLineParser(args);
		}
	}
}
