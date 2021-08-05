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
        public IList<IVertex> vertexInScope;
    }

    public class GraphChangeTriggerWatcher
    {
        static HashSet<IEdge> triggerEdgeList = new HashSet<IEdge>();
        
        static IList<WatcherEntry> watcherEntryList;

        public static void AddGraphChangeTrigger(IEdge triggerEdge)
        {
            triggerEdgeList.Add(triggerEdge);
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
                    foreach(IEdge ee in scopeQueryEdges)
                        en.scopeQuery.Add(ee.To.Value.ToString());

                watcherEntryList.Add(en);
            }
        }

        private static void FillVertexInScope()
        {
            foreach(WatcherEntry en in watcherEntryList)
            {
                if (en.scopeQuery != null)
                    foreach(string s in en.scopeQuery)
                        en.vertexInScope = GraphUtil.GetVertexListFromEdgeEnumerable(en.sourceVertex.GetAll(false, s));
                else {
                    en.vertexInScope = new List<IVertex>();
                    en.vertexInScope.Add(en.sourceVertex);
                }
            }
        }

        public static Dictionary<IVertex, List<WatcherEntry>> GetWatchedVertexDictionary()
        {
            CreateWatcherEntryList();

            FillVertexInScope();

            Dictionary<IVertex, List<WatcherEntry>> dict = new Dictionary<IVertex, List<WatcherEntry>>();

            foreach(WatcherEntry en in watcherEntryList)
                foreach(IVertex v in en.vertexInScope)
                    GeneralUtil.DictionaryAdd<IVertex, WatcherEntry>(dict, v, en);

            return dict;
        }

        public static void RemoveGraphChangeTrigger(IEdge triggerEdge)
        {
            triggerEdgeList.Remove(triggerEdge);
        }
    }
}
