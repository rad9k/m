using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    public interface ITransactionRoot
    {
        IVertex Root { get; }

        void Refresh();

        void StartTransaction();

        void RollbackTransaction();

        void CommitTransaction();
    }
}
