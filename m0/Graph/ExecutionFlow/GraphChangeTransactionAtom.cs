using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    public class GraphChangeTransactionAtom : TransacionAtom
    {
        public IVertex ChangedVertex;
        private GraphChangeEnum Type;
        private object OldValue;
        private object NewValue;
        private IEdge Edge;
        
        public GraphChangeTransactionAtom(
            IVertex _ChangedVertex,
            GraphChangeEnum _Type,
            object _OldValue,
            object _NewValue,
            IEdge _Edge)
        {
            ChangedVertex = _ChangedVertex;
            Type = _Type;
            OldValue = _OldValue;
            NewValue = _NewValue;
            Edge = _Edge;
        }
    }
}
