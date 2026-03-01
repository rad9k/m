using m0;
using m0.Bootstrap;
using m0.Network.Server;
using m0_console.console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.console
{
    public class ConsoleRunner
    {
        public static void RUN(string[] args)
        {
            CommandLineParameters commandLineParameters;

            try
            {
                commandLineParameters = CommandLineParameters.Parse(args);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine(CommandLineParameters.GetHelpText("m0_console"));
                return;
            }

            if (commandLineParameters.Help)
            {
                Console.WriteLine(CommandLineParameters.GetHelpText("m0_console"));
                return;
            }

            MinusZero.Instance.CommandLineParameters = commandLineParameters;
            MinusZero.Instance.SetUserInteraction(new ConsoleUserInteraction());

            MinusZero.Instance.Initialize();

            MinusZero.Instance.Initialize_AfterUXInitialized();

            MinusZero.Instance.Dispose();
        }
    }
}
