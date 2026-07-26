using m0.Foundation;
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
        static int InteractionControlReccurection = 0;

        public static int InteractionDepth
        {
            get { return InteractionControlReccurection; }
        }

        public static void BeginInteractionWithGraph()
        {
            if (InteractionControlReccurection == 0)
                ExecutionFlowHelper.StartTransaction();

            InteractionControlReccurection++;
        }

        public static void EndInteractionWithGraph()
        {
            if (InteractionControlReccurection == 1)
                ExecutionFlowHelper.CommitTransaction();

            InteractionControlReccurection--;
        }
    }
}
