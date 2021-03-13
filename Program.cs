using System;

namespace Redate
{
	class Program
	{
		internal enum RunMode
		{
			Init,
			Run,
			FileReg,
			FileUnreg
		};

		static int Main(string[] args)
		{
			Console.WriteLine("Redate");
			try
			{
				CmdLineParser cmd = new CmdLineParser(args);





			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("Fatal error: " + ex);
				return -1;
			}
			return 0;
		}
	}
}
