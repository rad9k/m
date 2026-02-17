using m0.Foundation;
using m0.Graph;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.UIWpf.Visualisers.Helper
{
    public class VisualiserUtil
    {
        public static void RemoveAllSelectedEdges(IVisualiser visualiser)
        {
            IVertex selectedEdges = visualiser.Vertex.Get(false, "SelectedEdges:");

            foreach (IEdge e in selectedEdges.GetAll(false, @"{$Is:Edge}"))
                selectedEdges.DeleteEdge(e);
        }

        static int GetVisualiserLevel(IVertex visualiserVertex)
        {
            if (GraphUtil.GetQueryOutCount(visualiserVertex, "$Is", "Wrap") == 1)
                return 0;

            return 1;
        }

        public static IEnumerable<IEdge> FilterEdges(IEnumerable<IEdge> toFilterEdges, IVertex visualiserVertex)
        {
            IList<IEdge> list = new List<IEdge>();

            int visualiserLevel = GetVisualiserLevel(visualiserVertex);

            foreach (IEdge e in toFilterEdges)
                if ( (visualiserLevel == 0 && FilterEdge_0(e))
                    || (visualiserLevel == 1 && FilterEdge_1(e)))
                    list.Add(e);

            return list;
            //return toFilterEdges;
        }

        public static bool FilterEdge(IEdge toFilterEdge, IVertex visualiserVertex)
        {
            int visualiserLevel = GetVisualiserLevel(visualiserVertex);

            if ((visualiserLevel == 0 && FilterEdge_0(toFilterEdge))
                    || (visualiserLevel == 1 && FilterEdge_1(toFilterEdge)))
                return true;

            return false;
        }

        public static bool FilterEdge_0(IEdge toFilterEdge)
        {            
            if (GraphUtil.ExistQueryOut(toFilterEdge.Meta, "$Hide", "0"))
                return false;

            return true;
        }

        public static bool FilterEdge_1(IEdge toFilterEdge)
        {
            if (GraphUtil.ExistQueryOut(toFilterEdge.Meta, "$Hide", "1"))
                return false;

            return true;
        }
    }
}
