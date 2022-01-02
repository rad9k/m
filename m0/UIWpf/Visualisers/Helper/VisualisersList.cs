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
        static Dictionary<IVisualiser, IEdge> Visualisers = new Dictionary<IVisualiser, IEdge>();

        public static void AddVisualiser(IVisualiser visualiser, IVisualiser parent)
        {
            MinusZero mz = MinusZero.Instance;

            IEdge visualiserVertexEdge = mz.Root.Get(false, @"User\CurrentUser:\Session:\Visualisers:").
                        AddEdge(mz.Root.Get(false, @"Meta\User\VisualiserList\Visualiser"), visualiser.Vertex);

            Visualisers.Add(visualiser, visualiserVertexEdge);

            
        }

        public static void RemoveVisualiser(IVisualiser visualiser)
        {
            if (!Visualisers.ContainsKey(visualiser))
                return;

            IEdge visualiserVertexEdge = Visualisers[visualiser];

            visualiserVertexEdge.From.DeleteEdge(visualiserVertexEdge);

            Visualisers.Remove(visualiser);
        }

        public static void RemoveAllVisualisers()
        {
            foreach (IVisualiser visualiser in Visualisers.Values)
                RemoveVisualiser(visualiser);
        }

    }
}
