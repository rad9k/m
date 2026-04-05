using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    public interface IEdge
    {
        IVertex From { get; }
        IVertex Meta { get; }
        IVertex To { get; }
    }
}
