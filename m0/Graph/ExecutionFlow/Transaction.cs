using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    public class Transaction : ITransaction
    {
        public TransactionStateEnum State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IList<ITransactionAtom> Atoms => throw new NotImplementedException();

        public ITransaction Previous => throw new NotImplementedException();

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
