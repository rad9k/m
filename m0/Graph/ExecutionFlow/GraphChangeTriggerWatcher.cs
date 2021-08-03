using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    public class WatcherEntry
    {
        public IVertex baseVertex;
        public IVertex listenerVertex;
        public string scopeQuery;
        public IList<IVertex> vertexInScope;
    }

    public class GraphChangeTriggerWatcher
    {
        static HashSet<IEdge> triggerEdgeList = new HashSet<IEdge>();
        
        static Dictionary<IVertex, WatcherEntry> watcherEntryDict;

        public static void AddGraphChangeTrigger(IEdge triggerEdge)
        {
            triggerEdgeList.Add(triggerEdge);
        }

        static void UpdateWatcherEntryList()
        {
            watcherEntryDict = new Dictionary<IVertex, WatcherEntry>();

            foreach(IEdge e in triggerEdgeList)
            {
                WatcherEntry en = new WatcherEntry();
                en.baseVertex = e.From;
                en.listenerVertex = e.To;

                IVertex scopeQuery = e.To.Get(false, "ScopeQuery:");

                if (scopeQuery != null)
                    en.scopeQuery = scopeQuery.Value.ToString();

                watcherEntryDict.Add(en.listenerVertex, en);
            }
        }

        static void UpdateWatchedEdges()
        {
            foreach(WatcherEntry en in watcherEntryDict.Values)
            {
                if (en.scopeQuery != null)
                    en.vertexInScope = GraphUtil.GetVertexListFromEdgeEnumerable(en.baseVertex.GetAll(false, en.scopeQuery));
                else {
                    en.vertexInScope = new List<IVertex>();
                    en.vertexInScope.Add(en.baseVertex);
                }
            }
        }

        public static Dictionary<IVertex, WatcherEntry> GetWatchedEdges()
        {
            UpdateWatcherEntryList();

            UpdateWatchedEdges();

            return watcherEntryDict;
        }

        public static void RemoveGraphChangeTrigger(IEdge triggerEdge)
        {
            triggerEdgeList.Remove(triggerEdge);
        }
    }
}
