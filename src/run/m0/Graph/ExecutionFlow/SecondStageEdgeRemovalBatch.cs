using m0.Foundation;
using m0.Util;
using System;
using System.Collections.Generic;

namespace m0.Graph.ExecutionFlow
{
    // Collects every edge dropped by one second-stage dispose wave and removes
    // each affected edge list once. Surviving edges keep their order.
    public sealed class SecondStageEdgeRemovalBatch : IDisposable
    {
        [ThreadStatic]
        static Stack<SecondStageEdgeRemovalBatch> activeBatches;

        readonly HashSet<IEdge> edges = new HashSet<IEdge>();
        bool flushed;

        public static bool IsActive
        {
            get { return activeBatches != null && activeBatches.Count > 0; }
        }

        public static SecondStageEdgeRemovalBatch Begin()
        {
            if (activeBatches == null)
                activeBatches = new Stack<SecondStageEdgeRemovalBatch>();

            SecondStageEdgeRemovalBatch batch = new SecondStageEdgeRemovalBatch();
            activeBatches.Push(batch);
            return batch;
        }

        public static void Add(IEdge edge)
        {
            if (!IsActive || edge == null)
                return;

            activeBatches.Peek().edges.Add(edge);
        }

        public void Dispose()
        {
            if (activeBatches != null &&
                activeBatches.Count > 0 &&
                ReferenceEquals(activeBatches.Peek(), this))
            {
                activeBatches.Pop();
            }

            Flush();
        }

        void Flush()
        {
            if (flushed || edges.Count == 0)
            {
                flushed = true;
                return;
            }

            flushed = true;

            Dictionary<IList<IEdge>, HashSet<IEdge>> edgesByList =
                new Dictionary<IList<IEdge>, HashSet<IEdge>>();

            foreach (IEdge edge in edges)
                AddEdgeToItsLists(edgesByList, edge);

            List<PendingEdgeListRemoval> pendingRemovals =
                new List<PendingEdgeListRemoval>();

            foreach (KeyValuePair<IList<IEdge>, HashSet<IEdge>> listEdges in edgesByList)
            {
                if (!(listEdges.Key is ExtandableList<IEdge> extendableList))
                    continue;

                List<IEdge> removed =
                    extendableList.RemoveReferencedItemsWithoutCallbacks(listEdges.Value);

                if (removed.Count > 0)
                    pendingRemovals.Add(new PendingEdgeListRemoval(extendableList, removed));
            }

            foreach (IEdge edge in edges)
                edge.EdgeRemovalExecuting = true;

            try
            {
                for (int pendingIndex = 0; pendingIndex < pendingRemovals.Count; pendingIndex++)
                {
                    PendingEdgeListRemoval pending = pendingRemovals[pendingIndex];

                    for (int removedIndex = 0; removedIndex < pending.RemovedEdges.Count; removedIndex++)
                        pending.EdgeList.OnRemove(pending.RemovedEdges[removedIndex]);
                }
            }
            finally
            {
                foreach (IEdge edge in edges)
                    edge.EdgeRemovalExecuting = false;
            }
        }

        static void AddEdgeToItsLists(
            Dictionary<IList<IEdge>, HashSet<IEdge>> edgesByList,
            IEdge edge)
        {
            if (edge.From != null)
                AddToList(edgesByList, edge.From.OutEdgesRaw, edge);

            if (edge.To != null)
                AddToList(edgesByList, edge.To.InEdgesRaw, edge);

            if (edge.Meta != null)
                AddToList(edgesByList, edge.Meta.MetaInEdgesRaw, edge);
        }

        static void AddToList(
            Dictionary<IList<IEdge>, HashSet<IEdge>> edgesByList,
            IList<IEdge> edgeList,
            IEdge edge)
        {
            if (edgeList == null)
                return;

            if (!edgesByList.TryGetValue(edgeList, out HashSet<IEdge> edgesInList))
            {
                edgesInList = new HashSet<IEdge>();
                edgesByList.Add(edgeList, edgesInList);
            }

            edgesInList.Add(edge);
        }

        sealed class PendingEdgeListRemoval
        {
            internal PendingEdgeListRemoval(
                ExtandableList<IEdge> edgeList,
                List<IEdge> removedEdges)
            {
                EdgeList = edgeList;
                RemovedEdges = removedEdges;
            }

            internal ExtandableList<IEdge> EdgeList { get; }

            internal List<IEdge> RemovedEdges { get; }
        }
    }
}
