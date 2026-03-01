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
        public static StartupEventArgs args;

        protected override void OnStartup(StartupEventArgs e)
        {
            args = e;
            
            base.OnStartup(e);

            StartWindow startWindow = new StartWindow();     
        }
    }
}
