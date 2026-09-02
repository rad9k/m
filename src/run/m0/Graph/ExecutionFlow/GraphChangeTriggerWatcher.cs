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
        static Dictionary<IVertex, List<WatcherEntry>>
            watchersBySourceVertex;
        static Dictionary<IVertex, List<WatcherEntry>>
            cachedWatchedVertexDictionary;

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

            RebuildWatchersBySourceVertex();
            cachedWatchedVertexDictionary = null;

            CaptureTriggerDefinitionStates();
            triggerListChanged = false;
        }

        private static bool IsTriggerDefinitionEdge(IEdge edge)
        {
            if (edge == null || edge.Meta == null)
                return false;

            return GeneralUtil.CompareStrings(
                    edge.Meta.Value,
                    "ScopeQuery") ||
                GeneralUtil.CompareStrings(
                    edge.Meta.Value,
                    "ChangeTypeFilter");
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

                int definitionEdgeIndex = 0;
                IList<IEdge> currentEdges =
                    triggerEdge.To.OutEdgesRaw;

                for (int index = 0;
                     index < currentEdges.Count;
                     index++)
                {
                    IEdge currentEdge = currentEdges[index];
                    if (!IsTriggerDefinitionEdge(currentEdge))
                        continue;

                    if (definitionEdgeIndex >= state.Edges.Count)
                        return false;

                    DefinitionEdgeState edgeState =
                        state.Edges[definitionEdgeIndex];
                    definitionEdgeIndex++;

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

                if (definitionEdgeIndex != state.Edges.Count)
                    return false;
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

                foreach (IEdge edge in triggerEdge.To.OutEdgesRaw)
                {
                    if (!IsTriggerDefinitionEdge(edge))
                        continue;

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
                }

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

        private static void RebuildWatchersBySourceVertex()
        {
            watchersBySourceVertex =
                new Dictionary<IVertex, List<WatcherEntry>>(
                    ReferenceEqualityComparer.Instance);

            foreach (WatcherEntry en in watcherEntryList)
            {
                if (en.sourceVertex == null)
                    continue;

                if (!watchersBySourceVertex.TryGetValue(
                    en.sourceVertex,
                    out List<WatcherEntry> entries))
                {
                    entries = new List<WatcherEntry>();
                    watchersBySourceVertex.Add(
                        en.sourceVertex,
                        entries);
                }

                entries.Add(en);
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
            if (vertex == null)
                return;

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

        private static void AddWatcherToWatchedDictionary(
            Dictionary<IVertex, List<WatcherEntry>>
                watchedVertexDictionary,
            WatcherEntry en)
        {
            if (en.triggerVertex == null ||
                en.triggerVertex.DisposedState !=
                    DisposeStateEnum.Live)
            {
                return;
            }

            if (en.sourceVertex == null ||
                en.sourceVertex.DisposedState !=
                    DisposeStateEnum.Live)
            {
                return;
            }

            if (!en.FilterOutRootVertexEvents)
                AddWatchedVertex(
                    watchedVertexDictionary,
                    en.sourceVertex,
                    en);

            if (en.scopeQuery == null)
                return;

            foreach (string scopeQuery in en.scopeQuery)
                foreach (IEdge edge in
                    en.sourceVertex.GetAll(
                        false,
                        scopeQuery))
                {
                    AddWatchedVertex(
                        watchedVertexDictionary,
                        edge.To,
                        en);
                }
        }

        private static Dictionary<IVertex, List<WatcherEntry>>
            BuildWatchedVertexDictionary()
        {
            Dictionary<IVertex, List<WatcherEntry>> dict =
                new Dictionary<IVertex, List<WatcherEntry>>(
                    ReferenceEqualityComparer.Instance);

            foreach (WatcherEntry en in watcherEntryList)
                AddWatcherToWatchedDictionary(dict, en);

            return dict;
        }

        const int IncomingEdgeFanoutLimit = 32;

        private static void AddParentWatcherSourcesForNewVertex(
            IVertex vertex,
            HashSet<IVertex> sourcesToRefresh)
        {
            if (vertex == null ||
                vertex.DisposedState != DisposeStateEnum.Live)
            {
                return;
            }

            IList<IEdge> inEdges = vertex.InEdgesRaw;
            if (inEdges == null ||
                inEdges.Count == 0 ||
                inEdges.Count > IncomingEdgeFanoutLimit)
            {
                return;
            }

            foreach (IEdge edge in inEdges)
            {
                if (edge.From == null)
                    continue;

                if (watchersBySourceVertex.ContainsKey(
                    edge.From))
                {
                    sourcesToRefresh.Add(edge.From);
                }
            }
        }

        private static int PruneDisposedWatchedVertices()
        {
            List<IVertex> disposedVertices = null;

            foreach (IVertex vertex in
                cachedWatchedVertexDictionary.Keys)
            {
                if (vertex.DisposedState ==
                    DisposeStateEnum.Live)
                {
                    continue;
                }

                if (disposedVertices == null)
                    disposedVertices = new List<IVertex>();

                disposedVertices.Add(vertex);
            }

            if (disposedVertices == null)
                return 0;

            foreach (IVertex vertex in disposedVertices)
                cachedWatchedVertexDictionary.Remove(vertex);

            return disposedVertices.Count;
        }

        private static int RefreshWatchedVertexDictionary(
            ICollection<IVertex> changedVertices)
        {
            HashSet<IVertex> sourcesToRefresh =
                new HashSet<IVertex>(
                    ReferenceEqualityComparer.Instance);

            foreach (IVertex changedVertex in changedVertices)
            {
                if (changedVertex == null)
                    continue;

                if (watchersBySourceVertex.ContainsKey(
                    changedVertex))
                {
                    sourcesToRefresh.Add(changedVertex);
                }

                if (!cachedWatchedVertexDictionary.ContainsKey(
                    changedVertex))
                {
                    AddParentWatcherSourcesForNewVertex(
                        changedVertex,
                        sourcesToRefresh);
                }
            }

            int watcherCount = watcherEntryList.Count;
            if (watcherCount > 0 &&
                sourcesToRefresh.Count * 4 > watcherCount)
            {
                return -1;
            }

            foreach (IVertex sourceVertex in sourcesToRefresh)
            {
                if (!watchersBySourceVertex.TryGetValue(
                    sourceVertex,
                    out List<WatcherEntry> watchers))
                {
                    continue;
                }

                foreach (WatcherEntry en in watchers)
                    AddWatcherToWatchedDictionary(
                        cachedWatchedVertexDictionary,
                        en);
            }

            return sourcesToRefresh.Count;
        }

        public static Dictionary<IVertex, List<WatcherEntry>>
            GetWatchedVertexDictionary()
        {
            return GetWatchedVertexDictionary(null);
        }

        public static Dictionary<IVertex, List<WatcherEntry>>
            GetWatchedVertexDictionary(
                ICollection<IVertex> changedVertices)
        {
            lock (synchronizationRoot)
            {
                if (!TriggerDefinitionsAreCurrent())
                    CreateWatcherEntryList();

                if (cachedWatchedVertexDictionary == null ||
                    watchersBySourceVertex == null)
                {
                    cachedWatchedVertexDictionary =
                        BuildWatchedVertexDictionary();
                }
                else if (changedVertices == null)
                {
                    cachedWatchedVertexDictionary =
                        BuildWatchedVertexDictionary();
                }
                else
                {
                    PruneDisposedWatchedVertices();
                    int sourcesRefreshed =
                        RefreshWatchedVertexDictionary(
                            changedVertices);
                    if (sourcesRefreshed < 0)
                    {
                        cachedWatchedVertexDictionary =
                            BuildWatchedVertexDictionary();
                    }
                }

                return cachedWatchedVertexDictionary;
            }
        }        
    }
}
