using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes
{
    public class TypedVertex
    {
        public IVertex Vertex { get; set; }

        public TypedVertex(IVertex vertex)
        {
            Vertex = vertex;
        }
    }
}
