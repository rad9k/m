using m0.Foundation;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    public class NonTransactedEvent
    {
        static void FireTrigger(IVertex triggerVertex, GraphChangeTransactionAtom gcta)
        {
            IVertex eventVertex = gcta.CreateEventVertex(triggerVertex, null);

            foreach (IEdge e in triggerVertex.GetAll(false, @"Listener:"))
            {
                IVertex parameters = InstructionHelpers.CreateStack();

                foreach (IVertex eventVertex in kvp.Value)
                    parameters.AddEdge(GenericEventHandler_event_meta, eventVertex);

                ZeroCodeExecutonUtil.FuncionCall(exe, e.To, parameters);
            }
        }

        public static void HandleOutEdgeValueChange(GraphChangeTransactionAtom gcta)
        {
            IVertex changedVeretx = gcta.ChangedVertex;

            foreach(IEdge trigger in GraphUtil.GetQueryOut(changedVeretx, "$GraphChangeTrigger", null))
                switch (gcta.Type)
                {
                    case AtomGraphChangeTypeEnum.ValueChange:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "ValueChange"))
                            FireTrigger(trigger.To, gcta);
                        break;

                    case AtomGraphChangeTypeEnum.EdgeAdded:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "OutputEdgeAdded"))
                            FireTrigger(trigger.To, gcta);
                        break;

                    case AtomGraphChangeTypeEnum.EdgeRemoved:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "OutputEdgeRemoved"))
                            FireTrigger(trigger.To, gcta);
                        break;

                    case AtomGraphChangeTypeEnum.OutputEdgeDisposed:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "OutputEdgeDisposed"))
                            FireTrigger(trigger.To, gcta);
                        break;
                }                                        
        }

        public static void HandleInEdge(GraphChangeTransactionAtom gcta)
        {
            IVertex changedVeretx = gcta.ChangedVertex;

            foreach (IEdge trigger in GraphUtil.GetQueryOut(changedVeretx, "$GraphChangeTrigger", null))
                switch (gcta.Type)
                {                    
                    case AtomGraphChangeTypeEnum.EdgeAdded:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "InputEdgeAdded"))
                            FireTrigger(trigger.To, gcta);
                        break;

                    case AtomGraphChangeTypeEnum.EdgeRemoved:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "InputEdgeRemoved"))
                            FireTrigger(trigger.To, gcta);
                        break;
                }
        }


        public static void HandleMetaEdge(GraphChangeTransactionAtom gcta)
        {
            IVertex changedVeretx = gcta.ChangedVertex;

            foreach (IEdge trigger in GraphUtil.GetQueryOut(changedVeretx, "$GraphChangeTrigger", null))
                switch (gcta.Type)
                {
                    case AtomGraphChangeTypeEnum.EdgeAdded:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "MetaEdgeAdded"))
                            FireTrigger(trigger.To, gcta);
                        break;

                    case AtomGraphChangeTypeEnum.EdgeRemoved:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "MetaEdgeRemoved"))
                            FireTrigger(trigger.To, gcta);
                        break;
                }
        }
    }
}
