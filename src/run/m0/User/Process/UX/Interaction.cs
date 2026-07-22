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
            long t0 = TxPerfLog.Timestamp();
            bool startedRoot = false;

            if (InteractionControlReccurection == 0)
            {
                ExecutionFlowHelper.StartTransaction();
                startedRoot = true;
            }

            InteractionControlReccurection++;

            TxPerfLog.Record("Interaction.Begin", TxPerfLog.Timestamp() - t0,
                startedRoot ? 1 : 0, "root");
        }

        public static void EndInteractionWithGraph()
        {
            long t0 = TxPerfLog.Timestamp();
            bool committedRoot = false;

            if (InteractionControlReccurection == 1)
            {
                ExecutionFlowHelper.CommitTransaction();
                committedRoot = true;
            }

            InteractionControlReccurection--;

            TxPerfLog.Record("Interaction.End", TxPerfLog.Timestamp() - t0,
                committedRoot ? 1 : 0, "rootCommit");
        }
    }
}
