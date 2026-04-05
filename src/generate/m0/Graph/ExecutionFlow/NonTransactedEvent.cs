using m0.Foundation;
using m0.ZeroCode;
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
        static void FireTrigger(GraphChangeTransactionAtom gcta, IVertex triggerVertex, EdgeDirectionEnum edgeDirection)
        {
            IExecution exe = new ZeroCodeExecution();

            ExecutionFlowHelper.GraphChangeWatchOff();

            IVertex eventVertex = gcta.CreateEventVertex_GraphChange(triggerVertex, gcta.ChangedVertex, edgeDirection);

            ExecutionFlowHelper.GraphChangeWatchOn();

            foreach (IEdge e in triggerVertex.GetAll(false, @"Listener:"))
            {
                IVertex parameters = InstructionHelpers.CreateStack();
                
                parameters.AddEdge(Transaction.GenericEventHandler_event_meta, eventVertex);

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
                            FireTrigger(gcta, trigger.To, EdgeDirectionEnum.Out);
                        break;

                    case AtomGraphChangeTypeEnum.EdgeAdded:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "OutputEdgeAdded"))
                            FireTrigger(gcta, trigger.To, EdgeDirectionEnum.Out);
                        break;

                    case AtomGraphChangeTypeEnum.EdgeRemoved:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "OutputEdgeRemoved"))
                            FireTrigger(gcta, trigger.To, EdgeDirectionEnum.Out);
                        break;

                    case AtomGraphChangeTypeEnum.OutputEdgeDisposed:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "OutputEdgeDisposed"))
                            FireTrigger(gcta, trigger.To, EdgeDirectionEnum.Out);
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
                            FireTrigger(gcta, trigger.To, EdgeDirectionEnum.In);
                        break;

                    case AtomGraphChangeTypeEnum.EdgeRemoved:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "InputEdgeRemoved"))
                            FireTrigger(gcta, trigger.To, EdgeDirectionEnum.In);
                        break;
                }
        }

        public static void HandleMetaEdge(GraphChangeTransactionAtom gcta)
        {
            //IVertex changedVeretx = gcta.ChangedVertex;
            IVertex changedVeretx = gcta.Edge.Meta;

            foreach (IEdge trigger in GraphUtil.GetQueryOut(changedVeretx, "$GraphChangeTrigger", null))
                switch (gcta.Type)
                {
                    case AtomGraphChangeTypeEnum.EdgeAdded:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "MetaEdgeAdded"))
                            FireTrigger(gcta, trigger.To, EdgeDirectionEnum.Meta);
                        break;

                    case AtomGraphChangeTypeEnum.EdgeRemoved:
                        if (GraphUtil.ExistQueryOut(trigger.To, "ChangeTypeFilter", "MetaEdgeRemoved"))
                            FireTrigger(gcta, trigger.To, EdgeDirectionEnum.Meta);
                        break;
                }
        }
    }
}
