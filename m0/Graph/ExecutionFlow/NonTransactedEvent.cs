using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    public class NonTransactedEvent
    {
        static void FireTrigger(IVertex trigger, GraphChangeTransactionAtom gcta)
        {
            IVertex eventVertex = gcta.CreateEventVertex(trigger, null);            
                

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
