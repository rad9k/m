using System.Threading;

namespace m0.Graph
{
    public enum OutEdgesRebuildKind
    {
        LogicalEdges,
        DirectMeta,
        QueryMeta,
        Value,
        QueryMetaAndValue
    }

    public readonly record struct GraphPerformanceSnapshot(
        long LogicalOutEdgesRebuilds,
        long DirectMetaIndexRebuilds,
        long QueryMetaIndexRebuilds,
        long ValueIndexRebuilds,
        long QueryMetaAndValueIndexRebuilds,
        long RebuildScannedEdges,
        long InheritChildTraversalCalls,
        long TraversedInheritChildren,
        long InvalidatedInheritChildren,
        long CreatedStacks,
        long CreatedTempStoreStacks,
        long QueryParseCacheHits,
        long QueryParseCacheMisses,
        long MaximumQueryParseCacheSize,
        long RollbackJournalAtoms,
        long CoalescedGraphChangeAtoms);

    public static class GraphPerformanceCounters
    {
        private static long logicalOutEdgesRebuilds;
        private static long directMetaIndexRebuilds;
        private static long queryMetaIndexRebuilds;
        private static long valueIndexRebuilds;
        private static long queryMetaAndValueIndexRebuilds;
        private static long rebuildScannedEdges;
        private static long inheritChildTraversalCalls;
        private static long traversedInheritChildren;
        private static long invalidatedInheritChildren;
        private static long createdStacks;
        private static long createdTempStoreStacks;
        private static long queryParseCacheHits;
        private static long queryParseCacheMisses;
        private static long maximumQueryParseCacheSize;
        private static long rollbackJournalAtoms;
        private static long coalescedGraphChangeAtoms;

        public static bool Enabled { get; set; }

        public static void Reset()
        {
            Interlocked.Exchange(ref logicalOutEdgesRebuilds, 0);
            Interlocked.Exchange(ref directMetaIndexRebuilds, 0);
            Interlocked.Exchange(ref queryMetaIndexRebuilds, 0);
            Interlocked.Exchange(ref valueIndexRebuilds, 0);
            Interlocked.Exchange(ref queryMetaAndValueIndexRebuilds, 0);
            Interlocked.Exchange(ref rebuildScannedEdges, 0);
            Interlocked.Exchange(ref inheritChildTraversalCalls, 0);
            Interlocked.Exchange(ref traversedInheritChildren, 0);
            Interlocked.Exchange(ref invalidatedInheritChildren, 0);
            Interlocked.Exchange(ref createdStacks, 0);
            Interlocked.Exchange(ref createdTempStoreStacks, 0);
            Interlocked.Exchange(ref queryParseCacheHits, 0);
            Interlocked.Exchange(ref queryParseCacheMisses, 0);
            Interlocked.Exchange(ref maximumQueryParseCacheSize, 0);
            Interlocked.Exchange(ref rollbackJournalAtoms, 0);
            Interlocked.Exchange(ref coalescedGraphChangeAtoms, 0);
        }

        public static GraphPerformanceSnapshot GetSnapshot()
        {
            return new GraphPerformanceSnapshot(
                Volatile.Read(ref logicalOutEdgesRebuilds),
                Volatile.Read(ref directMetaIndexRebuilds),
                Volatile.Read(ref queryMetaIndexRebuilds),
                Volatile.Read(ref valueIndexRebuilds),
                Volatile.Read(ref queryMetaAndValueIndexRebuilds),
                Volatile.Read(ref rebuildScannedEdges),
                Volatile.Read(ref inheritChildTraversalCalls),
                Volatile.Read(ref traversedInheritChildren),
                Volatile.Read(ref invalidatedInheritChildren),
                Volatile.Read(ref createdStacks),
                Volatile.Read(ref createdTempStoreStacks),
                Volatile.Read(ref queryParseCacheHits),
                Volatile.Read(ref queryParseCacheMisses),
                Volatile.Read(ref maximumQueryParseCacheSize),
                Volatile.Read(ref rollbackJournalAtoms),
                Volatile.Read(ref coalescedGraphChangeAtoms));
        }

        internal static void RecordOutEdgesRebuild(OutEdgesRebuildKind kind, int scannedEdges)
        {
            if (!Enabled)
                return;

            switch (kind)
            {
                case OutEdgesRebuildKind.LogicalEdges:
                    Interlocked.Increment(ref logicalOutEdgesRebuilds);
                    break;
                case OutEdgesRebuildKind.DirectMeta:
                    Interlocked.Increment(ref directMetaIndexRebuilds);
                    break;
                case OutEdgesRebuildKind.QueryMeta:
                    Interlocked.Increment(ref queryMetaIndexRebuilds);
                    break;
                case OutEdgesRebuildKind.Value:
                    Interlocked.Increment(ref valueIndexRebuilds);
                    break;
                case OutEdgesRebuildKind.QueryMetaAndValue:
                    Interlocked.Increment(ref queryMetaAndValueIndexRebuilds);
                    break;
            }

            Interlocked.Add(ref rebuildScannedEdges, scannedEdges);
        }

        internal static void RecordInheritChildTraversal(int traversedChildCount)
        {
            if (!Enabled)
                return;

            Interlocked.Increment(ref inheritChildTraversalCalls);
            Interlocked.Add(ref traversedInheritChildren, traversedChildCount);
        }

        internal static void RecordInvalidatedInheritChildren(int invalidatedChildCount)
        {
            if (Enabled)
                Interlocked.Add(ref invalidatedInheritChildren, invalidatedChildCount);
        }

        internal static void RecordStackCreated(bool isTempStoreStack)
        {
            if (!Enabled)
                return;

            Interlocked.Increment(ref createdStacks);

            if (isTempStoreStack)
                Interlocked.Increment(ref createdTempStoreStacks);
        }

        internal static void RecordQueryParseCacheLookup(bool hit, int cacheSize)
        {
            if (!Enabled)
                return;

            if (hit)
                Interlocked.Increment(ref queryParseCacheHits);
            else
                Interlocked.Increment(ref queryParseCacheMisses);

            UpdateMaximum(ref maximumQueryParseCacheSize, cacheSize);
        }

        internal static void RecordRollbackJournalAtom()
        {
            if (Enabled)
                Interlocked.Increment(ref rollbackJournalAtoms);
        }

        internal static void RecordCoalescedGraphChangeAtom()
        {
            if (Enabled)
                Interlocked.Increment(
                    ref coalescedGraphChangeAtoms);
        }

        private static void UpdateMaximum(ref long target, long value)
        {
            var current = Volatile.Read(ref target);

            while (value > current)
            {
                var observed = Interlocked.CompareExchange(ref target, value, current);

                if (observed == current)
                    return;

                current = observed;
            }
        }
    }
}
