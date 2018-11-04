using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    // Store.Attach & Store.Detach does not have to be implemented through 
    // IDetachableEdge.Attach & IDetachableEdge.Detach, so that is why 
    // we are introducing IDetachableEdge – interface separate from IEdge
    public interface IDetachableEdge:IEdge
    {
        void Detach();
        
        void Attach();

        DetachStateEnum DetachState { get; }
    }
}
