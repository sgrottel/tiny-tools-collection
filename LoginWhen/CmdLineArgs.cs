using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginWhen
{
    internal class CmdLineArgs
    {

        public int HistoryMaxDays { get; set; } = 7;

        public bool Parse(string[] args)
        {
            RootCommand root = new("Info about user session login/logout times");
            Option<int> historyMaxDays = new("--days", "Number of days to show");
            root.Add(historyMaxDays);

            bool ok = false;

            root.SetHandler((InvocationContext ctxt) =>
            {
                if (ctxt.ParseResult.HasOption(historyMaxDays))
                {
                    HistoryMaxDays = ctxt.ParseResult.GetValueForOption(historyMaxDays);
                }

                ok = true;
            });

            root.Invoke(args);

            return ok;
        }

    }
}
