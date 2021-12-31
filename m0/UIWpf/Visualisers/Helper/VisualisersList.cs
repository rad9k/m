using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.UIWpf.Visualisers.Helper
{
    public class VisualisersList
    {
        static Dictionary<IVisualiser> Visualisers = new Dictionary<IVertex, IVisualiser>();

        public static void AddVisualiser(IVisualiser visualiser)
        {
            MinusZero mz = MinusZero.Instance;

            Visualisers.Add(visualiser.Vertex, visualiser);

            visualiser.VisualiserVertexEdge = mz.Root.Get(false, @"User\CurrentUser:\Session:\Visualisers:").
                        AddEdge(mz.Root.Get(false, @"Meta\User\VisualiserList\Visualiser"), vVertex);
        }

        public static void RemoveVisualiser(IVisualiser visualiser)
        {
            Visualisers.Remove(visualiser.Vertex);
        }

        public static void RemoveAllVisualisers()
        {
            foreach (IVisualiser visualiser in Visualisers.Values)
                RemoveVisualiser(visualiser);
        }

    }
}
