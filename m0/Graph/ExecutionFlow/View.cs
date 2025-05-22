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

        private static void ProcessCreateViewEvent(IEdge e)
        {
            IVertex evt, edge, edgeFrom, edgeTo, view;

            evt = e.To;

            edge = GraphUtil.GetQueryOutFirst(evt, "Edge", null);

            edgeFrom = GraphUtil.GetQueryOutFirst(edge, "From", null);

            edgeFrom = GraphUtil.GetQueryOutFirst(edge, "To", null);

            view = GraphUtil.GetQueryOutFirst(evt, "Trigger", null);


        }
    }
}
