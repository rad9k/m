using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode.Helpers
{
    public class EdgeKey_FromMeta
    {
        IEdge edge;

        public EdgeKey_FromMeta(IEdge _edge)
        {
            edge = _edge;
        }

        public override int GetHashCode()
        {
            if(edge.From!=null && edge.Meta!=null)
                return edge.From.GetHashCode() + edge.To.GetHashCode();

            if (edge.From != null)
                return edge.From.GetHashCode();

            if (edge.Meta != null)
                return -1 * edge.To.GetHashCode();

            return 0;
        }
    }
}
