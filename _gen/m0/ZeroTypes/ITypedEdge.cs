using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes
{
    public interface ITypedEdge
    {
        IEdge Edge { get; set; }

        IVertex Vertex { get; }
    }
}
