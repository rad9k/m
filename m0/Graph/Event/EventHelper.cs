using m0.DotNetIntegration;
using m0.Foundation;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.Event
{
    public class EventHelper
    {
        public static Execute(IVertex baseVertex, IExecution exe)
        {
            bool dummy;

            if (InstructionHelpers.CheckIfIsInherits(baseVertex, "Executable"))
                return CallableEndPointDictionary_INIEIOV_ZCE.CallEndPoint(exe, baseVertex);
            else
                return InstructionHelpers.SequentiallyExecuteInstructions(exe, exe.Stack, baseVertex, out dummy, false);
        }
    }
}
