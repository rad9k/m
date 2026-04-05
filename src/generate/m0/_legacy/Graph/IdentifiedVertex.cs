using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;

namespace m0.Graph
{
    public class IdentifiedVertex:EasyVertex
    {
        public IdentifiedVertex(string identifier,IStore store)
            : base(store)
        {
            _Identifier = identifier;

            UsageCounter++; // identified vertex are used for volatile stores
        }
    }
}
