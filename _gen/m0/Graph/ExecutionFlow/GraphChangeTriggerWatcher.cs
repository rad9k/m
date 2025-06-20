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
        static bool triggerListChanged = false;

        static HashSet<IEdge> triggerEdgeList = new HashSet<IEdge>();
        
        static IList<WatcherEntry> watcherEntryList;

        public static void AddGraphChangeTrigger(IEdge triggerEdge)
        {
            triggerEdgeList.Add(triggerEdge);

            triggerListChanged = true;
        }

        public static void RemoveGraphChangeTrigger(IEdge triggerEdge)
        {
           // triggerEdge.To.Dispose(); // this is redundant and sometimes makes troubles

            triggerEdgeList.Remove(triggerEdge);

            triggerListChanged = true;
        }

        public static void RemoveAllGraphChangeTriggers()
        {
            foreach (IEdge e in triggerEdgeList.ToList())
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
        }

        private static void FillVertexInScope()
        {
            foreach(WatcherEntry en in watcherEntryList)
            {
                en.vertexInScope = new List<IVertex>();
                
                if(!en.FilterOutRootVertexEvents)
                    en.vertexInScope.Add(en.sourceVertex); 

                if (en.scopeQuery != null)
                    foreach(string s in en.scopeQuery)
                        foreach(IEdge e in en.sourceVertex.GetAll(false, s))
                            en.vertexInScope.Add(e.To);                
            }
        }        

        public static Dictionary<IVertex, List<WatcherEntry>> GetWatchedVertexDictionary()
        {            
            CreateWatcherEntryList();

            FillVertexInScope();

            Dictionary<IVertex, List<WatcherEntry>> dict = new Dictionary<IVertex, List<WatcherEntry>>();

            foreach(WatcherEntry en in watcherEntryList)
                foreach(IVertex v in en.vertexInScope)
                    if(en.triggerVertex.DisposedState == DisposeStateEnum.Live)
                        GeneralUtil.DictionaryAdd<IVertex, WatcherEntry>(dict, v, en);

            triggerListChanged = false;            

            return dict;
        }        
    }
}
