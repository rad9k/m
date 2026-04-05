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
        public NoInEdgeInOutVertexVertex(IStore _Store): base(_Store)
        {
            AllowInheritance = false;
            CanEmitGraphChangeEvents = false;
            edgeDictionaries.NoInEdgeInOutVertexVertexMode = true;
        }

        protected override IVertex CreateVertexInstance()
        {
            return new EasyVertex(this.Store);                
        }

        public override void Dispose() { }

        public void AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(IEdge e){
            //IEdge ne = new NoInEdgeInOutVertexEdge(e.From, e.Meta, e.To); // INoInEdgeInOutVertexVertex DIFF
            // but can it work that way?
            // before that there was jus a simple
             OutEdgesRaw.Add(e);

            //OutEdgesRaw.Add(ne); //eat this!
        }

        public void AddEdgeForNoInEdgeInOutVertexVertex(IEdge e)
        {
            IEdge ne = new EdgeBase(e.From, e.Meta, e.To); // INoInEdgeInOutVertexVertex DIFF
                                                                          // but can it work that way?
                                                                          // before that there was jus a simple
            //OutEdgesRaw.Add(e);

            OutEdgesRaw.Add(ne); //eat this!
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
