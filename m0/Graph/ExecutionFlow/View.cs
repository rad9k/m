using ICSharpCode.AvalonEdit.Rendering;
using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    class ViewProperties
    {
        public IList<string> FromTriggerQueries = new List<string>();
        public IVertex FromToTransformFunction;
        public IList<string> ToTriggerQueries = new List<string>();
        public IVertex ToFromTransformFunction;

        public ViewProperties(IVertex createViewVertex) {
            IVertex innerVertex = GraphUtil.GetQueryOutFirst(createViewVertex, "ViewInner", null);

            if (innerVertex == null)
                return;

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
                        FromToTransformFunction = GraphUtil.GetQueryOutFirst(e.To, "Target", null);

                        break;

                    case "ToTriggerQuery":
                        IVertex query2 = GraphUtil.GetQueryOutFirst(e.To, "Query", null);

                        if (query2 == null)
                            continue;

                        ToTriggerQueries.Add(GraphUtil.GetStringValue(query2));
                        break;

                    case "ToFromTransformFunction":
                        ToFromTransformFunction = GraphUtil.GetQueryOutFirst(e.To, "Target", null);

                        break;
                }
            }
        }


    }

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

            
        }
    }
}
