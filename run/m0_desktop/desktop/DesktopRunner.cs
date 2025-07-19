using m0.Network.Server;
using m0.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

            MinusZero.Instance.Initialize();

            mainWindow.Init();

            // can not do it directly becouse of circlular references
            //m0_COMPOSER.Runtime.Initialisation.Execute();

            m0_COMPOSER_Runtime_Initialisation_Execute();

            mainWindow.Show();

            m0Main.mainTree.BaseEdgeToUpdated();
        }
    }
}
