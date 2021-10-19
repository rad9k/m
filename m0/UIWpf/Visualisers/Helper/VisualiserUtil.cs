using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.UIWpf.Visualisers.Helper
{
    public class VisualiserUtil
    {
        public static IEnumerable<IEdge> FilterEdges(IEnumerable<IEdge> toFilterEdges, IVertex visualiserVertex)
        {
            IList<IEdge> list = new List<IEdge>();

            foreach (IEdge e in toFilterEdges)
                if (FilterEdge(e, visualiserVertex))
                    list.Add(e);

            return list;
        }

        public static bool FilterEdge(IEdge toFilterEdge, IVertex visualiserVertex)
        {
            if (GraphUtil.ExistQueryOut(toFilterEdge.Meta, "$Hide", null))
                return false;

            return true;
        }
    }
}
