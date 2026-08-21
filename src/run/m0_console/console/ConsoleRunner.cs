using m0;
using m0.Bootstrap;
using m0_console.console;
using System;

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

            // Keep the process alive for the HTTP server AFTER Autostart transaction
            // has been committed. Sleeping inside Autostart (webserver.m0t) left a
            // nested transaction open, so OnlyNonTransacted CreateView (e.g. VertexToJson)
            // was dropped and tree.json stayed empty.
            //MinusZero.Instance.GracefullExitToken.Token.WaitHandle.WaitOne(); 
            // if uncommented, the process will stay alive until the user presses Ctrl+C or closes the console window.

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
