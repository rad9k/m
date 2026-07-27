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
        private static string TruncateForLog(string value, int maxLength)
        {
            if (value == null)
                return "<null>";

            if (value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength) + "...(truncated, totalLen=" + value.Length + ")";
        }

        private static string SafeVertexId(IVertex vertex)
        {
            if (vertex == null)
                return "<null>";

            try
            {
                return GraphUtil.GetVertexIdString(vertex);
            }
            catch (Exception ex)
            {
                return "<id-error:" + ex.Message + ">";
            }
        }

        private static string SafeVertexValue(IVertex vertex)
        {
            if (vertex == null)
                return "<null>";

            try
            {
                return TruncateForLog(GraphUtil.GetStringValue(vertex), 120);
            }
            catch (Exception ex)
            {
                return "<value-error:" + ex.Message + ">";
            }
        }

        public static INoInEdgeInOutVertexVertex CreateView_MetaEdgeAdded(IExecution exe)
        {
            IList<IEdge> events = GraphUtil.GetQueryOut(exe.Stack, "event", null);
            MinusZero.Instance.Log(1, "View.CreateView_MetaEdgeAdded",
                "BEGIN eventCount=" + events.Count);

            foreach (IEdge e in events)
            {
                IVertex triggerVertex = GraphUtil.GetQueryOutFirst(e.To, "Trigger", null);
                string triggerValue = GraphUtil.GetStringValueOrNull(triggerVertex);

                MinusZero.Instance.Log(1, "View.CreateView_MetaEdgeAdded",
                    "event trigger=" + (triggerValue ?? "<null>")
                    + " triggerId=" + SafeVertexId(triggerVertex));

                if (triggerValue == "CreateView")
                    ProcessCreateViewEvent(exe, e);
            }

            MinusZero.Instance.Log(1, "View.CreateView_MetaEdgeAdded", "END");
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
            MinusZero.Instance.Log(1, "View.CreateView_FromToListener", "BEGIN");

            IList<IEdge> viewEvents;
            IVertex viewEdge;

            GetViewEventsAndEdge(exe, out viewEvents, out viewEdge);

            IVertex metaVertex = GraphUtil.GetQueryOutFirst(viewEdge, "Meta", null);

            IVertex createView = GraphUtil.GetQueryOutFirst(metaVertex, "CreateView", null);

            MinusZero.Instance.Log(1, "View.CreateView_FromToListener",
                "metaValue=" + SafeVertexValue(metaVertex)
                + " createViewNull=" + (createView == null)
                + " viewEventsCount=" + viewEvents.Count);

            ViewHolder vh = new ViewHolder(createView);

            vh.ExecuteFromToTransformFunction(exe,
                viewEvents,
                GraphUtil.GetQueryOutFirst(viewEdge, "From", null),
                metaVertex,
                GraphUtil.GetQueryOutFirst(viewEdge, "To", null));

            MinusZero.Instance.Log(1, "View.CreateView_FromToListener", "END");
            return exe.Stack;
        }

        public static INoInEdgeInOutVertexVertex CreateView_ToFromListener(IExecution exe)
        {
            MinusZero.Instance.Log(1, "View.CreateView_ToFromListener", "BEGIN");

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

            MinusZero.Instance.Log(1, "View.CreateView_ToFromListener", "END");
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
            IVertex createViewChild = edgeMeta == null
                ? null
                : GraphUtil.GetQueryOutFirst(edgeMeta, "CreateView", null);

            MinusZero.Instance.Log(1, "View.ProcessCreateViewEvent",
                "BEGIN edgeMetaValue=" + SafeVertexValue(edgeMeta)
                + " edgeMetaId=" + SafeVertexId(edgeMeta)
                + " edgeFromId=" + SafeVertexId(edgeFrom)
                + " edgeToId=" + SafeVertexId(edgeTo)
                + " edgeToValue=" + SafeVertexValue(edgeTo)
                + " viewNull=" + (view == null)
                + " viewId=" + SafeVertexId(view)
                + " createViewChildNull=" + (createViewChild == null)
                + " createViewChildId=" + SafeVertexId(createViewChild)
                + " triggerValue=" + SafeVertexValue(trigger));

            if (edgeMeta != null)
            {
                System.Text.StringBuilder metaChildren = new System.Text.StringBuilder();
                foreach (IEdge metaEdge in edgeMeta.OutEdges)
                {
                    if (metaChildren.Length > 0)
                        metaChildren.Append(" | ");
                    metaChildren.Append(SafeVertexValue(metaEdge.Meta));
                    metaChildren.Append("=>");
                    metaChildren.Append(SafeVertexValue(metaEdge.To));
                }
                MinusZero.Instance.Log(1, "View.ProcessCreateViewEvent",
                    "edgeMetaOutEdges=" + TruncateForLog(metaChildren.ToString(), 800));
            }

            if (view == null)
            {
                MinusZero.Instance.Log(1, "View.ProcessCreateViewEvent",
                    "View child is null on edgeMeta - CreateView transform will not run. createViewChildNull="
                    + (createViewChild == null)
                    + " END early");
                return;
            }

            ViewHolder vh = new ViewHolder(view);

            MinusZero.Instance.Log(1, "View.ProcessCreateViewEvent",
                "ViewHolder FromToTransformFunctions=" + vh.FromToTransformFunctions.Count
                + " ToFromTransformFunctions=" + vh.ToFromTransformFunctions.Count
                + " FromTriggerFilters=" + vh.FromTriggerFilters.Count
                + " ToTriggerFilters=" + vh.ToTriggerFilters.Count);

            if (vh.ToFromTransformFunctions.Count > 0 && vh.FromToTransformFunctions.Count > 0)
            {
                MinusZero.Instance.Log(1, "View.ProcessCreateViewEvent",
                    "calling ExecuteFromToTransformFunction (both directions present)");
                vh.ExecuteFromToTransformFunction(exe, null, edgeFrom, edgeMeta, edgeTo);
            }
            else {
                if (vh.FromToTransformFunctions.Count > 0)
                {
                    MinusZero.Instance.Log(1, "View.ProcessCreateViewEvent",
                        "calling ExecuteFromToTransformFunction");
                    vh.ExecuteFromToTransformFunction(exe, new List<IEdge>(), edgeFrom, edgeMeta, edgeTo);
                }
                else
                {
                    MinusZero.Instance.Log(1, "View.ProcessCreateViewEvent",
                        "SKIP ExecuteFromToTransformFunction - no FromToTransformFunctions");
                }

                if (vh.ToFromTransformFunctions.Count > 0)
                {
                    MinusZero.Instance.Log(1, "View.ProcessCreateViewEvent",
                        "calling ExecuteToFromTransformFunction");
                    vh.ExecuteToFromTransformFunction(exe, new List<IEdge>(), edgeFrom, edgeMeta, edgeTo);
                }
            }

            MinusZero.Instance.Log(1, "View.ProcessCreateViewEvent",
                "AFTER transform edgeToValue=" + SafeVertexValue(edgeTo)
                + " edgeToValueLength=" + (edgeTo == null || edgeTo.Value == null
                    ? -1
                    : edgeTo.Value.ToString().Length));

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

            MinusZero.Instance.Log(1, "View.ProcessCreateViewEvent", "END");
        }
    }
}
