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
        public IList<GraphChangeFilterEnum> FromTriggerFilters = new List<GraphChangeFilterEnum>();
        public IList<IVertex> FromToTransformFunctions = new List<IVertex>();
        public IList<string> ToTriggerQueries = new List<string>();
        public IList<GraphChangeFilterEnum> ToTriggerFilters = new List<GraphChangeFilterEnum>();
        public IList<IVertex> ToFromTransformFunctions = new List<IVertex>();

        public ViewHolder(IVertex viewVertex) {
            foreach (IEdge e in viewVertex)
            {                
                switch (GraphUtil.GetStringValue(e.Meta))
                {
                    case "FromTriggerQuery":                        

                        FromTriggerQueries.Add(GraphUtil.GetStringValue(e.To));
                        break;

                    case "FromTriggerFilter":                        

                        FromTriggerFilters.Add(GraphChangeFilterEnumHelper.GetEnum(e.To));
                        break;

                    case "FromToTransformFunction":
                        FromToTransformFunctions.Add(e.To);

                        break;

                    case "ToTriggerQuery":                        

                        ToTriggerQueries.Add(GraphUtil.GetStringValue(e.To));
                        break;

                    case "ToTriggerFilter":                        

                        ToTriggerFilters.Add(GraphChangeFilterEnumHelper.GetEnum(e.To));
                        break;

                    case "ToFromTransformFunction":

                        ToFromTransformFunctions.Add(e.To);
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

            foreach (IVertex function in FromToTransformFunctions)
                ZeroCodeExecutonUtil.FuncionCall(exe, function, parameters);
        }

        public void ExecuteToFromTransformFunction(IExecution exe, IList<IEdge> events, IVertex from, IVertex meta, IVertex to)
        {
            IVertex parameters = InstructionHelpers.CreateStack();

            foreach (IEdge e in events)
                parameters.AddEdge(viewGenericTransformFunction_viewEventMeta, e.To);

            parameters.AddEdge(viewGenericTransformFunction_fromMeta, from);
            parameters.AddEdge(viewGenericTransformFunction_metaMeta, meta);
            parameters.AddEdge(viewGenericTransformFunction_toMeta, to);

            foreach (IVertex function in ToFromTransformFunctions)
                ZeroCodeExecutonUtil.FuncionCall(exe, function, parameters);
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
            IVertex eventVertex, edge, edgeFrom, edgeMeta, edgeTo, trigger, view;

            eventVertex = eventEdge.To;

            edge = GraphUtil.GetQueryOutFirst(eventVertex, "Edge", null);

            edgeFrom = GraphUtil.GetQueryOutFirst(edge, "From", null);

            edgeMeta = GraphUtil.GetQueryOutFirst(edge, "Meta", null);

            edgeTo = GraphUtil.GetQueryOutFirst(edge, "To", null);

            trigger = GraphUtil.GetQueryOutFirst(eventVertex, "Trigger", null);

            view = GraphUtil.GetQueryOutFirst(edgeMeta, "View", null);

            //

            ViewHolder vh = new ViewHolder(view);

            if (vh.ToFromTransformFunctions.Count > 0 && vh.FromToTransformFunctions.Count > 0)
                vh.ExecuteFromToTransformFunction(exe, null, edgeFrom, edgeMeta, edgeTo);
            else {
                if (vh.FromToTransformFunctions.Count > 0)
                    vh.ExecuteFromToTransformFunction(exe, new List<IEdge>(), edgeFrom, edgeMeta, edgeTo);

                if (vh.ToFromTransformFunctions.Count > 0)
                    vh.ExecuteToFromTransformFunction(exe, new List<IEdge>(), edgeFrom, edgeMeta, edgeTo);
            }

            if (vh.FromToTransformFunctions.Count > 0 && vh.FromTriggerFilters.Count > 0)
            {
                IEdge createViewTriggerEdge = GraphChangeTrigger.AddTrigger(edgeFrom,
                vh.FromTriggerQueries,
                vh.FromTriggerFilters,
                "View");

                IVertex createViewTriggerVertex = createViewTriggerEdge.To;

                ExecutionFlowHelper.AddListener_DotNetDelegate(createViewTriggerVertex, 
                    m0.Graph.ExecutionFlow.View.CreateView_FromToListener,
                    "CreateViewFromListener");

                EdgeHelper.AddEdgeVertex(createViewTriggerVertex, edgeFrom, edgeMeta, edgeTo, "ViewEdge");
            }

            if (vh.ToFromTransformFunctions.Count > 0 && vh.ToTriggerFilters.Count > 0)
            {
                IEdge createViewTriggerEdge = GraphChangeTrigger.AddTrigger(edgeTo,
                vh.ToTriggerQueries,
                vh.ToTriggerFilters,
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
