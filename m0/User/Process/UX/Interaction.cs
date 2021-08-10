using m0.Graph.ExecutionFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.User.Process.UX
{
    public class Interaction
    {
        public static void BeginInteractionWithGraph()
        {
            ExecutionFlowHelper.StartTransaction();
        }

        public static void EndInteractionWithGraph()
        {
            ExecutionFlowHelper.CommitTransaction();
        }
    }
}
