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
            MinusZero.Instance.SetUserInteraction(new ConsoleUserInteraction());

            HandleArgs(args);

            MinusZero.Instance.Initialize();

            MinusZero.Instance.Initialize_AfterPossibleUXInitialized();

            MinusZero.Instance.Dispose();
        }

        public static void HandleArgs(string[] args)
        {
            CommandLineParameters commandLineParameters;

            try
            {
                commandLineParameters = CommandLineParameters.Parse(args);
            }
            catch (ArgumentException ex)
            {
                MinusZero.Instance.UserInteraction.InteractionOutput("[ERROR] " + ex.Message);

                MinusZero.Instance.UserInteraction.InteractionOutput("[INFO] " + CommandLineParameters.GetHelpText("m0_desktop"));

                return;
            }

            if (commandLineParameters.DoHelp)            
                MinusZero.Instance.UserInteraction.InteractionOutput("[INFO] " + CommandLineParameters.GetHelpText("m0_desktop"));                

            MinusZero.Instance.CommandLineParameters = commandLineParameters;
        }
    }
}
