using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    public enum DetachStateEnum
    {
        Attached,
        Attaching,
        InDetaching,
        InDetached,
        Detached,
        Detaching
    }

    public enum AccessLevelEnum 
    {
        NoRestrictions, 
        VertexExecute,
        VertexValueRead, 
        VertexValueWrite,
        InEdgesList,
        OutEdgesList,
        OutEdgeRead,
        OutEdgeDelete,
        OutEdgeAdd
     }
    
}
