using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using m0;
using m0.Bootstrap;
using m0.Desktop;

namespace m0_RUN
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            CommandLineParameters commandLineParameters;

            try
            {
                commandLineParameters = CommandLineParameters.Parse(e.Args);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(
                    ex.Message + Environment.NewLine + Environment.NewLine + CommandLineParameters.GetHelpText("m0_desktop"),
                    "m0_desktop",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
                return;
            }

            MinusZero.Instance.CommandLineParameters = commandLineParameters;

            if (commandLineParameters.Help)
            {
                MessageBox.Show(
                    CommandLineParameters.GetHelpText("m0_desktop"),
                    "m0_desktop",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Shutdown();
                return;
            }

            base.OnStartup(e);

            StartWindow startWindow = new StartWindow();          
        }
    }
}
