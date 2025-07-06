using System;
using System.CommandLine;
using System.Linq;

namespace LoginWhen
{
    internal class CmdLineArgs
    {

        public int HistoryMaxDays { get; set; } = 7;

        public bool Parse(string[] args)
        {
            RootCommand root = new(description: "Info about user session login/logout times");
            Option<int> historyMaxDays = new(name: "--days") { Description = "Number of days to show" };
            root.Add(historyMaxDays);

            var res = root.Parse(args);
            if (res.Errors.Any())
            {
                Console.Error.WriteLine("Cmd line parsing failed:\n\t" + string.Join("\n\t", res.Errors));
                return false;
            }
            if (res.Action != null)
            {
                // version or help
                res.Invoke();
                return false;
            }

            var hmdr = res.GetResult(historyMaxDays);
            if (hmdr != null && !hmdr.Errors.Any())
            {
                HistoryMaxDays = res.GetValue(historyMaxDays);
            }

            return true;
        }

    }
}
