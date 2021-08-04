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
        public IVertex baseVertex;
        public IVertex listenerVertex;
        public string scopeQuery;
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

        static void CreateWatcherEntryList()
        {
            watcherEntryList = new List<WatcherEntry>();

            foreach(IEdge e in triggerEdgeList)
            {
                WatcherEntry en = new WatcherEntry();
                en.baseVertex = e.From;
                en.listenerVertex = e.To;

                IVertex scopeQuery = e.To.Get(false, "ScopeQuery:");

                if (scopeQuery != null)
                    en.scopeQuery = scopeQuery.Value.ToString();

                watcherEntryList.Add(en);
            }
        }

        static void FillVertexInScope()
        {
            foreach(WatcherEntry en in watcherEntryList)
            {
                if (en.scopeQuery != null)
                    en.vertexInScope = GraphUtil.GetVertexListFromEdgeEnumerable(en.baseVertex.GetAll(false, en.scopeQuery));
                else {
                    en.vertexInScope = new List<IVertex>();
                    en.vertexInScope.Add(en.baseVertex);
                }
            }
        }

        public static Dictionary<IVertex, List<WatcherEntry>> GetWatcherDictionary()
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
