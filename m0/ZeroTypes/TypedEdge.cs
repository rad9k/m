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

        public IEdge Edge; 

        public IVertex Vertex;

        public TypedEdge(IEdge edge)
        {
            Edge = edge;

            Vertex = edge.To;

            vertexDictionary.Add(this.Edge.To, this);
        }

        static public TypedEdge Get(IEdge edge, Type toCreateType)
        {
            IVertex v = edge.To;

            if (vertexDictionary.ContainsKey(v))
            {
                TypedEdge ret = vertexDictionary[v];

                if (ret.Edge.To.DisposedState != DisposeStateEnum.Live)
                    throw new Exception("Vertex not live");

                if (ret.Edge == edge)
                    return ret;
                else
                    throw new Exception("Vertex allready in TypedEdge.vertexDictionary. Tried to access from another Edge.");
            }
            else
            {
                TypedEdge te = (TypedEdge)Activator.CreateInstance(toCreateType);

                te.Edge = edge;
                te.Vertex = edge.To;

                vertexDictionary.Add(te.Vertex, te);

                return te;
            }

            
        }
    }
}
