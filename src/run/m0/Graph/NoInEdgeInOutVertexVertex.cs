using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
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
        }

        protected override IVertex CreateVertexInstance()
        {
            return new EasyVertex(this.Store);                
        }

        public override IEdge AddEdge(
            IVertex metaVertex,
            IVertex destVertex)
        {
            try
            {
                return base.AddEdge(
                    metaVertex,
                    destVertex);
            }
            finally
            {
                InvalidateParentStackFrameCache();
            }
        }

        public override void DeleteEdge(IEdge edge)
        {
            try
            {
                base.DeleteEdge(edge);
            }
            finally
            {
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
            }
            finally
            {
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
            }
            finally
            {
                InvalidateParentStackFrameCache();
            }
        }

        public void AddRangeOriginalEdges(
            IEnumerable<IEdge> edges)
        {
            int addedEdgeCount = 0;
            try
            {
                addedEdgeCount = edgeDictionaries.Out
                    .AddRangeOriginalStackEdges(edges);
            }
            finally
            {
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

        private IVertex GetParentStackFrame()
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
    }
}
