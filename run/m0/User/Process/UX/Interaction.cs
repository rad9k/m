using m0.Foundation;
using m0.Graph.ExecutionFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0.Graph.ExecutionFlow;

namespace m0.User.Process.UX
{
    public class Interaction
    {
        static int InteractionControlReccurection = 0;

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

    /*

    public class Interaction
    {
        static int InteractionControlReccurection = 0;

        static int cnt = 0;

        static void PrintCNT()
        {
            MinusZero.Instance.Log(1, "LogWatchState", "CNT" + cnt++);
        }
        static void LogWatchState()
        {
            _LogWatchState(MinusZero.Instance.GetTopTransaction());
        }

        static void _log_one(Dictionary<IVertex, List<GraphChangeTransactionAtom>> o)
        {
            return;

            foreach(IVertex k in o.Keys)
            {
                MinusZero.Instance.Log(1, "LogWatchState", "        " + k.Value);

                foreach(GraphChangeTransactionAtom a in o[k])
                {
                    string s = "            Type:" + a.Type
                        + " ChangedVertex:" + a.ChangedVertex;

                    if (a.Edge != null) s += " Edge.From:" + a.Edge.From.Value
                        + " Edge.Meta:" + a.Edge.Meta.Value
                        + " Edge.To:" + a.Edge.To.Value;

                    s += " OldValue:" + a.OldValue
                        + " NewValue: " + a.NewValue;

                    MinusZero.Instance.Log(1, "LogWatchState", "            " + s);

                }                    
            }
        }

        static void _LogWatchState(ITransaction _t)
        {
            Transaction t = (Transaction)_t;

            MinusZero.Instance.Log(1, "LogWatchState", "    ..." + t.GetHashCode());
            MinusZero.Instance.Log(1, "LogWatchState", "    graphChangeTransactionAtoms_OutEdgeValueChange " + t.graphChangeTransactionAtoms_OutEdgeValueChange.Count);
            _log_one(t.graphChangeTransactionAtoms_OutEdgeValueChange);
            MinusZero.Instance.Log(1, "LogWatchState", "    graphChangeTransactionAtoms_InEdge " + t.graphChangeTransactionAtoms_InEdge.Count);
            _log_one(t.graphChangeTransactionAtoms_InEdge);
            MinusZero.Instance.Log(1, "LogWatchState", "    graphChangeTransactionAtoms_MetaEdge " + t.graphChangeTransactionAtoms_MetaEdge.Count);
            _log_one(t.graphChangeTransactionAtoms_MetaEdge);

            if (t.Previous != null)
                _LogWatchState(t.Previous);
        }

        public static void BeginInteractionWithGraph()
        {
            PrintCNT();

            MinusZero.Instance.Log(1, "BEGINInteractionWithGraph", "start InteractionControlReccurection: "+ InteractionControlReccurection);            

            if (InteractionControlReccurection == 0)
            {
                LogWatchState();
                ExecutionFlowHelper.StartTransaction();
                MinusZero.Instance.Log(1, "BEGINInteractionWithGraph", "    START----TRANSACTION");
                LogWatchState();
            }
            else
                MinusZero.Instance.Log(1, "BEGINInteractionWithGraph", "    ----");

            InteractionControlReccurection++;            

            MinusZero.Instance.Log(1, "BEGINInteractionWithGraph", "stop");
        }

        public static void EndInteractionWithGraph()
        {
            PrintCNT();

            MinusZero.Instance.Log(1, "ENDInteractionWithGraph", "start InteractionControlReccurection: " + InteractionControlReccurection);            

            if (InteractionControlReccurection == 1)
            {
                LogWatchState();
                ExecutionFlowHelper.CommitTransaction();
                MinusZero.Instance.Log(1, "ENDInteractionWithGraph", "    COMMIT----TRANSACTION");
                LogWatchState();
            }
            else
                MinusZero.Instance.Log(1, "ENDInteractionWithGraph", "    ----");

            InteractionControlReccurection--;            

            MinusZero.Instance.Log(1, "ENDInteractionWithGraph", "stop");
        }
    }*/
}
