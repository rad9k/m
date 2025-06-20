using m0;
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
        public static void RUN()
        {
            MinusZero.Instance.SetUserInteraction(new ConsoleUserInteraction());

            MinusZero.Instance.Initialize();

            MinusZero.Instance.Initialize_AfterUXInitialized();
        }
    }
}
