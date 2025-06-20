using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode.Helpers
{
    public class EdgeKey_MetaTo
    {
        public IEdge edge;

        public EdgeKey_MetaTo(IEdge _edge)
        {
            edge = _edge;
        }

        public override int GetHashCode()
        {
            if (edge.To != null && edge.Meta != null)
                return edge.To.GetHashCode() + edge.Meta.GetHashCode();

            if (edge.To != null)
                return edge.To.GetHashCode();

            if (edge.Meta != null)
                return -1 * edge.Meta.GetHashCode();

            return 0;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            EdgeKey_MetaTo other = (EdgeKey_MetaTo)obj;

            if (this.edge.To != other.edge.To)
                return false;

            if (this.edge.Meta != other.edge.Meta)
                return false;

            return true;
        }
    }
}
