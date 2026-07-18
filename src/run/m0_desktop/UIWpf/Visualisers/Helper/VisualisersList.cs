using m0.Foundation;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.UIWpf.Visualisers.Helper
{
    class VisualiserData
    {
        public IVisualiser Visualiser;
        public IEdge ParentVisualiserVertexEdge;
    }

    public class VisualisersList
    {
        static Dictionary<IVertex, VisualiserData> Visualisers = new Dictionary<IVertex, VisualiserData>();

        static IVertex UserCurrentUserSessionVisualisers_vertex = MinusZero.Instance.root.Get(false, @"Home:\CurrentUser:\Session:\Visualisers:");
        static IVertex SystemMetaZeroTypexUXItem_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item");
        static IVertex SystemMetaZeroTypexUXVolatileItem_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\Item\VolatileItem");

        public static void AddVisualiser(IVisualiser visualiser, IVertex parentVisualiserVertex, bool AddVertex, bool isVolatile)
        {
            MinusZero mz = MinusZero.Instance;

            IEdge visualiserVertexEdge = null;

            if (AddVertex)
            {
                if (parentVisualiserVertex == null)
                    visualiserVertexEdge = UserCurrentUserSessionVisualisers_vertex.
                            AddEdge(SystemMetaZeroTypexUXItem_meta, visualiser.Vertex);
                else
                {
                    if (isVolatile)
                        visualiserVertexEdge = parentVisualiserVertex.
                               AddEdge(SystemMetaZeroTypexUXVolatileItem_meta, visualiser.Vertex);
                    else
                        visualiserVertexEdge = parentVisualiserVertex.
                               AddEdge(SystemMetaZeroTypexUXItem_meta, visualiser.Vertex);
                }
            }

            VisualiserData vd = new VisualiserData();
            vd.Visualiser = visualiser;
            vd.ParentVisualiserVertexEdge = visualiserVertexEdge;

            Visualisers.Add(visualiser.Vertex, vd);

            MinusZero.Instance.Log(1, "VisualisersList",
                "AddVisualiser type=" + (visualiser == null ? "null" : visualiser.GetType().Name)
                + " vertex=" + DescribeVertex(visualiser == null ? null : visualiser.Vertex)
                + " parent=" + DescribeVertex(parentVisualiserVertex)
                + " AddVertex=" + AddVertex
                + " isVolatile=" + isVolatile
                + " parentItemEdgeCreated=" + (visualiserVertexEdge != null));
        }

        public static void RemoveVisualiser(IVisualiser visualiser)
        {
            if (visualiser == null || visualiser.Vertex == null)
            {
                MinusZero.Instance.Log(1, "VisualisersList", "RemoveVisualiser skipped null visualiser/vertex");
                return;
            }

            if (!Visualisers.ContainsKey(visualiser.Vertex))
            {
                MinusZero.Instance.Log(1, "VisualisersList",
                    "RemoveVisualiser notFound type=" + visualiser.GetType().Name
                    + " vertex=" + DescribeVertex(visualiser.Vertex));
                return;
            }

            IEdge visualiserVertexEdge = Visualisers[visualiser.Vertex].ParentVisualiserVertexEdge;

            if (visualiserVertexEdge != null && visualiserVertexEdge.From.DisposedState == DisposeStateEnum.Live)
                visualiserVertexEdge.From.DeleteEdge(visualiserVertexEdge);

            Visualisers.Remove(visualiser.Vertex);

            MinusZero.Instance.Log(1, "VisualisersList",
                "RemoveVisualiser type=" + visualiser.GetType().Name
                + " vertex=" + DescribeVertex(visualiser.Vertex)
                + " hadParentItemEdge=" + (visualiserVertexEdge != null));
        }

        private static string DescribeVertex(IVertex vertex)
        {
            if (vertex == null)
                return "null";

            return "val=" + (vertex.Value == null ? "null" : vertex.Value.ToString())
                + " hash=" + vertex.GetHashCode();
        }

        public static void RemoveAllVisualisers()
        {
            foreach (VisualiserData vd in Visualisers.Values.ToList())
                vd.Visualiser.Dispose();                
        }

        public static IVisualiser GetVisualiser(IVertex visualiserVertex) {
            if (Visualisers.ContainsKey(visualiserVertex))
                return Visualisers[visualiserVertex].Visualiser;
            
            return null;
        }

    }
}
