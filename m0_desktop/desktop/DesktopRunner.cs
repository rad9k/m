using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.desktop
{
    public class DesktopRunner
    {
        public static void RUN()
        {
            MinusZero.Instance.Initialize();

            m0Main mainWindow = new m0Main();

            MinusZero.Instance.SetUserInteraction(mainWindow);

            //m0_COMPOSER.Runtime.Initialisation.Execute();

            mainWindow.Show();

            m0Main.mainTree.BaseEdgeToUpdated();
        }
    }
}
