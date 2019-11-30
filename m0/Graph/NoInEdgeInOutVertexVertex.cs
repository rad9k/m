using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Util;

namespace m0.Graph
{
    public class NoInEdgeInOutVertexVertex: EasyVertex, INoInEdgeInOutVertexVertex
    {
        public override IEdge AddEdge(Foundation.IVertex metaVertex, Foundation.IVertex destVertex)
        {           
            if (destVertex == null)
                destVertex = MinusZero.Instance.Empty; // can be

            IEdge ne = new NoInEdgeInOutVertexEdge(this, metaVertex, destVertex);

            OutEdgesRaw.Add(ne);

            UsageCounter++;

            OutEdgesDictionariesNeedsRebuild = true;

            InheritChildsDictionariesNeedsRebuild(false);

            if (GeneralUtil.CompareStrings(ne.Meta.Value, "$Inherits"))
            {
                InheritanceCount++;

                HasInheritance = true;
            }

            FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeAdded, ne));

            return ne;
        }

        public NoInEdgeInOutVertexVertex(IStore _store) : base(_store) { }

        public void AddEdgeForNoInEdgeInOutVertexVertex(IEdge e){
            OutEdgesRaw.Add(e);
        }

        public override void DeleteEdge(IEdge _edge)
        {
            IEdge edge = _edge;

            if (!OutEdgesRaw.Contains(edge))
                foreach (IEdge e in OutEdgesRaw)
                    if (e.Meta == _edge.Meta && e.To == _edge.To)
                        edge = e;

            if (edge != null)
            {
                OutEdgesRaw.Remove(edge);

                UsageCounter--;

                OutEdgesDictionariesNeedsRebuild = true;
                InheritChildsDictionariesNeedsRebuild(false);

                if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
                {
                    InheritanceCount--;

                    if (InheritanceCount == 0)
                        HasInheritance = false;
                }

                FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, edge));
            }
        }

        public override void QueryOutEdges(object meta, object from, out IEdge result, out IList<IEdge> results)
        {
            result = null;
            results = null;

            base.QueryOutEdges(meta, from, out result, out results);

            if (result != null || results != null)
                return;

            IEdge _result;
            IList<IEdge> _results;

            base.QueryOutEdges("$StackFrameInherits", null, out _result, out _results);

            IVertex stackFrameInherits_inheritsFrom = null;

            if (_results != null)
                stackFrameInherits_inheritsFrom = _results[0].To;

            if (_result != null)
                stackFrameInherits_inheritsFrom = _result.To;

            if (stackFrameInherits_inheritsFrom != null)
                stackFrameInherits_inheritsFrom.QueryOutEdges(meta, from, out result, out results);
        }
    }
}
