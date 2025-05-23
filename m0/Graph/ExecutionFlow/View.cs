using ICSharpCode.AvalonEdit.Rendering;
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
    class ViewHolder
    {
        static IVertex r = MinusZero.Instance.Root;

        static IVertex viewGenericTransformFunction_eventMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\ViewGenericTransformFunction\event");
        static IVertex viewGenericTransformFunction_fromMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\ViewGenericTransformFunction\from");
        static IVertex viewGenericTransformFunction_toMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\ViewGenericTransformFunction\to");

        public IList<string> FromTriggerQueries = new List<string>();
        public IVertex FromToTransformFunction;
        public IList<string> ToTriggerQueries = new List<string>();
        public IVertex ToFromTransformFunction;

        public ViewHolder(IVertex createViewVertex) {
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

        public void ExecuteFromToTransformFunction(IVertex events, IVertex from, IVertex to)
        {
            IVertex parameters = InstructionHelpers.CreateStack();

            if (events != null)
                parameters.AddEdge(viewGenericTransformFunction_eventMeta, events);

            parameters.AddEdge(viewGenericTransformFunction_fromMeta, from);
            parameters.AddEdge(viewGenericTransformFunction_toMeta, to);

            ZeroCodeExecutonUtil.FuncionCall(FromToTransformFunction, parameters);
        }

        public void ExecuteToFromTransformFunction(IVertex events, IVertex from, IVertex to)
        {
            IVertex parameters = InstructionHelpers.CreateStack();

            if (events != null)
                parameters.AddEdge(viewGenericTransformFunction_eventMeta, events);

            parameters.AddEdge(viewGenericTransformFunction_fromMeta, from);
            parameters.AddEdge(viewGenericTransformFunction_toMeta, to);

            ZeroCodeExecutonUtil.FuncionCall(ToFromTransformFunction, parameters);
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

        public static INoInEdgeInOutVertexVertex CreateView_FromListener(IExecution exe)
        {
            return exe.Stack;
        }

        public static INoInEdgeInOutVertexVertex CreateView_ToListener(IExecution exe)
        {
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

            ViewHolder vh = new ViewHolder(createView);

            if (vh.ToFromTransformFunction != null && vh.FromToTransformFunction != null)
                vh.ExecuteFromToTransformFunction(null, edgeFrom, edgeTo);
            else {
                if (vh.FromToTransformFunction != null)
                    vh.ExecuteFromToTransformFunction(null, edgeFrom, edgeTo);

                if (vh.ToFromTransformFunction != null)
                    vh.ExecuteToFromTransformFunction(null, edgeFrom, edgeTo);
            }

            if (vh.FromToTransformFunction != null)
            {
                IEdge createViewTriggerEdge = GraphChangeTrigger.AddTrigger(edgeFrom,
                vh.FromTriggerQueries,
                new List<GraphChangeFilterEnum> {GraphChangeFilterEnum.InputEdgeAdded,
                     GraphChangeFilterEnum.MetaEdgeAdded},
                "CreateView");

                ExecutionFlowHelper.AddListener_DotNetDelegate(createViewTriggerEdge.To, m0.Graph.ExecutionFlow.View.CreateView_MetaEdgeAdded, "CreateViewMetaEdgeAdded");

            }

            if (vh.ToFromTransformFunction != null)
            {

            }
        }
    }
}
