using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Graph.Internal;
using m0.Util;
using m0.ZeroCode;

namespace m0.Graph
{
    public class NoInEdgeInOutVertexVertex: EasyVertex, INoInEdgeInOutVertexVertex
    {
        private const int ParentCycleTrackingThreshold =
            1024;

        private static readonly object NoParentStackFrame =
            new object();

        [NonSerialized]
        private object cachedParentStackFrame;

        [NonSerialized]
        private bool isInTemporaryPool;

        [NonSerialized]
        private Dictionary<string,
            DistinctFromMetaCacheEntry>
            distinctFromMetaQueryCache;

        private readonly bool canUseTemporaryPool;

        private sealed class DistinctFromMetaCacheEntry
        {
            private IEdge singleRepresentative;
            private List<IEdge> multipleRepresentatives;

            internal long DependencyEpoch;
            internal long MatchCount;

            internal int RepresentativeCount =>
                multipleRepresentatives?.Count ??
                (singleRepresentative == null ? 0 : 1);

            internal void AddMatch(IEdge edge)
            {
                MatchCount++;

                if (singleRepresentative == null &&
                    multipleRepresentatives == null)
                {
                    singleRepresentative = edge;
                    return;
                }

                if (multipleRepresentatives == null)
                {
                    if (HasSameFromMeta(
                        singleRepresentative,
                        edge))
                        return;

                    multipleRepresentatives =
                        new List<IEdge>
                        {
                            singleRepresentative,
                            edge
                        };
                    singleRepresentative = null;
                    return;
                }

                foreach (IEdge representative in
                    multipleRepresentatives)
                    if (HasSameFromMeta(
                        representative,
                        edge))
                        return;

                multipleRepresentatives.Add(edge);
            }

            internal void GetResult(
                out IEdge result,
                out IList<IEdge> results)
            {
                result = singleRepresentative;
                results = multipleRepresentatives;
            }

            private static bool HasSameFromMeta(
                IEdge left,
                IEdge right)
            {
                return ReferenceEquals(
                        left?.From,
                        right?.From) &&
                    ReferenceEquals(
                        left?.Meta,
                        right?.Meta);
            }
        }

        public NoInEdgeInOutVertexVertex(IStore store)
            : this(
                store,
                VertexIdentifierRegistrationMode.Registered)
        {
        }

        internal NoInEdgeInOutVertexVertex(
            IStore store,
            VertexIdentifierRegistrationMode registrationMode)
            : base(
                store,
                registrationMode,
                useSpecializedStackStorage: true)
        {
            AllowInheritance = false;
            CanEmitGraphChangeEvents = false;
            canUseTemporaryPool =
                registrationMode ==
                    VertexIdentifierRegistrationMode.Ephemeral &&
                ReferenceEquals(
                    store,
                    MinusZero.Instance.TempStore);
        }

        protected override IVertex CreateVertexInstance()
        {
            return new EasyVertex(
                Store,
                VertexIdentifierRegistrationMode.Ephemeral);
        }

        protected override EdgeBase CreateEdge(
            IVertex metaVertex,
            IVertex destVertex)
        {
            return new EdgeBase(
                this,
                metaVertex,
                destVertex);
        }

        protected override void InitializeNewVertexValue(
            IVertex vertex,
            object value)
        {
            if (vertex is EasyVertex easyVertex)
                easyVertex
                    .InitializeValueWithoutGraphChange(
                        value);
            else
                base.InitializeNewVertexValue(
                    vertex,
                    value);
        }

        public override IEdge AddEdge(
            IVertex metaVertex,
            IVertex destVertex)
        {
            bool changesParentStackFrame =
                IsParentStackFrameMeta(metaVertex);
            IEdge addedEdge = null;
            try
            {
                addedEdge = base.AddEdge(
                    metaVertex,
                    destVertex);
                return addedEdge;
            }
            finally
            {
                if (addedEdge != null)
                    UpdateDistinctFromMetaCacheForAdd(
                        addedEdge);

                if (changesParentStackFrame)
                    InvalidateParentStackFrameCache();
            }
        }

        public override void DeleteEdge(IEdge edge)
        {
            bool changesParentStackFrame =
                IsParentStackFrameMeta(edge?.Meta);
            try
            {
                base.DeleteEdge(edge);
            }
            finally
            {
                distinctFromMetaQueryCache?.Clear();

                if (changesParentStackFrame)
                    InvalidateParentStackFrameCache();
            }
        }

        public override void Dispose() { }

        public void AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(IEdge e){
            //IEdge ne = new NoInEdgeInOutVertexEdge(e.From, e.Meta, e.To); // INoInEdgeInOutVertexVertex DIFF
            // but can it work that way?
            // before that there was jus a simple
            try
            {
                OutEdgesRaw.Add(e);
                UpdateDistinctFromMetaCacheForAdd(e);
            }
            finally
            {
                if (IsParentStackFrameMeta(e?.Meta))
                    InvalidateParentStackFrameCache();
            }

            //OutEdgesRaw.Add(ne); //eat this!
        }

        public void AddEdgeForNoInEdgeInOutVertexVertex(IEdge e)
        {
            IEdge ne = new EdgeBase(e.From, e.Meta, e.To); // INoInEdgeInOutVertexVertex DIFF
                                                                          // but can it work that way?
                                                                          // before that there was jus a simple
            //OutEdgesRaw.Add(e);

            try
            {
                OutEdgesRaw.Add(ne); //eat this!
                UpdateDistinctFromMetaCacheForAdd(ne);
            }
            finally
            {
                if (IsParentStackFrameMeta(e?.Meta))
                    InvalidateParentStackFrameCache();
            }
        }

        public void AddRangeOriginalEdges(
            IEnumerable<IEdge> edges)
        {
            int addedEdgeCount = 0;
            try
            {
                addedEdgeCount = EdgeDictionaries.Out
                    .AddRangeOriginalStackEdges(edges);
            }
            finally
            {
                distinctFromMetaQueryCache?.Clear();
                InvalidateParentStackFrameCache();
            }

            ZeroCodePerformanceCounters.RecordOriginalStackEdgeBatch(
                addedEdgeCount);
        }
        
        public override void QueryOutEdges(object meta, object from, out IEdge result, out IList<IEdge> results)
        {
            result = null;
            results = null;
            NoInEdgeInOutVertexVertex current =
                this;
            HashSet<IVertex> visitedFrames =
                null;
            int traversedFrameCount = 0;

            while (true)
            {
                current.QueryLocalOutEdges(
                    meta,
                    from,
                    out result,
                    out results);

                if (result != null ||
                    results != null)
                    return;

                IVertex parentStackFrame =
                    current.GetParentStackFrame();

                if (parentStackFrame == null)
                    return;

                if (!(parentStackFrame is
                    NoInEdgeInOutVertexVertex
                        parentStack))
                {
                    parentStackFrame.QueryOutEdges(
                        meta,
                        from,
                        out result,
                        out results);
                    return;
                }

                traversedFrameCount++;

                if (traversedFrameCount >=
                    ParentCycleTrackingThreshold)
                {
                    visitedFrames ??=
                        new HashSet<IVertex>();

                    if (!visitedFrames.Add(
                        parentStack))
                        return;
                }

                current = parentStack;
            }
        }

        private void QueryLocalOutEdges(
            object meta,
            object from,
            out IEdge result,
            out IList<IEdge> results)
        {
            base.QueryOutEdges(
                meta,
                from,
                out result,
                out results);
        }

        internal void QueryOutEdgesDistinctByFromMeta(
            object meta,
            out IEdge result,
            out IList<IEdge> results,
            out long collapsedEdgeCount)
        {
            result = null;
            results = null;
            collapsedEdgeCount = 0;
            NoInEdgeInOutVertexVertex current = this;
            HashSet<IVertex> visitedFrames = null;
            int traversedFrameCount = 0;

            while (true)
            {
                current.QueryLocalOutEdgesDistinctByFromMeta(
                    meta,
                    out result,
                    out results,
                    out collapsedEdgeCount);

                if (result != null ||
                    results != null)
                    return;

                IVertex parentStackFrame =
                    current.GetParentStackFrame();
                if (parentStackFrame == null)
                    return;

                if (!(parentStackFrame is
                    NoInEdgeInOutVertexVertex parentStack))
                {
                    parentStackFrame.QueryOutEdges(
                        meta,
                        null,
                        out IEdge parentResult,
                        out IList<IEdge> parentResults);
                    CollapseQueryResultByFromMeta(
                        parentResult,
                        parentResults,
                        out result,
                        out results,
                        out collapsedEdgeCount);
                    return;
                }

                traversedFrameCount++;
                if (traversedFrameCount >=
                    ParentCycleTrackingThreshold)
                {
                    visitedFrames ??=
                        new HashSet<IVertex>();
                    if (!visitedFrames.Add(parentStack))
                        return;
                }

                current = parentStack;
            }
        }

        private void QueryLocalOutEdgesDistinctByFromMeta(
            object meta,
            out IEdge result,
            out IList<IEdge> results,
            out long collapsedEdgeCount)
        {
            string queryKey =
                meta as string ??
                meta?.ToString() ??
                "";
            long dependencyEpoch =
                InheritanceDependencyEpoch;
            DistinctFromMetaCacheEntry entry = null;
            bool hasValidEntry =
                distinctFromMetaQueryCache != null &&
                distinctFromMetaQueryCache.TryGetValue(
                    queryKey,
                    out entry) &&
                IsQueryMetaOutIndexCurrent &&
                entry.DependencyEpoch ==
                    dependencyEpoch;

            if (!hasValidEntry)
            {
                base.QueryOutEdges(
                    meta,
                    null,
                    out IEdge matchingEdge,
                    out IList<IEdge> matchingEdges);

                if (matchingEdge == null &&
                    matchingEdges == null)
                {
                    distinctFromMetaQueryCache?.Remove(
                        queryKey);
                    result = null;
                    results = null;
                    collapsedEdgeCount = 0;
                    return;
                }

                entry = new DistinctFromMetaCacheEntry
                {
                    DependencyEpoch =
                        InheritanceDependencyEpoch
                };

                if (matchingEdge != null)
                    entry.AddMatch(matchingEdge);

                if (matchingEdges != null)
                    foreach (IEdge edge in matchingEdges)
                        entry.AddMatch(edge);

                distinctFromMetaQueryCache ??=
                    new Dictionary<string,
                        DistinctFromMetaCacheEntry>(
                            StringComparer.Ordinal);
                distinctFromMetaQueryCache[queryKey] =
                    entry;
            }

            entry.GetResult(
                out result,
                out results);
            collapsedEdgeCount =
                entry.MatchCount -
                entry.RepresentativeCount;
        }

        private static void CollapseQueryResultByFromMeta(
            IEdge matchingEdge,
            IList<IEdge> matchingEdges,
            out IEdge result,
            out IList<IEdge> results,
            out long collapsedEdgeCount)
        {
            var entry =
                new DistinctFromMetaCacheEntry();

            if (matchingEdge != null)
                entry.AddMatch(matchingEdge);

            if (matchingEdges != null)
                foreach (IEdge edge in matchingEdges)
                    entry.AddMatch(edge);

            entry.GetResult(
                out result,
                out results);
            collapsedEdgeCount =
                entry.MatchCount -
                entry.RepresentativeCount;
        }

        private void UpdateDistinctFromMetaCacheForAdd(
            IEdge edge)
        {
            if (distinctFromMetaQueryCache == null ||
                edge?.Meta == null)
                return;

            foreach (KeyValuePair<string,
                DistinctFromMetaCacheEntry> pair in
                distinctFromMetaQueryCache)
            {
                if (!DoesMetaMatchQuery(
                    edge.Meta,
                    pair.Key))
                    continue;

                pair.Value.AddMatch(edge);
                pair.Value.DependencyEpoch =
                    InheritanceDependencyEpoch;
            }
        }

        private static bool DoesMetaMatchQuery(
            IVertex meta,
            string queryKey)
        {
            if (StringComparer.Ordinal.Equals(
                meta?.Value?.ToString() ?? "",
                queryKey))
                return true;

            if (meta == null)
                return false;

            if (GraphUtil.GetQueryOutCount(
                meta,
                "$Inherits",
                null) == 0)
                return false;

            foreach (IVertex parent in
                VertexHelper.GetInheritParents(meta))
                if (StringComparer.Ordinal.Equals(
                    parent?.Value?.ToString() ?? "",
                    queryKey))
                    return true;

            return false;
        }

        internal IVertex GetParentStackFrame()
        {
            object cachedParent =
                cachedParentStackFrame;

            if (cachedParent != null)
            {
                ZeroCodePerformanceCounters
                    .RecordStackParentFrameCacheLookup(true);
                return ReferenceEquals(
                    cachedParent,
                    NoParentStackFrame)
                        ? null
                        : (IVertex)cachedParent;
            }

            ZeroCodePerformanceCounters
                .RecordStackParentFrameCacheLookup(false);
            base.QueryOutEdges(
                "$StackFrameInherits",
                null,
                out IEdge result,
                out IList<IEdge> results);

            IVertex parentStackFrame =
                results != null && results.Count > 0
                    ? results[0].To
                    : result?.To;
            cachedParentStackFrame =
                parentStackFrame ??
                NoParentStackFrame;
            return parentStackFrame;
        }

        private void InvalidateParentStackFrameCache()
        {
            cachedParentStackFrame = null;
        }

        private static bool IsParentStackFrameMeta(
            IVertex meta)
        {
            return ReferenceEquals(
                    meta,
                    MinusZero.Instance.StackFrameInherits) ||
                string.Equals(
                    meta?.Value?.ToString(),
                    "$StackFrameInherits",
                    StringComparison.Ordinal);
        }

        internal bool TryResetForTemporaryPool()
        {
            if (!canUseTemporaryPool ||
                isInTemporaryPool ||
                !IdentifierRegistrationDeferred ||
                ExternalReferenceCount != 0 ||
                StoredInEdgeCount != 0 ||
                StoredMetaInEdgeCount != 0)
                return false;

            if (edgeDictionaries != null)
                edgeDictionaries.Out.Clear();
            ClearDictionaries();
            distinctFromMetaQueryCache?.Clear();
            cachedParentStackFrame = null;
            Value = "";
            isInTemporaryPool = true;
            return true;
        }

        internal void PrepareAfterTemporaryPoolRent()
        {
            if (!isInTemporaryPool)
                throw new InvalidOperationException(
                    "The ZeroCode stack was not in the temporary pool.");

            isInTemporaryPool = false;
        }
    }
}
