using m0.DotNetIntegration;
using m0.Foundation;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System.Collections.Generic;

namespace m0.Graph.ExecutionFlow
{
    public enum GraphChangeFilterEnum { NoBaseVertex, ValueChange, OutputEdgeAdded, OutputEdgeRemoved, InputEdgeAdded, InputEdgeRemoved, OutputEdgeDisposed };

    public class GraphChangeTrigger
    {
        static IVertex graphChangeTrigger_meta;
        static IVertex scopeQuery_meta;
        static IVertex changeTypeFilter_meta;
        static IVertex graphChangeFilterEnum_NoBaseVertex_meta;
        static IVertex graphChangeFilterEnum_ValueChange_meta;
        static IVertex graphChangeFilterEnum_OutputEdgeAdded_meta;
        static IVertex graphChangeFilterEnum_OutputEdgeRemoved_meta;
        static IVertex graphChangeFilterEnum_InputEdgeAdded_meta;
        static IVertex graphChangeFilterEnum_InputEdgeRemoved_meta;
        static IVertex graphChangeFilterEnum_OutputEdgeDisposed_meta;

        public static void Initialize()
        {
            IVertex r = m0.MinusZero.Instance.root;

            graphChangeTrigger_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\$GraphChangeTrigger");
            scopeQuery_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\$GraphChangeTrigger\ScopeQuery");
            changeTypeFilter_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\$GraphChangeTrigger\ChangeTypeFilter");

            graphChangeFilterEnum_NoBaseVertex_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\NoBaseVertex");
            graphChangeFilterEnum_ValueChange_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\ValueChange");
            graphChangeFilterEnum_OutputEdgeAdded_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\OutputEdgeAdded");
            graphChangeFilterEnum_OutputEdgeRemoved_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\OutputEdgeRemoved");
            graphChangeFilterEnum_InputEdgeAdded_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\InputEdgeAdded");
            graphChangeFilterEnum_InputEdgeRemoved_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\nputEdgeRemoved");
            graphChangeFilterEnum_OutputEdgeDisposed_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeFilterEnum\OutputEdgeDisposed");
        }

        public static IEdge AddEventTriggerAndListener(IVertex baseVertex, 
            IList<string> scopeQueries,
            IList<GraphChangeFilterEnum> changeTypeFilter,
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
            IList<GraphChangeFilterEnum> changeTypeFilter)
        {
            return AddGraphChangeTrigger(baseVertex, scopeQueries, changeTypeFilter, null);
        }

        public static IEdge AddGraphChangeTrigger(IVertex baseVertex, 
            IList<string> scopeQueries,
            IList<GraphChangeFilterEnum> changeTypeFilter,
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
                foreach(GraphChangeFilterEnum ct in changeTypeFilter)
                {
                    switch (ct)
                    {
                        case GraphChangeFilterEnum.NoBaseVertex:
                            triggerEdge.To.AddEdge(changeTypeFilter_meta, graphChangeFilterEnum_NoBaseVertex_meta);
                            break;

                        case GraphChangeFilterEnum.ValueChange:
                            triggerEdge.To.AddEdge(changeTypeFilter_meta, graphChangeFilterEnum_ValueChange_meta);
                            break;

                        case GraphChangeFilterEnum.InputEdgeAdded:
                            triggerEdge.To.AddEdge(changeTypeFilter_meta, graphChangeFilterEnum_InputEdgeAdded_meta);
                            break;

                        case GraphChangeFilterEnum.InputEdgeRemoved:
                            triggerEdge.To.AddEdge(changeTypeFilter_meta, graphChangeFilterEnum_InputEdgeRemoved_meta);
                            break;

                        case GraphChangeFilterEnum.OutputEdgeAdded:
                            triggerEdge.To.AddEdge(changeTypeFilter_meta, graphChangeFilterEnum_OutputEdgeAdded_meta);
                            break;

                        case GraphChangeFilterEnum.OutputEdgeRemoved:
                            triggerEdge.To.AddEdge(changeTypeFilter_meta, graphChangeFilterEnum_OutputEdgeRemoved_meta);
                            break;

                        case GraphChangeFilterEnum.OutputEdgeDisposed:
                            triggerEdge.To.AddVertex(changeTypeFilter_meta, graphChangeFilterEnum_OutputEdgeDisposed_meta);
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
