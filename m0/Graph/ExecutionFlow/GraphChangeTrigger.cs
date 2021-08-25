using m0.DotNetIntegration;
using m0.Foundation;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System.Collections.Generic;

namespace m0.Graph.ExecutionFlow
{
    public enum GraphChangeTypeEnum { ValueChange, OutputEdgeAdded, OutputEdgeRemoved, InputEdgeAdded, InputEdgeRemoved, OutputEdgeDisposed };

    public class GraphChangeTrigger
    {
        static IVertex graphChangeTrigger_meta;
        static IVertex scopeQuery_meta;
        static IVertex changeTypeFilter_meta;
        static IVertex graphChangeEnum_ValueChange_meta;
        static IVertex graphChangeEnum_OutputEdgeAdded_meta;
        static IVertex graphChangeEnum_OutputEdgeRemoved_meta;
        static IVertex graphChangeEnum_InputEdgeAdded_meta;
        static IVertex graphChangeEnum_InputEdgeRemoved_meta;
        static IVertex graphChangeEnum_OutputEdgeDisposed_meta;

        public static void Initialize()
        {
            IVertex r = m0.MinusZero.Instance.root;

            graphChangeTrigger_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\$GraphChangeTrigger");
            scopeQuery_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\$GraphChangeTrigger\ScopeQuery");
            changeTypeFilter_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\$GraphChangeTrigger\ChangeTypeFilter");
            graphChangeEnum_OutputEdgeAdded_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\OutputEdgeAdded");
            graphChangeEnum_OutputEdgeRemoved_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\OutputEdgeRemoved");
            graphChangeEnum_InputEdgeAdded_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\InputEdgeAdded");
            graphChangeEnum_InputEdgeRemoved_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\nputEdgeRemoved");
            graphChangeEnum_OutputEdgeDisposed_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\OutputEdgeDisposed");
        }

        public static IEdge AddEventTriggerAndListener(IVertex baseVertex, 
            IList<string> scopeQueries,
            IList<GraphChangeTypeEnum> changeTypeFilter,
            string triggerVertexName, 
            ExecutionFlowHelper.DotNetDelegate _delegate, 
            string listenerName)
        {
            IEdge graphChangeTriggerEdge = GraphChangeTrigger.AddGraphChangeTrigger(baseVertex, 
                scopeQueries, 
                changeTypeFilter, 
                triggerVertexName);

            return ExecutionFlowHelper.AddListener_DotNetDelegate(graphChangeTriggerEdge.To, _delegate, listenerName);
        }

        public static IEdge AddGraphChangeTrigger(IVertex baseVertex, 
            IList<string> scopeQueries, 
            IList<GraphChangeTypeEnum> changeTypeFilter)
        {
            return AddGraphChangeTrigger(baseVertex, scopeQueries, changeTypeFilter, null);
        }

        public static IEdge AddGraphChangeTrigger(IVertex baseVertex, 
            IList<string> scopeQueries,
            IList<GraphChangeTypeEnum> changeTypeFilter,
            string triggerVertexName)
        {
            IEdge triggerEdge = null;

            if (triggerVertexName != null)
            {
                IVertex existingTriggers = baseVertex.GetAll(false, "$GraphChangeTrigger:" + triggerVertexName);

                if (existingTriggers.OutEdges.Count == 1)
                    triggerEdge = existingTriggers.OutEdges[0];
            }

            if (triggerEdge == null)
            {
                triggerEdge = VertexOperations.AddInstanceAndReturnEdge(baseVertex, graphChangeTrigger_meta);
                triggerEdge.To.Value = triggerVertexName;
            }

            if (scopeQueries != null)
                foreach (string s in scopeQueries)
                    triggerEdge.To.AddVertex(scopeQuery_meta, s);

            if(changeTypeFilter != null)
                foreach(GraphChangeTypeEnum ct in changeTypeFilter)
                {
                    switch (ct)
                    {
                        case GraphChangeTypeEnum.ValueChange:
                            triggerEdge.To.AddVertex(changeTypeFilter_meta, graphChangeEnum_ValueChange_meta);
                            break;

                        case GraphChangeTypeEnum.InputEdgeAdded:
                            triggerEdge.To.AddVertex(changeTypeFilter_meta, graphChangeEnum_InputEdgeAdded_meta);
                            break;

                        case GraphChangeTypeEnum.InputEdgeRemoved:
                            triggerEdge.To.AddVertex(changeTypeFilter_meta, graphChangeEnum_InputEdgeRemoved_meta);
                            break;

                        case GraphChangeTypeEnum.OutputEdgeAdded:
                            triggerEdge.To.AddVertex(changeTypeFilter_meta, graphChangeEnum_OutputEdgeAdded_meta);
                            break;

                        case GraphChangeTypeEnum.OutputEdgeRemoved:
                            triggerEdge.To.AddVertex(changeTypeFilter_meta, graphChangeEnum_OutputEdgeRemoved_meta);
                            break;

                        case GraphChangeTypeEnum.OutputEdgeDisposed:
                            triggerEdge.To.AddVertex(changeTypeFilter_meta, graphChangeEnum_OutputEdgeDisposed_meta);
                            break;
                    }
                }

            return triggerEdge;
        }

        public static void RemoveGraphChangeListener(IEdge listenerEdge)
        {
            IVertex triggerVertex = listenerEdge.From;

            triggerVertex.DeleteEdge(listenerEdge);

            if (GraphUtil.GetQueryOutCount(triggerVertex, "Listener", null) == 0)
            {
                IEdge triggerSourceEdge = GraphUtil.GetQueryInFirstEdge(triggerVertex, "$GraphChangeTrigger", null);

                if (triggerSourceEdge != null)
                    triggerSourceEdge.From.DeleteEdge(triggerSourceEdge);
            }
        }

    }
}
