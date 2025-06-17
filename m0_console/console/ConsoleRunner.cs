using m0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.console
{
    public class ConsoleRunner
    {
        public static void RUN()
        {
            MinusZero.Instance.Initialize();



           // MinusZero.Instance.SetUserInteraction(mainWindow);

            MinusZero.Instance.Initialize_AfterUXInitialized();
        }
    }
}
