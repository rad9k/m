using m0.Bootstrap;
using m0.Network.Server;
using m0.Util;
using m0_RUN;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace m0.Desktop
{
    public class DesktopRunner
    {
        static void m0_COMPOSER_Runtime_Initialisation_Execute()
        {
            string path = AppContext.BaseDirectory + Path.DirectorySeparatorChar + "m0_COMPOSER.dll";

            DynamicMethodInvoker.InvokeMethod(path, "m0_COMPOSER.Runtime.Initialisation", "Execute");
        }

        public static void RUN()
        {
            m0Main mainWindow = new m0Main();

            MinusZero.Instance.SetUserInteraction(mainWindow);

            HandleArgs();            

            MinusZero.Instance.Initialize();

            mainWindow.Init();

            // can not do it directly becouse of circlular references
            //m0_COMPOSER.Runtime.Initialisation.Execute();

            m0_COMPOSER_Runtime_Initialisation_Execute();

            mainWindow.Show();

            m0Main.mainTree.BaseEdgeToUpdated();
        }

        //

        public static void HandleArgs()
        {
            CommandLineParameters commandLineParameters;

            try
            {
                commandLineParameters = CommandLineParameters.Parse(App.args.Args);
            }
            catch (ArgumentException ex)
            {
                MinusZero.Instance.UserInteraction.InteractionOutput(ex.Message + Environment.NewLine + Environment.NewLine + CommandLineParameters.GetHelpText("m0_desktop"));

                // left in case user interaction code do not work

                /*MessageBox.Show(
                    ex.Message + Environment.NewLine + Environment.NewLine + CommandLineParameters.GetHelpText("m0_desktop"),
                    "m0_desktop",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);*/
                
                return;
            }

            MinusZero.Instance.CommandLineParameters = commandLineParameters;

            if (commandLineParameters.DoHelp)
            {
                MinusZero.Instance.UserInteraction.InteractionOutput(CommandLineParameters.GetHelpText("m0_desktop"));

                // left in case user interaction code do not work

                /*MessageBox.Show(
                    CommandLineParameters.GetHelpText("m0_desktop"),
                    "m0_desktop",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);*/

                return;
            }
        }
    }
}
