using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Util;

namespace m0.Graph
{
    public class NoInEdgeInOutVertexVertex: EasyVertex
    {
        public override IEdge AddEdge(Foundation.IVertex metaVertex, Foundation.IVertex destVertex)
        {           
            IEdge ne = new NoInEdgeInOutVertexEdge(this, metaVertex, destVertex);

            OutEdgesRaw.Add(ne);

            UsageCounter++;

            
            FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeAdded, ne));

            return ne;
        }

        public NoInEdgeInOutVertexVertex(IStore _store) : base(_store) { }

        public void AddEdgeForNoInEdgeInOutVertexVertex(IEdge e){
            OutEdgesRaw.Add(e);
        }

        public override void DeleteEdge(IEdge _edge)
        {
            _edge.From.DeleteEdge(_edge);
        }            
    }
}
