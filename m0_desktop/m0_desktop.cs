using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.desktop
{
    public class m0_desktop
    {
        public static void RUN()
        {
            MinusZero.Instance.Initialize();

            m0Main m = new m0Main();

            m0_COMPOSER.Runtime.Initialisation.Execute();

            m.Show();

            m0Main.mainTree.BaseEdgeToUpdated();
        }
    }
}
