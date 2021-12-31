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
        static Dictionary<IVertex, IVisualiser> Visualisers = new Dictionary<IVertex, IVisualiser>();

        public static void AddVisualiser(IVisualiser visuliser)
        {
            Visualisers.Add(visuliser.Vertex, visuliser);
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
