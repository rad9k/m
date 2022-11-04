using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes
{
    public class TypedEdge : ITypedEdge
    {
        public static Dictionary<IVertex, ITypedEdge> vertexDictionary = new Dictionary<IVertex, ITypedEdge>();

        IEdge edge;
        public IEdge Edge { get { return edge; } }

        IVertex vertex;
        public IVertex Vertex { get { return vertex; } }

        public TypedEdge(IEdge _edge)
        {
            edge = _edge;

            vertex = _edge.To;

            vertexDictionary.Add(this.Edge.To, this);
        }

        static public ITypedEdge Get(IEdge edge, Type toCreateType)
        {
            IVertex v = edge.To;

            if (vertexDictionary.ContainsKey(v))
            {
                ITypedEdge ret = vertexDictionary[v];

                if (ret.Edge.To.DisposedState != DisposeStateEnum.Live)
                    throw new Exception("Vertex not live");

                //if (EdgeHelper.CompareIEdges(ret.Edge, edge))
                    return ret;
               // else
                 //   throw new Exception("Vertex allready in TypedEdge.vertexDictionary. Tried to access from another Edge.");
            }
            else
            {
                ITypedEdge te = (ITypedEdge)Activator.CreateInstance(toCreateType, edge);

                return te;
            }           
        }
    }
}
