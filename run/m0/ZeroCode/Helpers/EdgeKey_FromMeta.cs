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
        public IEdge edge;

        public EdgeKey_FromMeta(IEdge _edge)
        {
            edge = _edge;
        }

        public override int GetHashCode()
        {
            if (edge.From!=null && edge.Meta!=null)
                return edge.From.GetHashCode() + edge.Meta.GetHashCode();

            if (edge.From != null)
                return edge.From.GetHashCode();

            if (edge.Meta != null)
                return -1 * edge.Meta.GetHashCode();

            return 0;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            EdgeKey_FromMeta other = (EdgeKey_FromMeta)obj;

            if (this.edge.From != other.edge.From)
                return false;

            if (this.edge.Meta != other.edge.Meta)
                return false;

            return true;
        }
    }
}
