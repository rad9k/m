using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace m0.ZeroTypes.UX
{
    public class TypedEdgeUserControl: Control, ITypedEdge
    {
        IEdge edge;
        public IEdge Edge { get { return edge; } }

        IVertex vertex;
        public IVertex Vertex { get { return vertex; } }

        public TypedEdgeUserControl(IEdge _edge)
        {
            edge = _edge;

            vertex = _edge.To;

            TypedEdge.vertexDictionary.Add(this.Edge.To, this);
        }
    }
}
