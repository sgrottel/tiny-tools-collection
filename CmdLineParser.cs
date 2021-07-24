using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redate
{

	class CmdLineParser
	{

		public Program.RunMode RunMode { get; private set; } = Program.RunMode.Run;

		public string RedateFile { get; private set; } = null;

		public string[] SourceDirs { get; private set; } = null;

		public CmdLineParser(string[] args)
		{

			if (args.Length < 1) throw new ArgumentException("You need to specify a run mode");

			switch (args[0].ToLowerInvariant())
			{
				case "init": RunMode = Program.RunMode.Init; break;
				case "run": RunMode = Program.RunMode.Run; break;
				case "reg": RunMode = Program.RunMode.FileReg; return; // no more args
				case "unreg": RunMode = Program.RunMode.FileUnreg; return; // no more args
				default: throw new ArgumentException("Run mode must be 'init' or 'run'. Found: " + args[0]);
			}

			if (args.Length < 2) throw new ArgumentException("You need to specify a 'redate' file");
			RedateFile = args[1];

			SourceDirs = args.AsSpan(2).ToArray();
			if (SourceDirs.Length == 0 && RunMode == Program.RunMode.Init) throw new ArgumentException("You need to specify source directories information for 'init'");
		}

		static public void PrintHelp()
		{
			Console.WriteLine("Syntax:");
			Console.WriteLine("\tredate.exe init <file.redate> <input ...>");
			Console.WriteLine("\tredate.exe run <file.redate>");
			Console.WriteLine("\tredate.exe reg");
			Console.WriteLine("\tredate.exe unreg");
			Console.WriteLine();
			Console.WriteLine("Init");
			Console.WriteLine("Specify redate file to be created, and one or multiple input source directories.");
			Console.WriteLine();
			Console.WriteLine("Run");
			Console.WriteLine("Specify redate file to run.");
			Console.WriteLine();
			Console.WriteLine("Reg/Unreg");
			Console.WriteLine("Registers redate file type in windows registry, or unregisters the redate file type.");
			Console.WriteLine("This must likely be run with elevated priviliges.");
			Console.WriteLine();
		}

	}

}
