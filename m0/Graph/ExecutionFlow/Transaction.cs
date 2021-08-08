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

        IList<ISecondStageCommitAction> secondStageCommitActionList = new List<ISecondStageCommitAction>();

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

                foreach (IEdge e in triggerVertex.GetAll(false, @"Listener:"))
                {
                    IVertex parameters = InstructionHelpers.CreateStack();

                    foreach (IVertex eventVertex in kvp.Value)
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

        public void Commit_SecondStage()
        {
            IList<ISecondStageCommitAction> secondStageCommitActionList_copy;

            while(secondStageCommitActionList.Count() > 0)
            {
                secondStageCommitActionList_copy = secondStageCommitActionList.ToList();

                secondStageCommitActionList.Clear();

                foreach (ISecondStageCommitAction a in secondStageCommitActionList_copy)
                    a.ExecuteSecondStageCommitAction();
            }
        }

        public void Commit(IExecution exe)
        {
            if (state != TransactionStateEnum.Started)
                throw new Exception("Transaction Commit while transaction not started.");

            state = TransactionStateEnum.Commiting;

            CommitAtoms();

            SendGrahChangeEvents(exe);

            if (state == TransactionStateEnum.Commiting)
            {
                Commit_SecondStage();

                state = TransactionStateEnum.Commited;

                return;
            }

            if(state == TransactionStateEnum.Rolledback)
            {
                return;
            }
        }

        private void RollbackAtoms()
        {
            GraphChangeWatch = false;

            foreach (ITransactionAtom a in atoms)
                a.Rollback();

            foreach (List<GraphChangeTransactionAtom> al in graphChangeTransactionAtoms_OutEdgeValueChange.Values)
                foreach (GraphChangeTransactionAtom a in al)
                    a.Rollback();

            // no need to rollback graphChangeTransactionAtoms_InEdge

            GraphChangeWatch = true;
        }

        public void Rollback(IExecution exe)
        {
            if (state != TransactionStateEnum.Started && state != TransactionStateEnum.Commiting)
                throw new Exception("Transaction Rollingback while not transaction started or not commiting");

            if(state == TransactionStateEnum.Commiting)
                state = TransactionStateEnum.RollingbackWhileCommiting;
            else
                state = TransactionStateEnum.Rollingback;

            RollbackAtoms();

            state = TransactionStateEnum.Rolledback;
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

                GeneralUtil.DictionaryAdd<IVertex, GraphChangeTransactionAtom>(
                    graphChangeTransactionAtoms_OutEdgeValueChange, 
                    gcta.ChangedVertex, 
                    gcta);

                if (gcta.Type == GraphChangeEnum.EdgeAdded || gcta.Type == GraphChangeEnum.EdgeRemoved)
                {
                    GraphChangeTransactionAtom gcta_inEdge = new GraphChangeTransactionAtom(gcta);
                    gcta_inEdge.ChangedVertex = gcta.Edge.To;

                    GeneralUtil.DictionaryAdd<IVertex, GraphChangeTransactionAtom>(
                        graphChangeTransactionAtoms_InEdge, 
                        gcta_inEdge.ChangedVertex, 
                        gcta_inEdge);
                }
            } else
                atoms.Add(atom);
        }

        public void AddSecondStageCommitAction(ISecondStageCommitAction commitAction)
        {
            secondStageCommitActionList.Add(commitAction);
        }
    }
}
