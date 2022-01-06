using m0.Foundation;
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
        public IEdge VisualiserVertexEdge;
    }

    public class VisualisersList
    {
        static Dictionary<IVertex, VisualiserData> Visualisers = new Dictionary<IVertex, VisualiserData>();

        public static void AddVisualiser(IVisualiser visualiser, IVertex parentVisualiserVertex)
        {
            MinusZero mz = MinusZero.Instance;

            IEdge visualiserVertexEdge;

            if(parentVisualiserVertex == null)
                visualiserVertexEdge = mz.Root.Get(false, @"User\CurrentUser:\Session:\Visualisers:").
                        AddEdge(mz.Root.Get(false, @"System\Meta\Visualiser\AbstractVisualiser"), visualiser.Vertex);
            else
                visualiserVertexEdge = parentVisualiserVertex.
                        AddEdge(mz.Root.Get(false, @"System\Meta\Visualiser\AbstractVisualiser\SubVisualiser"), visualiser.Vertex);


            VisualiserData vd = new VisualiserData();
            vd.Visualiser = visualiser;
            vd.VisualiserVertexEdge = visualiserVertexEdge;

            Visualisers.Add(visualiser.Vertex, vd);
        }

        public static void RemoveVisualiser(IVisualiser visualiser)
        {
            if (!Visualisers.ContainsKey(visualiser.Vertex))
                return;

            IEdge visualiserVertexEdge = Visualisers[visualiser.Vertex].VisualiserVertexEdge;

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
