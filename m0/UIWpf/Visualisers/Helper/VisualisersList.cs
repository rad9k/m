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

        public static bool x = false;
        public static void AddVisualiser(IVisualiser visualiser, IVertex parentVisualiserVertex, bool AddVertex)
        {
            MinusZero mz = MinusZero.Instance;

            IEdge visualiserVertexEdge = null;

            if (AddVertex)
            {
                if (parentVisualiserVertex == null)
                    visualiserVertexEdge = mz.Root.Get(false, @"User\CurrentUser:\Session:\Visualisers:").
                            AddEdge(mz.Root.Get(false, @"System\Meta\ZeroTypes\UX\Item"), visualiser.Vertex);
                else
                {
                   // if (x)
                       // visualiserVertexEdge = mz.Root.Get(false, @"User\CurrentUser:\Session:\Visualisers:").
                       //     AddEdge(mz.Root.Get(false, @"System\Meta\ZeroTypes\UX\Item"), visualiser.Vertex);
                   
                    //visualiserVertexEdge = parentVisualiserVertex.
                      //             AddEdge(mz.Root.Get(false, @"System\Meta\Base\$Empty"), visualiser.Vertex);
                    //else
                        visualiserVertexEdge = parentVisualiserVertex.
                               AddEdge(mz.Root.Get(false, @"System\Meta\ZeroTypes\UX\Item\Item"), visualiser.Vertex);

                }
            }

            VisualiserData vd = new VisualiserData();
            vd.Visualiser = visualiser;
            vd.ParentVisualiserVertexEdge = visualiserVertexEdge;

            Visualisers.Add(visualiser.Vertex, vd);
        }

        public static void RemoveVisualiser(IVisualiser visualiser)
        {
            if (!Visualisers.ContainsKey(visualiser.Vertex))
                return;

            IEdge visualiserVertexEdge = Visualisers[visualiser.Vertex].ParentVisualiserVertexEdge;

            if (visualiserVertexEdge != null && visualiserVertexEdge.From.DisposedState == DisposeStateEnum.Live)
                visualiserVertexEdge.From.DeleteEdge(visualiserVertexEdge);

            Visualisers.Remove(visualiser.Vertex);
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
