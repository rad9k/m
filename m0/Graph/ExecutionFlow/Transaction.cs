using m0.Foundation;
using m0.Util;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
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
        static bool GraphChangeWatch = true;

        static IVertex r = m0.MinusZero.Instance.root;

        static IVertex GenericEventHandler_event_meta;

        TransactionStateEnum state;
        public TransactionStateEnum State { get => state; }

        IList<ITransactionAtom> atoms = new List<ITransactionAtom>();

        Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_OutEdgeValueChange = new Dictionary<IVertex, List<GraphChangeTransactionAtom>>();
        Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_InEdge = new Dictionary<IVertex, List<GraphChangeTransactionAtom>>();

        ITransaction previous;
        public ITransaction Previous { get => previous; }

        public static void Initialize()
        {
            GenericEventHandler_event_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GenericEventHandler\event");
        }

        public void Start()
        {
            state = TransactionStateEnum.Started;
        }

        private void CommitAtoms()
        {
            foreach (ITransactionAtom a in atoms)
                a.Commit();

            foreach (List<GraphChangeTransactionAtom> al in graphChangeTransactionAtoms_OutEdgeValueChange.Values)
                foreach (GraphChangeTransactionAtom a in al)
                    a.Commit();
        }

        private Dictionary<IVertex, List<IVertex>> getTriggerEventDictionary_byWatchedVertexDictionary(Dictionary<IVertex, List<WatcherEntry>> watchedVertexDictionary)
        {
            Dictionary<IVertex, List<IVertex>> triggerEventDictionary = new Dictionary<IVertex, List<IVertex>>();

            foreach(KeyValuePair<IVertex, List<WatcherEntry>> kvp in watchedVertexDictionary)
            {
                if (graphChangeTransactionAtoms_OutEdgeValueChange.ContainsKey(kvp.Key))
                    foreach (GraphChangeTransactionAtom a in graphChangeTransactionAtoms_OutEdgeValueChange[kvp.Key])
                        foreach (WatcherEntry we in kvp.Value)
                            GeneralUtil.DictionaryAdd<IVertex, IVertex>(triggerEventDictionary, we.triggerVertex, a.CreateEventVertex_GraphChange(we.triggerVertex, we.sourceVertex, false));

                if (graphChangeTransactionAtoms_OutEdgeValueChange.ContainsKey(kvp.Key))
                    foreach (GraphChangeTransactionAtom a in graphChangeTransactionAtoms_InEdge[kvp.Key])
                        foreach (WatcherEntry we in kvp.Value)
                            GeneralUtil.DictionaryAdd<IVertex, IVertex>(triggerEventDictionary, we.triggerVertex, a.CreateEventVertex_GraphChange(we.triggerVertex, we.sourceVertex, true));
            }
               
            return triggerEventDictionary;
        }

        private Dictionary<IVertex, List<IVertex>> getTriggerEventDictionary_byGraphChangeTransactionAtoms(Dictionary<IVertex, List<WatcherEntry>> watchedVertexDictionary)
        {
            Dictionary<IVertex, List<IVertex>> triggerEventDictionary = new Dictionary<IVertex, List<IVertex>>();

            foreach (KeyValuePair<IVertex, List<GraphChangeTransactionAtom>> kvp in graphChangeTransactionAtoms_OutEdgeValueChange)
                if (watchedVertexDictionary.ContainsKey(kvp.Key))
                    foreach (WatcherEntry we in watchedVertexDictionary[kvp.Key])
                        foreach (GraphChangeTransactionAtom a in kvp.Value)
                            GeneralUtil.DictionaryAdd<IVertex, IVertex>(triggerEventDictionary, we.triggerVertex, a.CreateEventVertex_GraphChange(we.triggerVertex, we.sourceVertex, false));

            foreach (KeyValuePair<IVertex, List<GraphChangeTransactionAtom>> kvp in graphChangeTransactionAtoms_InEdge)
                if (watchedVertexDictionary.ContainsKey(kvp.Key))
                    foreach (WatcherEntry we in watchedVertexDictionary[kvp.Key])
                        foreach (GraphChangeTransactionAtom a in kvp.Value)
                            GeneralUtil.DictionaryAdd<IVertex, IVertex>(triggerEventDictionary, we.triggerVertex, a.CreateEventVertex_GraphChange(we.triggerVertex, we.sourceVertex, true));

            return triggerEventDictionary;
        }

        private void SendGrahChangeEvents(IExecution exe, Dictionary<IVertex, List<IVertex>> triggerEventDictionary)
        {
            foreach(KeyValuePair<IVertex, List<IVertex>> kvp in triggerEventDictionary)
            {
                IVertex triggerVertex = kvp.Key;

                foreach(IEdge e in triggerVertex.GetAll(false, @"Listener:"))
                    foreach(IVertex eventVertex in kvp.Value)
                    {
                        IVertex parameters = InstructionHelpers.CreateStack();

                        parameters.AddEdge(GenericEventHandler_event_meta, eventVertex);

                        ZeroCodeExecutonUtil.FuncionCall(exe, e.To, parameters);   
                    }
            }
        }

        private void SendGrahChangeEvents(IExecution exe)
        {
            GraphChangeWatch = false;

            Dictionary<IVertex, List<IVertex>> triggerEventDictionary;

            Dictionary<IVertex, List<WatcherEntry>> watchedVertexDictionary = GraphChangeTriggerWatcher.GetWatchedVertexDictionary();

            int graphChangeTransactionAtoms_TotalCount =
                graphChangeTransactionAtoms_OutEdgeValueChange.Keys.Count +
                graphChangeTransactionAtoms_InEdge.Keys.Count;

            if (graphChangeTransactionAtoms_TotalCount < watchedVertexDictionary.Count)
                triggerEventDictionary = getTriggerEventDictionary_byWatchedVertexDictionary(watchedVertexDictionary);
            else
                triggerEventDictionary = getTriggerEventDictionary_byGraphChangeTransactionAtoms(watchedVertexDictionary);

            SendGrahChangeEvents(exe, triggerEventDictionary);

            GraphChangeWatch = true;
        }

        public void Commit(IExecution exe)
        {
            CommitAtoms();

            SendGrahChangeEvents(exe);

            state = TransactionStateEnum.Commited;
        }

        private void RollbackAtoms()
        {
            foreach (ITransactionAtom a in atoms)
                a.Rollback();

            foreach (List<GraphChangeTransactionAtom> al in graphChangeTransactionAtoms_OutEdgeValueChange.Values)
                foreach (GraphChangeTransactionAtom a in al)
                    a.Rollback();

            // no need to rollback graphChangeTransactionAtoms_InEdge
        }

        public void Rollback(IExecution exe)
        {
            RollbackAtoms();

            state = TransactionStateEnum.RolledBack;
        }

        public Transaction(ITransaction prevTransaction)
        {
            previous = prevTransaction;

            state = TransactionStateEnum.NotStarted;
        }

        public void AddAtom(ITransactionAtom atom)
        {
            if (atom is GraphChangeTransactionAtom && GraphChangeWatch) {
                GraphChangeTransactionAtom gcta = (GraphChangeTransactionAtom)atom;

                GeneralUtil.DictionaryAdd<IVertex, GraphChangeTransactionAtom>(graphChangeTransactionAtoms_OutEdgeValueChange, gcta.ChangedVertex, gcta);

                if(gcta.Type == GraphChangeEnum.EdgeAdded || gcta.Type == GraphChangeEnum.EdgeRemoved)
                    GeneralUtil.DictionaryAdd<IVertex, GraphChangeTransactionAtom>(graphChangeTransactionAtoms_InEdge, gcta.Edge.To, gcta);
            } else
                atoms.Add(atom);
        }
    }
}
