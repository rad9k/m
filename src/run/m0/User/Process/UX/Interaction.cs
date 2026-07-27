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
            {
                MinusZero.Instance.Log(1, "Interaction.BeginInteractionWithGraph",
                    "starting nested transaction recursion=0 -> 1 before="
                    + m0.Lib.Sys.DescribeTransactionStack());
                ExecutionFlowHelper.StartTransaction();
            }

            InteractionControlReccurection++;

            MinusZero.Instance.Log(1, "Interaction.BeginInteractionWithGraph",
                "AFTER recursion=" + InteractionControlReccurection
                + " " + m0.Lib.Sys.DescribeTransactionStack());
        }

        public static void EndInteractionWithGraph()
        {
            MinusZero.Instance.Log(1, "Interaction.EndInteractionWithGraph",
                "BEFORE recursion=" + InteractionControlReccurection
                + " " + m0.Lib.Sys.DescribeTransactionStack());

            if (InteractionControlReccurection == 1)
                ExecutionFlowHelper.CommitTransaction();

            InteractionControlReccurection--;
        }
    }
}
