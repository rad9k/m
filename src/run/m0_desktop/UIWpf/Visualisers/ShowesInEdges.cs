using System;
using System.Collections.Generic;
using System.Text;
using m0.Foundation;
using m0.ZeroTypes;

namespace m0.UIWpf.Visualisers
{
    public class ShowesInEdges
    {
        public static IEnumerable<IEdge> FilterFromToSourceChangedVertex(IEnumerable<IEdge> set)
        {
            List<IEdge> newList = new List<IEdge>();

            foreach (IEdge e in set)
            {
                string meta = e.Meta.Value.ToString();

                if (meta == "From" || meta == "To" || meta == "Source" || meta == "ChangedVertex")
                    continue;

                newList.Add(e);
            }

            return newList;
        }
    }
}
