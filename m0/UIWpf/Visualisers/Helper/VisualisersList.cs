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

        public static void AddVisualiser(IVisualiser visualiser, IVisualiser parent)
        {
            MinusZero mz = MinusZero.Instance;

            IEdge visualiserVertexEdge;

            if(parent == null)
                visualiserVertexEdge = mz.Root.Get(false, @"User\CurrentUser:\Session:\Visualisers:").
                        AddEdge(mz.Root.Get(false, @"Meta\User\VisualiserList\Visualiser"), visualiser.Vertex);
            else
                visualiserVertexEdge = parent.Vertex.
                        AddEdge(mz.Root.Get(false, @"Meta\User\VisualiserList\Visualiser"), visualiser.Vertex);



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
            foreach (IVisualiser visualiser in Visualisers.Keys.ToList())
                visualiser.Dispose();                
        }

    }
}
