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
                if (GraphUtil.GetValueAndCompareStrings(e.To, "Item"))
                {
                    toCreateType = typeof(Item);
                    return true;
                }

                if(GraphUtil.ExistQueryOut(e.To, "$Inherits", "Item"))
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

        public static bool IsUXItem(IVertex v, out Type toCreateType)
        {
            toCreateType = null;

            IList<IEdge> is_edges = GraphUtil.GetQueryOut(v, "$Is", null);

            foreach (IEdge e in is_edges)
            {
                if (GraphUtil.GetValueAndCompareStrings(e.To, "UXItem"))
                {
                    toCreateType = typeof(UXItem);
                    return true;
                }

                if (GraphUtil.ExistQueryOut(e.To, "$Inherits", "UXItem"))
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

        static public IItem Get_ItemVersion(IEdge edge)
        {
            IVertex v = edge.To;

            if (vertexDictionary.ContainsKey(v))
            {
                ITypedEdge ret = vertexDictionary[v];

                if (ret.Edge.To.DisposedState != DisposeStateEnum.Live)
                    throw new Exception("Vertex not live");

                if (ret is IItem)
                    return (IItem)ret;
                
                return null;                
            }
            else
            {
                Type toCreateType;

                if (!IsItem(v, out toCreateType))
                    return null; 
                                               
                return (IUXItem)Activator.CreateInstance(toCreateType, edge);                
            }
        }

        static public IUXItem Get_UXItemVersion(IEdge edge)
        {
            IVertex v = edge.To;

            if (vertexDictionary.ContainsKey(v))
            {
                ITypedEdge ret = vertexDictionary[v];

                if (ret.Edge.To.DisposedState != DisposeStateEnum.Live)
                    throw new Exception("Vertex not live");

                if (ret is IUXItem)
                    return (IUXItem)ret;

                return (IUXItem)ret;
            }
            else
            {
                Type toCreateType;

                if (!IsUXItem(v, out toCreateType))
                    return null;

                return (IUXItem)Activator.CreateInstance(toCreateType, edge);
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

                return (ITypedEdge)Activator.CreateInstance(toCreateType, edge);
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
