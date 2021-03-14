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

	}

}
