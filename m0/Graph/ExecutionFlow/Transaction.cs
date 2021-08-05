using m0.Foundation;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    // This ITransaction implementation supports GraphChangeTransactionAtom support
    public class Transaction : ITransaction
    {
        TransactionStateEnum state;
        public TransactionStateEnum State { get => state; }

        IList<ITransactionAtom> atoms = new List<ITransactionAtom>();

        Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_OutEdgeValueChange = new Dictionary<IVertex, List<GraphChangeTransactionAtom>>();
        Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_InEdge = new Dictionary<IVertex, List<GraphChangeTransactionAtom>>();

        ITransaction previous;
        public ITransaction Previous { get => previous; }

        public void Start()
        {
            state = TransactionStateEnum.Started;
        }

        private void CommitAtoms()
        {
            foreach (ITransactionAtom a in atoms)
                a.Commit();

            foreach (List<GraphChangeTransactionAtom> al in graphChangeTransactionAtoms_OutEdgeValueChange.Keys)
                foreach (GraphChangeTransactionAtom a in al)
                    a.Commit();
        }

        private Dictionary<IVertex, IVertex> getTriggerEventDictionary_byWatchedVertexDictionary(Dictionary<IVertex, List<WatcherEntry>> watchedVertexDictionary)
        {
            Dictionary<IVertex, IVertex> triggerEventDictionary = new Dictionary<IVertex, IVertex>();

            foreach(KeyValuePair<IVertex, List<WatcherEntry>> kvp in watchedVertexDictionary)
            {   
                if(graphChangeTransactionAtoms_OutEdgeValueChange.ContainsKey(kvp.Key))
                    foreach(GraphChangeTransactionAtom a in graphChangeTransactionAtoms_OutEdgeValueChange[kvp.Key])
                        foreach(WatcherEntry we in kvp.Value)
                            triggerEventDictionary.Add(we.triggerVertex, a.CreateEventVertex_GraphChange(we.sourceVertex, a.)
            }
               

            return triggerEventDictionary;
        }

        private Dictionary<IVertex, IVertex> getTriggerEventDictionary_byGraphChangeTransactionAtoms()
        {
            Dictionary<IVertex, IVertex> triggerEventDictionary = new Dictionary<IVertex, IVertex>();

            return triggerEventDictionary;
        }

        private void SendGrahChangeEvents(Dictionary<IVertex, IVertex> triggerEventDictionary)
        {

        }

        private void SendGrahChangeEvents()
        {
            Dictionary<IVertex, IVertex> triggerEventDictionary;

            Dictionary<IVertex, List<WatcherEntry>> watchedVertexDictionary = GraphChangeTriggerWatcher.GetWatchedVertexDictionary();

            int graphChangeTransactionAtoms_TotalCount =
                graphChangeTransactionAtoms_OutEdgeValueChange.Keys.Count +
                graphChangeTransactionAtoms_InEdge.Keys.Count;

            if (graphChangeTransactionAtoms_TotalCount < watchedVertexDictionary.Count)
                triggerEventDictionary = getTriggerEventDictionary_byWatchedVertexDictionary(watchedVertexDictionary);
            else
                triggerEventDictionary = getTriggerEventDictionary_byGraphChangeTransactionAtoms();

            SendGrahChangeEvents(triggerEventDictionary);
        }

        public void Commit()
        {
            CommitAtoms();

            SendGrahChangeEvents();
        }

        private void RollbackAtoms()
        {
            foreach (ITransactionAtom a in atoms)
                a.Rollback();

            foreach (List<GraphChangeTransactionAtom> al in graphChangeTransactionAtoms_OutEdgeValueChange.Keys)
                foreach (GraphChangeTransactionAtom a in al)
                    a.Rollback();

            // no need to rollback graphChangeTransactionAtoms_InEdge
        }

        public void Rollback()
        {
            RollbackAtoms();
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

                GeneralUtil.DictionaryAdd<IVertex, GraphChangeTransactionAtom>(graphChangeTransactionAtoms_OutEdgeValueChange, gcta.ChangedVertex, gcta);

                if(gcta.Type == GraphChangeEnum.EdgeAdded || gcta.Type == GraphChangeEnum.EdgeRemoved)
                    GeneralUtil.DictionaryAdd<IVertex, GraphChangeTransactionAtom>(graphChangeTransactionAtoms_InEdge, gcta.Edge.To, gcta);
            } else
                atoms.Add(atom);
        }
    }
}
