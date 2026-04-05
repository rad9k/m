using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace m0.ZeroTypes
{
    public class EdgeHelper
    {
        public static IVertex EdgeMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge");

        public static IVertex FromMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge\From");
        public static IVertex MetaMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge\Meta");
        public static IVertex ToMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge\To");

        static IVertex vIs = MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$Is");

        static public void CopyAndReplaceEdgeVertexByEdgeVertex(IVertex baseVertex, string MetaValue, IVertex EdgeVertex)
        {
            IEdge toReplace = GraphUtil.FindEdgeByMetaValue(baseVertex, MetaValue);

            if (toReplace == null)
                throw new Exception("Vertex does not have \"" + MetaValue + "\" edge");

            IVertex meta = toReplace.Meta;

            baseVertex.DeleteEdge(toReplace);

            IVertex edge = baseVertex.AddVertex(meta, null);

            edge.AddEdge(vIs, EdgeMeta);

            IVertex EdgeVertex_From = GraphUtil.GetQueryOutFirst(EdgeVertex, "From", null);
            IVertex EdgeVertex_Meta = GraphUtil.GetQueryOutFirst(EdgeVertex, "Meta", null);
            IVertex EdgeVertex_To = GraphUtil.GetQueryOutFirst(EdgeVertex, "To", null);

            edge.AddEdge(FromMeta, EdgeVertex_From);
            edge.AddEdge(MetaMeta, EdgeVertex_Meta);
            edge.AddEdge(ToMeta, EdgeVertex_To);   
        }

        static public void CreateOrReplaceEdgeVertexFromIEdgeByMeta(IVertex baseVertex, IVertex metaVertex, IEdge Edge)
        {
            IEdge toReplace = GraphUtil.FindEdgeByMetaVertex(baseVertex, metaVertex);

            IVertex edge;

            if (toReplace == null)
                edge = baseVertex.AddVertex(metaVertex, null);
            else
            {
                IVertex meta = toReplace.Meta;

                baseVertex.DeleteEdge(toReplace);

                edge = baseVertex.AddVertex(meta, null);
            }            

            edge.AddEdge(vIs, EdgeMeta);

            edge.AddEdge(FromMeta, Edge.From);
            edge.AddEdge(MetaMeta, Edge.Meta);
            edge.AddEdge(ToMeta, Edge.To);
        }

        static public IVertex AddEdgeVertexByToVertex(IVertex baseVertex, IVertex toVertex)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex ev = baseVertex.AddVertex(EdgeMeta, null);

            ev.AddEdge(vIs, EdgeMeta);

            ev.AddVertex(FromMeta, null);
            ev.AddEdge(MetaMeta,MinusZero.Instance.Empty);
            ev.AddEdge(ToMeta, toVertex);                        

            return ev;
        }

        static public IVertex AddEdgeVertexByToVertexByMeta(IVertex baseVertex, IVertex MetaEdge, IVertex toVertex)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex ev = baseVertex.AddVertex(MetaEdge, null);

            ev.AddEdge(vIs, EdgeMeta);

            ev.AddVertex(FromMeta, null);
            ev.AddEdge(MetaMeta, MinusZero.Instance.Empty);
            ev.AddEdge(ToMeta, toVertex);

            return ev;
        }

        static public IVertex AddEdgeVertex(IVertex baseVertex, IVertex fromVertex, IVertex metaVertex, IVertex toVertex)
        {
            return AddEdgeVertex(baseVertex, fromVertex, metaVertex, toVertex, null);
        }

        static public IVertex AddEdgeVertex(IVertex baseVertex, IVertex fromVertex, IVertex metaVertex, IVertex toVertex, string name)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex ev = baseVertex.AddVertex(EdgeMeta, name);

            ev.AddEdge(vIs, EdgeMeta);

            ev.AddEdge(FromMeta, fromVertex);
            ev.AddEdge(MetaMeta, metaVertex);
            ev.AddEdge(ToMeta, toVertex);

            return ev;
        }

        static public IVertex AddEdgeVertex(IVertex baseVertex, IEdge edge)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex ev = baseVertex.AddVertex(EdgeMeta, null);

            AddEdgeVertexEdges(ev, edge);

            return ev;
        }

        static public void AddEdgeVertexEdgeByEdgeVertex(IVertex baseVertex, IVertex edge)
        {
            baseVertex.AddEdge(EdgeMeta, edge);          
        }        

        static public IVertex CreateTempEdgeVertex(IEdge edge)
        {
            IVertex ev = MinusZero.Instance.CreateTempVertex();

            AddEdgeVertexEdges(ev, edge);

            return ev;
        }

        static public IVertex CreateTempEdgeVertex(IVertex from, IVertex meta, IVertex to)
        {
            IVertex ev = MinusZero.Instance.CreateTempVertex();

            AddEdgeVertexEdges(ev, from, meta, to);

            return ev;
        }

        static public void AddOrReplaceEdgeVertexEdges(IVertex baseVertex, IEdge edge)
        {
            if (GraphUtil.ExistQueryOut(baseVertex, "From", null))
                ReplaceEdgeVertexEdges(baseVertex, edge);
            else
                AddEdgeVertexEdges(baseVertex, edge);
        }

        static public void ReplaceEdgeVertexEdges(IVertex baseVertex, IEdge edge)
        {
            GraphUtil.ReplaceEdge(baseVertex, "From", edge.From);
            GraphUtil.ReplaceEdge(baseVertex, "Meta", edge.Meta);

            if (edge.To != null) // there are edges with .To==null
                GraphUtil.ReplaceEdge(baseVertex, "To", edge.To);
            else
                GraphUtil.DeleteEdgeByMeta(baseVertex, "To");
            //GraphUtil.ReplaceEdge(baseVertex, "To", MinusZero.Instance.Empty);
        }

        static public void AddEdgeVertexEdges(IVertex baseVertex, IEdge edge)
        {
            baseVertex.AddEdge(vIs, EdgeMeta);

            if (edge.From !=null && edge.From.DisposedState == DisposeStateEnum.Live)
                baseVertex.AddEdge(FromMeta, edge.From);
            else
                baseVertex.AddEdge(FromMeta, MinusZero.Instance.Empty);

            if (edge.Meta != null && edge.Meta.DisposedState == DisposeStateEnum.Live)
                baseVertex.AddEdge(MetaMeta, edge.Meta);
            else
                baseVertex.AddEdge(MetaMeta, MinusZero.Instance.Empty);

            if (edge.To != null && edge.To.DisposedState == DisposeStateEnum.Live)
                baseVertex.AddEdge(ToMeta, edge.To);
            else
                baseVertex.AddEdge(ToMeta, MinusZero.Instance.Empty);
        }

        static public void AddEdgeVertexEdges(IVertex baseVertex, IVertex edgeFrom, IVertex edgeMeta, IVertex edgeTo)
        {
            baseVertex.AddEdge(vIs, EdgeMeta);

            baseVertex.AddEdge(FromMeta, edgeFrom);
            baseVertex.AddEdge(MetaMeta, edgeMeta);
            baseVertex.AddEdge(ToMeta, edgeTo);
        }

        static public void AddEdgeVertexEdgesByEdgeVertex(IVertex baseVertex, IVertex edge)
        {
            IVertex edgeFrom = GraphUtil.GetQueryOutFirst(edge, "From", null);
            IVertex edgeMeta = GraphUtil.GetQueryOutFirst(edge, "Meta", null);
            IVertex edgeTo = GraphUtil.GetQueryOutFirst(edge, "To", null);

            baseVertex.AddEdge(vIs, EdgeMeta);

            baseVertex.AddEdge(FromMeta, edgeFrom);
            baseVertex.AddEdge(MetaMeta, edgeMeta);
            baseVertex.AddEdge(ToMeta, edgeTo);
        }

        static public void AddEdgeVertexEdgesOnlyMetaTo(IVertex baseVertex, IVertex edgeMeta, IVertex edgeTo)
        {
            baseVertex.AddEdge(vIs, EdgeMeta);

            baseVertex.AddVertex(FromMeta, null);
            baseVertex.AddEdge(MetaMeta, edgeMeta);
            baseVertex.AddEdge(ToMeta, edgeTo);
        }

        static public void AddEdgeVertexEdgesOnlyTo(IVertex baseVertex, IVertex toVertex)
        {
            baseVertex.AddEdge(vIs, EdgeMeta);

            baseVertex.AddVertex(FromMeta, null);
            baseVertex.AddEdge(MetaMeta, MinusZero.Instance.Empty);
            baseVertex.AddEdge(ToMeta, toVertex);
        }

        static public IEdge FindIEdgeVertexByIEdge(IVertex baseVertex, IEdge edge)
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

        static public IEdge FindIEdgeByEdgeVertex(IVertex baseVertex, IVertex edge)
        {
            IVertex from = GraphUtil.GetQueryOutFirst(edge, "From", null);
            IVertex meta = GraphUtil.GetQueryOutFirst(edge, "Meta", null);
            IVertex to = GraphUtil.GetQueryOutFirst(edge, "To", null);

            if (from == null || meta == null || to == null)
                return null;

            foreach (IEdge e in baseVertex)                
                if (e.From == from 
                   && e.Meta == meta
                   && e.To == to
                   )
                    return e;

            return null;
        }

        static public IEdge GetIEdgeByEdgeVertex(IVertex edge)
        {            
            IVertex baseVertex = GraphUtil.GetQueryOutFirst(edge, "From", null);

            if (baseVertex == null)
                return null;

            return FindIEdgeByEdgeVertex(baseVertex, edge);
        }

        static public IEdge FindEdgeVertexByIEdgeOnlyToVertex(IVertex baseVertex, IEdge edge)
        {
            foreach (IEdge e in baseVertex)
                //if (e.To.Get(false, "To:") == edge.To)
                if (GraphUtil.GetQueryOutFirst(e.To, "To", null) == edge.To) // was In and it seems to be wrong
                    return e;

            return null;
        }

        static public IEdge FindEdgeVertexByToVertex(IVertex baseVertex, IVertex toVertex)
        {
            foreach (IEdge e in baseVertex)
                //if (e.To.Get(false, "To:") == edge.To)
                if (GraphUtil.GetQueryOutFirst(e.To, "To", null) == toVertex) // was In and it seems to be wrong
                    return e;

            return null;
        }

        static public void DeleteVertexByEdge(IVertex baseVertex, IEdge edge)
        {
            IEdge e = FindIEdgeVertexByIEdge(baseVertex, edge);

            if (e != null)
                baseVertex.DeleteEdge(e);            
        }

        static public void DeleteVertexByEdgeOnlyToVertex(IVertex baseVertex, IEdge edge)
        {
            IEdge e = FindEdgeVertexByToVertex(baseVertex, edge.To);

            if (e != null)
                baseVertex.DeleteEdge(e);
        }      

        static public void DeleteVertexByEdgeTo(IVertex baseVertex, IVertex to)
        {
            IEdge e = FindEdgeVertexByToVertex(baseVertex, to);

            if (e != null)
                baseVertex.DeleteEdge(e);
        }

        static public bool CompareIEdges(IEdge edge_A, IEdge edge_B)
        {
            if (edge_A.From == edge_B.From
                && edge_A.Meta == edge_B.Meta
                && edge_A.To == edge_B.To)
                return true;

            return false;
        }

        static public IEdge CreateIEdgeFromEdgeVertex(IVertex edgeVertex)
        {
            IVertex edgeVertex_From = GraphUtil.GetQueryOutFirst(edgeVertex, "From", null);
            IVertex edgeVertex_Meta = GraphUtil.GetQueryOutFirst(edgeVertex, "Meta", null);
            IVertex edgeVertex_To = GraphUtil.GetQueryOutFirst(edgeVertex, "To", null);

            return new EasyEdge(edgeVertex_From,
                edgeVertex_Meta,
                edgeVertex_To);
        }
    }
}
