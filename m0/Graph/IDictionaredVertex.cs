using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph
{
    interface IDictionaredVertex:IVertex
    {
        IDictionary<object, object> OutEdgesByMeta { get; }

        IDictionary<object, object> OutEdgesByValue { get; }

        IDictionary<object, object> IngesByMeta { get; }

        IDictionary<object, object> InEdgesByValue { get; }
    }
}
