using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    class WatcherEntry
    {
        public IVertex baseVertex;
        public IVertex listenerVertex;
        public string scopeQuery;
    }

    public class GraphChangeTriggerWatcher
    {
        static Dictionary<IVertex, WatcherEntry> triggers = new Dictionary<IVertex, WatcherEntry>();

        public static void AddGraphChangeTrigger(IEdge triggerEdge)
        {
            WatcherEntry en = new WatcherEntry();
            en.baseVertex = triggerEdge.From;
            en.listenerVertex = triggerEdge.To;

            IVertex scopeQuery = triggerEdge.To.Get(false, "ScopeQuery");

            if (scopeQuery != null)
                en.scopeQuery = scopeQuery.Value.ToString();

            triggers.Add(en.listenerVertex, en);
        }

        public static void RemoveGraphChangeTrigger(IEdge triggerEdge)
        {
            triggers.Remove(triggerEdge.To);
        }
    }
}
