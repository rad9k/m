using ICSharpCode.AvalonEdit.Rendering;
using m0.Foundation;
using m0.Util;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
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

        static IVertex viewGenericTransformFunction_viewEventMeta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\ViewGenericTransformFunction\'viewEvent'");
        static IVertex viewGenericTransformFunction_fromMeta =  r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\ViewGenericTransformFunction\from");
        static IVertex viewGenericTransformFunction_metaMeta =  r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\ViewGenericTransformFunction\'meta'");
        static IVertex viewGenericTransformFunction_toMeta =    r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\ViewGenericTransformFunction\to");

        public IList<string> FromTriggerQueries = new List<string>();
        public IList<GraphChangeFilterEnum> FromFilters = new List<GraphChangeFilterEnum>();
        public IVertex FromToTransformFunction;
        public IList<string> ToTriggerQueries = new List<string>();
        public IList<GraphChangeFilterEnum> ToFilters = new List<GraphChangeFilterEnum>();
        public IVertex ToFromTransformFunction;

        public ViewHolder(IVertex createViewVertex) {
            IVertex innerVertex = GraphUtil.GetQueryOutFirst(createViewVertex, "CreateViewInner", null);

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

                    case "FromTriggerFilter":
                        IVertex value = GraphUtil.GetQueryOutFirst(e.To, "Value", null);

                        if (value == null)
                            continue;

                        FromFilters.Add(GraphChangeFilterEnumHelper.GetEnum(value));
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

                    case "ToTriggerFilter":
                        IVertex value2 = GraphUtil.GetQueryOutFirst(e.To, "Value", null);

                        if (value2 == null)
                            continue;

                        ToFilters.Add(GraphChangeFilterEnumHelper.GetEnum(value2));
                        break;

                    case "ToFromTransformFunction":
                        ToFromTransformFunction = GraphUtil.GetQueryOutFirst(e.To, "Target", null);

                        break;
                }
            }
        }

        public void ExecuteFromToTransformFunction(IExecution exe, IList<IEdge> events, IVertex from, IVertex meta, IVertex to)
        {
            IVertex parameters = InstructionHelpers.CreateStack();

            foreach (IEdge e in events)
                parameters.AddEdge(viewGenericTransformFunction_viewEventMeta, e.To);

            parameters.AddEdge(viewGenericTransformFunction_fromMeta, from);
            parameters.AddEdge(viewGenericTransformFunction_metaMeta, meta);
            parameters.AddEdge(viewGenericTransformFunction_toMeta, to);

            ZeroCodeExecutonUtil.FuncionCall(exe, FromToTransformFunction, parameters);
        }

        public void ExecuteToFromTransformFunction(IExecution exe, IList<IEdge> events, IVertex from, IVertex meta, IVertex to)
        {
            IVertex parameters = InstructionHelpers.CreateStack();

            foreach (IEdge e in events)
                parameters.AddEdge(viewGenericTransformFunction_viewEventMeta, e.To);

            parameters.AddEdge(viewGenericTransformFunction_fromMeta, from);
            parameters.AddEdge(viewGenericTransformFunction_metaMeta, meta);
            parameters.AddEdge(viewGenericTransformFunction_toMeta, to);

            ZeroCodeExecutonUtil.FuncionCall(exe, ToFromTransformFunction, parameters);
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
                    ProcessCreateViewEvent(exe, e);
            }

            return exe.Stack;
        }

        public static void GetViewEventsAndEdge(IExecution exe, out IList<IEdge> viewEvents, out IVertex viewEdge)
        {
            viewEvents = new List<IEdge>();
            viewEdge = null;

            foreach (IEdge e in GraphUtil.GetQueryOut(exe.Stack, "event", null))
            {
                IVertex triggerVertex = GraphUtil.GetQueryOutFirst(e.To, "Trigger", null);

                if (GeneralUtil.CompareStrings(triggerVertex.Value, "View"))
                {
                    viewEvents.Add(e);

                    viewEdge = GraphUtil.GetQueryOutFirst(triggerVertex, "Edge", "ViewEdge");
                }
            }
        }

        public static INoInEdgeInOutVertexVertex CreateView_FromToListener(IExecution exe)
        {
            IList<IEdge> viewEvents;
            IVertex viewEdge;

            GetViewEventsAndEdge(exe, out viewEvents, out viewEdge);

            IVertex metaVertex = GraphUtil.GetQueryOutFirst(viewEdge, "Meta", null);

            IVertex createView = GraphUtil.GetQueryOutFirst(metaVertex, "CreateView", null);

            ViewHolder vh = new ViewHolder(createView);

            vh.ExecuteFromToTransformFunction(exe,
                viewEvents,
                GraphUtil.GetQueryOutFirst(viewEdge, "From", null),
                metaVertex,
                GraphUtil.GetQueryOutFirst(viewEdge, "To", null));

            return exe.Stack;
        }

        public static INoInEdgeInOutVertexVertex CreateView_ToFromListener(IExecution exe)
        {
            IList<IEdge> viewEvents;
            IVertex viewEdge;

            GetViewEventsAndEdge(exe, out viewEvents, out viewEdge);

            IVertex metaVertex = GraphUtil.GetQueryOutFirst(viewEdge, "Meta", null);

            IVertex createView = GraphUtil.GetQueryOutFirst(metaVertex, "CreateView", null);

            ViewHolder vh = new ViewHolder(createView);

            vh.ExecuteToFromTransformFunction(exe,
                viewEvents,
                GraphUtil.GetQueryOutFirst(viewEdge, "From", null),
                metaVertex,
                GraphUtil.GetQueryOutFirst(viewEdge, "To", null));

            return exe.Stack;
        }

        private static void ProcessCreateViewEvent(IExecution exe, IEdge eventEdge)
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
                vh.ExecuteFromToTransformFunction(exe, null, edgeFrom, edgeMeta, edgeTo);
            else {
                if (vh.FromToTransformFunction != null)
                    vh.ExecuteFromToTransformFunction(exe, new List<IEdge>(), edgeFrom, edgeMeta, edgeTo);

                if (vh.ToFromTransformFunction != null)
                    vh.ExecuteToFromTransformFunction(exe, new List<IEdge>(), edgeFrom, edgeMeta, edgeTo);
            }

            if (vh.FromToTransformFunction != null && vh.FromFilters.Count > 0)
            {
                IEdge createViewTriggerEdge = GraphChangeTrigger.AddTrigger(edgeFrom,
                vh.FromTriggerQueries,
                vh.FromFilters,
                "View");

                IVertex createViewTriggerVertex = createViewTriggerEdge.To;

                ExecutionFlowHelper.AddListener_DotNetDelegate(createViewTriggerVertex, 
                    m0.Graph.ExecutionFlow.View.CreateView_FromToListener,
                    "CreateViewFromListener");

                EdgeHelper.AddEdgeVertex(createViewTriggerVertex, edgeFrom, edgeMeta, edgeTo, "ViewEdge");
            }

            if (vh.ToFromTransformFunction != null && vh.ToFilters.Count > 0)
            {
                IEdge createViewTriggerEdge = GraphChangeTrigger.AddTrigger(edgeTo,
                vh.ToTriggerQueries,
                vh.ToFilters,
                "View");

                IVertex createViewTriggerVertex = createViewTriggerEdge.To;

                ExecutionFlowHelper.AddListener_DotNetDelegate(createViewTriggerVertex,
                    m0.Graph.ExecutionFlow.View.CreateView_ToFromListener,
                    "CreateViewToListener");

                EdgeHelper.AddEdgeVertex(createViewTriggerVertex, edgeFrom, edgeMeta, edgeTo, "ViewEdge");
            }
        }
    }
}
