using m0;
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
        public static void RUN() // Marking the method as async and changing return type to Task
        {
            var server = new HttpServerLibrary();

            server.StartAsync("http://localhost:5000");

            MinusZero.Instance.SetUserInteraction(new ConsoleUserInteraction());

            MinusZero.Instance.Initialize();

            MinusZero.Instance.Initialize_AfterUXInitialized();

            MinusZero.Instance.Dispose();
        }
    }
}
