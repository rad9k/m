using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    public enum TransactionStateEnum { NotStarted, Started, Commiting, Commited, Rollingback, RollingbackWhileCommiting, Rolledback}

    public interface ITransaction
    {
        bool GraphChangeWatchActive {get; set;}

        TransactionStateEnum State { get; }

        void Start();

        void Commit(IExecution exe);

        void Rollback(IExecution exe);

        void AddAtom(ITransactionAtom atom);

        void AddSecondStageCommitAction(ISecondStageCommitAction commitAction);

        ITransaction Previous { get; }
    }
}
