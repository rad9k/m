using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    public enum GraphChangeEnum {ValueChange, OutputEdgeAdded, OutputEdgeRemoved, InputEdgeAdded, InputEdgeRemoved};

    public class TransacionAtom : ITransactionAtom
    {
        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}
