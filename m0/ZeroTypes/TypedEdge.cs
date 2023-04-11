using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media.Converters;

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

        public static bool IsItem(IVertex v, out Type toCreateType)
        {
            toCreateType = null;

            IList<IEdge> is_edges = GraphUtil.GetQueryOut(v, "$Is", null);

            foreach(IEdge e in is_edges)
            {
                if(GraphUtil.ExistQueryOut(e.To, "$Inherits", "UXItem"))
                {
                    string pcn = GraphUtil.GetQueryOutFirst(e.To, "$PlatformClassName", null).Value.ToString();

                    if (pcn == null)
                        return false;

                    toCreateType = Type.GetType(pcn);
                    return true;
                }
            }

            return false;            
        }

        public static Type GetPlatformClass(IVertex v)
        {            
            IList<IEdge> is_edges = GraphUtil.GetQueryOut(v, "$Is", null);

            foreach (IEdge e in is_edges)
            {               
                string pcn = GraphUtil.GetQueryOutFirst(e.To, "$PlatformClassName", null).Value.ToString();

                if (pcn == null)
                    return null;

                return Type.GetType(pcn);                                
            }

            return null;
        }

        static public ITypedEdge Get_itemVerstion(IEdge edge)
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
                Type toCreateType;

                if (!IsItem(v, out toCreateType))
                    return null; 
                                               
                ITypedEdge te = (ITypedEdge)Activator.CreateInstance(toCreateType, edge);

                return te;
            }
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

                ITypedEdge te = (ITypedEdge)Activator.CreateInstance(toCreateType, edge);

                return te;
            }
        }

        public bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                RemoveFromDictionary(this);
            }
        }
    }
}
