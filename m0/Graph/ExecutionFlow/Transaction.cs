using m0.Foundation;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    public class Transaction : ITransaction
    {
        TransactionStateEnum state;
        public TransactionStateEnum State { get => state; }

        IList<ITransactionAtom> atoms = new List<ITransactionAtom>();

        Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms = new Dictionary<IVertex, List<GraphChangeTransactionAtom>>();

        ITransaction previous;
        public ITransaction Previous { get => previous; }

        public void Start()
        {
            state = TransactionStateEnum.Started;
        }

        public void Commit()
        {
            foreach (ITransactionAtom a in atoms)
                a.Commit();

            foreach (List<GraphChangeTransactionAtom> al in graphChangeTransactionAtoms.Keys)
                foreach (GraphChangeTransactionAtom a in al)
                    a.Commit();
        }

        public void Rollback()
        {
            foreach (ITransactionAtom a in atoms)
                a.Rollback();

            foreach (List<GraphChangeTransactionAtom> al in graphChangeTransactionAtoms.Keys)
                foreach (GraphChangeTransactionAtom a in al)
                    a.Rollback();
        }

        public Transaction(ITransaction prevTransaction)
        {
            previous = prevTransaction;

            state = TransactionStateEnum.NotStarted;
        }

        public void AddAtom(ITransactionAtom atom)
        {
            if (atom is GraphChangeTransactionAtom) {
                GraphChangeTransactionAtom gcta = (GraphChangeTransactionAtom)atom;
                GeneralUtil.DictionaryAdd<IVertex, GraphChangeTransactionAtom>(graphChangeTransactionAtoms, gcta.ChangedVertex, gcta);
            } else
                atoms.Add(atom);
        }
    }
}
