using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    public enum TransactionStateEnum { NotStarted, Started, Commited, RolledBack}

    public interface ITransaction
    {
        TransactionStateEnum State { get; }

        void Start();

        void Commit(IExecution exe);

        void Rollback(IExecution exe);

        void AddAtom(ITransactionAtom atom);

        ITransaction Previous { get; }
    }
}
