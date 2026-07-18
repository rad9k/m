using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode.Helpers
{
    public readonly struct EdgeKey_FromMeta
        : IEquatable<EdgeKey_FromMeta>
    {
        private readonly IVertex from;
        private readonly IVertex meta;

        public EdgeKey_FromMeta(IEdge _edge)
        {
            from = _edge.From;
            meta = _edge.Meta;
        }

        public override int GetHashCode()
        {
            if (from != null && meta != null)
                return from.GetHashCode() + meta.GetHashCode();

            if (from != null)
                return from.GetHashCode();

            if (meta != null)
                return -1 * meta.GetHashCode();

            return 0;
        }

        public override bool Equals(object obj)
        {
            return obj is EdgeKey_FromMeta other &&
                Equals(other);
        }

        public bool Equals(EdgeKey_FromMeta other)
        {
            return ReferenceEquals(from, other.from) &&
                ReferenceEquals(meta, other.meta);
        }
    }
}
