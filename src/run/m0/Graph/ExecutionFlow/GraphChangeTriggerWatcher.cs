using m0.Foundation;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{ 
    public class WatcherEntry
    {
        public IVertex sourceVertex;
        public IVertex triggerVertex;
        public IList<string> scopeQuery;
        public bool FilterOutRootVertexEvents = false;
        public HashSet<GraphChangeFilterEnum> graphChangeFilter;
        public IList<IVertex> vertexInScope;      
    }

    public class GraphChangeTriggerWatcher
    {
        private sealed class DefinitionEdgeState
        {
            public IEdge Edge;
            public IVertex Meta;
            public IVertex To;
            public string MetaValue;
            public string ToValue;
        }

        private sealed class TriggerDefinitionState
        {
            public IVertex SourceVertex;
            public IVertex TriggerVertex;
            public IList<DefinitionEdgeState> Edges;
        }

        static readonly object synchronizationRoot =
            new object();

        static bool triggerListChanged = true;

        static HashSet<IEdge> triggerEdgeList =
            new HashSet<IEdge>(
                ReferenceEqualityComparer.Instance);
        
        static IList<WatcherEntry> watcherEntryList;
        static Dictionary<IEdge, TriggerDefinitionState>
            triggerDefinitionStates;

        internal static int TriggerCount
        {
            get
            {
                lock (synchronizationRoot)
                    return triggerEdgeList.Count;
            }
        }

        public static void AddGraphChangeTrigger(IEdge triggerEdge)
        {
            lock (synchronizationRoot)
            {
                if (triggerEdgeList.Add(triggerEdge))
                    triggerListChanged = true;
            }
        }

        public static void RemoveGraphChangeTrigger(IEdge triggerEdge)
        {
           // triggerEdge.To.Dispose(); // this is redundant and sometimes makes troubles

            lock (synchronizationRoot)
            {
                if (triggerEdgeList.Remove(triggerEdge))
                    triggerListChanged = true;
            }
        }

        public static void RemoveAllGraphChangeTriggers()
        {
            IEdge[] triggerEdges;

            lock (synchronizationRoot)
                triggerEdges = triggerEdgeList.ToArray();

            foreach (IEdge e in triggerEdges)
                RemoveGraphChangeTrigger(e);
        }

        private static void CreateWatcherEntryList()
        {
            watcherEntryList = new List<WatcherEntry>();

            foreach(IEdge e in triggerEdgeList)
            {
                WatcherEntry en = new WatcherEntry();
                en.sourceVertex = e.From;
                en.triggerVertex = e.To;
                

                IVertex scopeQueryEdges = e.To.GetAll(false, "ScopeQuery:");

                if (scopeQueryEdges.OutEdges.Count > 0)
                {
                    en.scopeQuery = new List<string>();

                    foreach (IEdge ee in scopeQueryEdges)
                        en.scopeQuery.Add(ee.To.Value.ToString());
                }

                IVertex changeTypeFilterEdges = e.To.GetAll(false, "ChangeTypeFilter:");

                if (changeTypeFilterEdges.OutEdges.Count > 0)
                {
                    en.graphChangeFilter = new HashSet<GraphChangeFilterEnum>();

                    foreach (IEdge ee in changeTypeFilterEdges)
                    {
                        string value = ee.To.ToString();

                        switch (value)
                        {
                            case "FilterOutRootVertexEvents":
                                en.FilterOutRootVertexEvents = true;                                
                                break;

                            case "ValueChange":
                                en.graphChangeFilter.Add(GraphChangeFilterEnum.ValueChange);
                                break;

                            case "InputEdgeAdded":
                                en.graphChangeFilter.Add(GraphChangeFilterEnum.InputEdgeAdded);
                                break;

                            case "InputEdgeRemoved":
                                en.graphChangeFilter.Add(GraphChangeFilterEnum.InputEdgeRemoved);
                                break;

                            case "OutputEdgeAdded":
                                en.graphChangeFilter.Add(GraphChangeFilterEnum.OutputEdgeAdded);
                                break;

                            case "OutputEdgeRemoved":
                                en.graphChangeFilter.Add(GraphChangeFilterEnum.OutputEdgeRemoved);
                                break;                            

                            case "MetaEdgeAdded":
                                en.graphChangeFilter.Add(GraphChangeFilterEnum.MetaEdgeAdded);
                                break;

                            case "MetaEdgeRemoved":
                                en.graphChangeFilter.Add(GraphChangeFilterEnum.MetaEdgeRemoved);
                                break;

                            case "OutputEdgeDisposed":
                                en.graphChangeFilter.Add(GraphChangeFilterEnum.OutputEdgeDisposed);
                                break;
                        }
                    }                    
                }
                
                watcherEntryList.Add(en);
            }

            CaptureTriggerDefinitionStates();
            triggerListChanged = false;
        }

        private static bool TriggerDefinitionsAreCurrent()
        {
            if (triggerListChanged ||
                watcherEntryList == null ||
                triggerDefinitionStates == null ||
                triggerDefinitionStates.Count !=
                    triggerEdgeList.Count)
            {
                return false;
            }

            foreach (IEdge triggerEdge in triggerEdgeList)
            {
                if (!triggerDefinitionStates.TryGetValue(
                    triggerEdge,
                    out TriggerDefinitionState state))
                {
                    return false;
                }

                if (!ReferenceEquals(
                        state.SourceVertex,
                        triggerEdge.From) ||
                    !ReferenceEquals(
                        state.TriggerVertex,
                        triggerEdge.To))
                {
                    return false;
                }

                IList<IEdge> currentEdges =
                    triggerEdge.To.OutEdges;
                if (currentEdges.Count != state.Edges.Count)
                    return false;

                for (var index = 0;
                     index < currentEdges.Count;
                     index++)
                {
                    IEdge currentEdge = currentEdges[index];
                    DefinitionEdgeState edgeState =
                        state.Edges[index];

                    if (!ReferenceEquals(
                            edgeState.Edge,
                            currentEdge) ||
                        !ReferenceEquals(
                            edgeState.Meta,
                            currentEdge.Meta) ||
                        !ReferenceEquals(
                            edgeState.To,
                            currentEdge.To) ||
                        edgeState.MetaValue !=
                            GetValueString(
                                currentEdge.Meta) ||
                        edgeState.ToValue !=
                            GetValueString(
                                currentEdge.To))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void CaptureTriggerDefinitionStates()
        {
            triggerDefinitionStates =
                new Dictionary<IEdge, TriggerDefinitionState>(
                    ReferenceEqualityComparer.Instance);

            foreach (IEdge triggerEdge in triggerEdgeList)
            {
                IList<DefinitionEdgeState> edgeStates =
                    new List<DefinitionEdgeState>();

                foreach (IEdge edge in triggerEdge.To.OutEdges)
                    edgeStates.Add(
                        new DefinitionEdgeState
                        {
                            Edge = edge,
                            Meta = edge.Meta,
                            To = edge.To,
                            MetaValue =
                                GetValueString(edge.Meta),
                            ToValue =
                                GetValueString(edge.To)
                        });

                triggerDefinitionStates.Add(
                    triggerEdge,
                    new TriggerDefinitionState
                    {
                        SourceVertex = triggerEdge.From,
                        TriggerVertex = triggerEdge.To,
                        Edges = edgeStates
                    });
            }
        }

        private static string GetValueString(
            IVertex vertex)
        {
            return vertex?.Value?.ToString();
        }

        private static void AddWatchedVertex(
            Dictionary<IVertex, List<WatcherEntry>>
                watchedVertexDictionary,
            IVertex vertex,
            WatcherEntry watcherEntry)
        {
            if (!watchedVertexDictionary.TryGetValue(
                vertex,
                out List<WatcherEntry> entries))
            {
                entries = new List<WatcherEntry>();
                watchedVertexDictionary.Add(vertex, entries);
            }

            if (!entries.Contains(watcherEntry))
                entries.Add(watcherEntry);
        }

        public static Dictionary<IVertex, List<WatcherEntry>> GetWatchedVertexDictionary()
        {
            lock (synchronizationRoot)
            {
                if (!TriggerDefinitionsAreCurrent())
                    CreateWatcherEntryList();

                Dictionary<IVertex, List<WatcherEntry>> dict =
                    new Dictionary<IVertex, List<WatcherEntry>>(
                        ReferenceEqualityComparer.Instance);

                foreach (WatcherEntry en in watcherEntryList)
                {
                    if (en.triggerVertex.DisposedState !=
                        DisposeStateEnum.Live)
                    {
                        continue;
                    }

                    if (!en.FilterOutRootVertexEvents)
                        AddWatchedVertex(
                            dict,
                            en.sourceVertex,
                            en);

                    if (en.scopeQuery == null)
                        continue;

                    foreach (string scopeQuery in en.scopeQuery)
                        foreach (IEdge edge in
                            en.sourceVertex.GetAll(
                                false,
                                scopeQuery))
                        {
                            AddWatchedVertex(
                                dict,
                                edge.To,
                                en);
                        }
                }

                return dict;
            }
        }        
    }
}
