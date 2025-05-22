using ICSharpCode.AvalonEdit.Rendering;
using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    class View
    {
        public static INoInEdgeInOutVertexVertex CreateView_MetaEdgeAdded(IExecution exe)
        {
            foreach (IEdge e in GraphUtil.GetQueryOut(exe.Stack, "event", null))
            {
                IVertex triggerVertex = GraphUtil.GetQueryOutFirst(e.To, "Trigger", null);

                if (GraphUtil.GetStringValueOrNull(triggerVertex) == "CreateView")
                    ProcessCreateViewEvent(e);
            }

            return exe.Stack;
        }

        private static void ProcessCreateViewEvent(IEdge eventEdge)
        {
            IVertex eventVertex, edge, edgeFrom, edgeMeta, edgeTo, trigger, createView;

            eventVertex = eventEdge.To;

            edge = GraphUtil.GetQueryOutFirst(eventVertex, "Edge", null);

            edgeFrom = GraphUtil.GetQueryOutFirst(edge, "From", null);

            edgeMeta = GraphUtil.GetQueryOutFirst(edge, "Meta", null);

            edgeTo = GraphUtil.GetQueryOutFirst(edge, "To", null);

            trigger = GraphUtil.GetQueryOutFirst(eventVertex, "Trigger", null);

            createView = GraphUtil.GetQueryOutFirst(edgeMeta, "CreateView", null);

            //

            IVertex innerVertex = GraphUtil.GetQueryOutFirst(createView, "ViewInner", null);

            if (innerVertex == null)
                return;

            IList<string> FromTriggerQueries = new List<string>();
            IList<IVertex> FromToTransformFunctions = new List<IVertex>();
            IList<string> ToTriggerQueries = new List<string>();
            IList<IVertex> ToFromTransformFunctions = new List<IVertex>();

            foreach (IEdge e in innerVertex)
            {
                if (GraphUtil.GetStringValue(e.Meta) != "Expression")
                    continue;

                IVertex expressionIs = GraphUtil.GetQueryOutFirst(e.To, "$Is", null);

                if (expressionIs == null)
                    continue;

                switch (GraphUtil.GetStringValue(expressionIs))
                {
                    case "FromTriggerQuery":
                        IVertex query = GraphUtil.GetQueryOutFirst(e.To, "Query", null);

                        if (query == null)
                            continue;

                        FromTriggerQueries.Add(GraphUtil.GetStringValue(query));
                        break;

                    case "FromToTransformFunction":
                        IVertex target = GraphUtil.GetQueryOutFirst(e.To, "Target", null);

                        if (target == null)
                            continue;

                        FromToTransformFunctions.Add(target);
                        break;

                    case "ToTriggerQuery":
                        IVertex query2 = GraphUtil.GetQueryOutFirst(e.To, "Query", null);

                        if (query2 == null)
                            continue;

                        ToTriggerQueries.Add(GraphUtil.GetStringValue(query2));
                        break;

                    case "ToFromTransformFunction":
                        IVertex target2 = GraphUtil.GetQueryOutFirst(e.To, "Target", null);

                        if (target2 == null)
                            continue;

                        ToFromTransformFunctions.Add(target2);
                        break;

                }

                int x = 0;

            }
        }
    }
}
