using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes
{
    public class TypedEdge
    {
        static Dictionary<IVertex, TypedEdge> vertexDictionary = new Dictionary<IVertex, TypedEdge>();

        public IEdge Edge { get; set; }

        public IVertex Vertex;

        public TypedEdge(IEdge edge)
        {
            Edge = edge;

            Vertex = edge.To;

            vertexDictionary.Add(this.Edge.To, this);
        }

        static public TypedEdge Get(IVertex vertex)
        {
            if (vertexDictionary.ContainsKey(vertex))
            {
                TypedEdge ret = vertexDictionary[vertex];

                if (ret.Edge.To.DisposedState != DisposeStateEnum.Live)
                    throw new Exception("Vertex not live");

                return ret; 
            }

            return null;
        }
    }
}
