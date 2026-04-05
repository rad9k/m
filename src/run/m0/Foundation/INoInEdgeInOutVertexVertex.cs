using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    public interface INoInEdgeInOutVertexVertex : IVertex
    {
        void AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(IEdge e);

        void AddEdgeForNoInEdgeInOutVertexVertex(IEdge e);
    }
}
