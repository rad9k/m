using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    public class TransacionAtom : ITransactionAtom
    {
        public virtual void Commit()
        {
            throw new NotImplementedException();
        }

        public virtual void Rollback()
        {
            throw new NotImplementedException();
        }

        public virtual IVertex CreateEventVertex(IVertex triggerVertex, IVertex sourceVertex)
        {
            throw new NotImplementedException();
        }
    }
}
