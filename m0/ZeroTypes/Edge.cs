using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace m0.ZeroTypes
{
    public class Edge
    {
        static IVertex vEdge = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge");

        static IVertex vFrom = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge\From");
        static IVertex vMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge\Meta");
        static IVertex vTo = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge\To");

        static public void CopyAndReplaceEdge(IVertex baseVertex, string MetaValue, IVertex EdgeVertex)
        {
            IEdge toReplace = GraphUtil.FindEdgeByMetaValue(baseVertex, MetaValue);

            if (toReplace == null)
                throw new Exception("Vertex does not have \"" + MetaValue + "\" edge");

            baseVertex.DeleteEdge(toReplace);

            IVertex edge = baseVertex.AddVertex(toReplace.Meta,null);

            edge.AddEdge(vFrom, EdgeVertex.Get(false, "From:"));
            edge.AddEdge(vMeta, EdgeVertex.Get(false, "Meta:"));
            edge.AddEdge(vTo, EdgeVertex.Get(false, "To:"));   
        }

        static public void CreateEdgeAndCreateOrReplaceEdgeByMeta(IVertex baseVertex, IVertex metaVertex, IEdge Edge)
        {
            IEdge toReplace = GraphUtil.FindEdgeByMetaVertex(baseVertex, metaVertex);

            IVertex edge;

            if (toReplace == null)
                edge = baseVertex.AddVertex(metaVertex, null);
            else
            {
                baseVertex.DeleteEdge(toReplace);

                edge = baseVertex.AddVertex(toReplace.Meta, null);
            }

            IVertex r = MinusZero.Instance.Root;

            edge.AddEdge(vFrom, Edge.From);
            edge.AddEdge(vMeta, Edge.Meta);
            edge.AddEdge(vTo, Edge.To);
        }

        static public IVertex AddEdgeByToVertex(IVertex baseVertex, IVertex toVertex)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex ev = baseVertex.AddVertex(vEdge, null);

            ev.AddVertex(vFrom, null);
            ev.AddEdge(vMeta,MinusZero.Instance.Empty);
            ev.AddEdge(vTo, toVertex);                        

            return ev;
        }

        static public IVertex AddEdgeByToVertex(IVertex baseVertex, IVertex MetaEdge, IVertex toVertex)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex ev = baseVertex.AddVertex(MetaEdge, null);

            ev.AddVertex(vFrom, null);
            ev.AddEdge(vMeta, MinusZero.Instance.Empty);
            ev.AddEdge(vTo, toVertex);

            return ev;
        }

        static public IVertex AddEdge(IVertex baseVertex, IVertex fromEdge, IVertex metaEdge, IVertex toVertex)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex ev = baseVertex.AddVertex(vEdge, null);

            ev.AddEdge(vFrom, fromEdge);
            ev.AddEdge(vMeta, metaEdge);
            ev.AddEdge(vTo, toVertex);

            return ev;
        }

        static public IVertex AddEdge(IVertex baseVertex, IEdge edge)
        {
            IVertex r=MinusZero.Instance.Root;

            IVertex ev = baseVertex.AddVertex(vEdge, null);

            AddEdgeEdges(ev, edge);

            return ev;
        }

        static public void AddEdge(IVertex baseVertex, IVertex edge)
        {
            baseVertex.AddEdge(vEdge, edge);          
        }

        static public IVertex CreateTempEdge(IEdge edge)
        {
            IVertex ev = MinusZero.Instance.CreateTempVertex();

            AddEdgeEdges(ev, edge);

            return ev;
        }

        static public void ReplaceEdgeEdges(IVertex baseVertex, IEdge edge)
        {
            GraphUtil.ReplaceEdge(baseVertex, "From", edge.From);
            GraphUtil.ReplaceEdge(baseVertex, "Meta", edge.Meta);

            if (edge.To != null) // there are edges with .To==null
                GraphUtil.ReplaceEdge(baseVertex, "To", edge.To);
            else
                GraphUtil.DeleteEdgeByMeta(baseVertex, "To");
            //GraphUtil.ReplaceEdge(baseVertex, "To", MinusZero.Instance.Empty);
        }

        static public void AddEdgeEdges(IVertex baseVertex, IEdge edge)
        {
            baseVertex.AddEdge(vFrom, edge.From);
            baseVertex.AddEdge(vMeta, edge.Meta);
            baseVertex.AddEdge(vTo, edge.To);
        }

        static public void AddEdgeEdges(IVertex baseVertex, IVertex edgeFrom, IVertex edgeMeta, IVertex edgeTo)
        {
            baseVertex.AddEdge(vFrom, edgeFrom);
            baseVertex.AddEdge(vMeta, edgeMeta);
            baseVertex.AddEdge(vTo, edgeTo);
        }

        static public void AddEdgeEdgesByEdge(IVertex baseVertex, IVertex edge)
        {
            IVertex edgeFrom = GraphUtil.GetQueryOutFirst(edge, "From", null);
            IVertex edgeMeta = GraphUtil.GetQueryOutFirst(edge, "Meta", null);
            IVertex edgeTo = GraphUtil.GetQueryOutFirst(edge, "To", null);

            baseVertex.AddEdge(vFrom, edgeFrom);
            baseVertex.AddEdge(vMeta, edgeMeta);
            baseVertex.AddEdge(vTo, edgeTo);
        }

        static public void AddEdgeEdgesOnlyMetaTo(IVertex baseVertex, IVertex edgeMeta, IVertex edgeTo)
        {
            baseVertex.AddVertex(vFrom, null);
            baseVertex.AddEdge(vMeta, edgeMeta);
            baseVertex.AddEdge(vTo, edgeTo);
        }

        static public void AddEdgeEdgesOnlyTo(IVertex baseVertex, IVertex toVertex)
        {
            baseVertex.AddVertex(vFrom, null);
            baseVertex.AddEdge(vMeta, MinusZero.Instance.Empty);
            baseVertex.AddEdge(vTo, toVertex);
        }

        static public IEdge FindEdgeByEdge(IVertex baseVertex, IEdge edge)
        {
            foreach (IEdge e in baseVertex)
                //if (e.To.Get(false, "From:") == edge.From
                  //     && e.To.Get(false, "Meta:") == edge.Meta
                    //   && e.To.Get(false, "To:") == edge.To
                    if (GraphUtil.GetQueryOutFirst(e.To, "From", null) == edge.From // was In and it seems to be wrong
                       && GraphUtil.GetQueryOutFirst(e.To, "Meta", null) == edge.Meta
                       && GraphUtil.GetQueryOutFirst(e.To, "To", null) == edge.To
                       )
                        return e;

            return null;
        }

        static public IEdge FindEdgeByEdgeOnlyToVertex(IVertex baseVertex, IEdge edge)
        {
            foreach (IEdge e in baseVertex)
                //if (e.To.Get(false, "To:") == edge.To)
                if (GraphUtil.GetQueryOutFirst(e.To, "To", null) == edge.To) // was In and it seems to be wrong
                    return e;

            return null;
        }

        static public void DeleteVertexByEdge(IVertex baseVertex, IEdge edge)
        {
            IEdge e = FindEdgeByEdge(baseVertex, edge);

            if (e != null)
                baseVertex.DeleteEdge(e);            
        }

        static public void DeleteVertexByEdgeOnlyToVertex(IVertex baseVertex, IEdge edge)
        {
            IEdge e = FindEdgeByEdgeOnlyToVertex(baseVertex, edge);

            if (e != null)
                baseVertex.DeleteEdge(e);
        }      

        static public IEdge FindEdgeByEdgeTo(IVertex baseVertex, IVertex to)
        {
            foreach (IEdge e in baseVertex)
                // if (e.To.Get(false, "To:") == to)
                if (GraphUtil.GetQueryOutFirst(e.To, "To", null) == to)
                    return e;

            return null;
        }

        static public void DeleteVertexByEdgeTo(IVertex baseVertex, IVertex to)
        {
            IEdge e = FindEdgeByEdgeTo(baseVertex, to);

            if (e != null)
                baseVertex.DeleteEdge(e);
        }
    }
}
