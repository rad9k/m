using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes
{
    public class TypedEdge : ITypedEdge
    {
        public static Dictionary<IVertex, ITypedEdge> vertexDictionary = new Dictionary<IVertex, ITypedEdge>();
        
        public IEdge Edge { get; set; }

        IVertex vertex;
        public IVertex Vertex { get { return vertex; } }

        public TypedEdge(IEdge _edge)
        {
            Edge = _edge;

            vertex = _edge.To;

            AddToDictinary(this);
        }

        static public void AddToDictinary(ITypedEdge typedEdge)
        {
            if (!vertexDictionary.ContainsKey(typedEdge.Edge.To))
                vertexDictionary.Add(typedEdge.Edge.To, typedEdge);
        }

        static public void RemoveFromDictionary(ITypedEdge e)
        {
            vertexDictionary.Remove(e.Vertex);
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

        static public ITypedEdge GetFromDictionary(IVertex v)
        {
            if (vertexDictionary.ContainsKey(v))
            {
                ITypedEdge ret = vertexDictionary[v];

                if (ret.Edge.To.DisposedState != DisposeStateEnum.Live)
                    throw new Exception("Vertex not live");

                return ret;
            }
            else
                return null;
        }

        public static Type GetPlatformClass(IVertex v)
        {            
            IList<IEdge> is_edges = GraphUtil.GetQueryOut(v, "$Is", null);

            foreach (IEdge e in is_edges)
            {
                IVertex pcnv = GraphUtil.GetQueryOutFirst(e.To, "$PlatformClassName", null);

                if (pcnv != null)
                    return Type.GetType(pcnv.Value.ToString());

                return null;
            }

            return null;
        }

        static public ITypedEdge Get(IEdge edge)
        {
            IVertex v = edge.To;

            if (vertexDictionary.ContainsKey(v))
            {
                ITypedEdge ret = vertexDictionary[v];

                if (ret.Edge.To.DisposedState != DisposeStateEnum.Live)
                    throw new Exception("Vertex not live");
                
                return ret;
            }
            else
            {
                Type toCreateType = GetPlatformClass(v);

                if (toCreateType == null)
                    return null;

                // as all of the m0.UIWpf.Visualisers.* are not created with Edges (constructor used is Vertex based),
                // we can not use those objects as fully working ItypedEdges, so that is why we will need to create a separate
                // object for those

                //if (!toCreateType.GetInterfaces().Contains(typeof(IItem))) // << so far this was only triggered by UXTemplate and that was not the idea here

                Type[] interfacesInToCreateType = toCreateType.GetInterfaces();

                if (m0.MinusZero.Instance.UserInteraction.TypedEdge_Get_Test(interfacesInToCreateType))
                    toCreateType = typeof(Edge);

                object obj = Activator.CreateInstance(toCreateType, edge);

                if (obj is ITypedEdge)
                    return (ITypedEdge)obj;

                return null;
            }
        }

        public bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;

                RemoveFromDictionary(this);
            }
        }
    }
}