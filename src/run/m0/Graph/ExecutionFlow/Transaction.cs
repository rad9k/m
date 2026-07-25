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
        bool graphChangeWatch = true;

        public bool GraphChangeWatchActive
        {
            get { return graphChangeWatch; }
            set { graphChangeWatch = value; }
        }

        static IVertex r = m0.MinusZero.Instance.root;

        public static IVertex GenericEventHandler_event_meta;

        TransactionStateEnum state;
        public TransactionStateEnum State { get => state; }

        IList<ITransactionAtom> atoms = new List<ITransactionAtom>();
        IList<GraphChangeTransactionAtom> rollbackJournal =
            new List<GraphChangeTransactionAtom>();

        public Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_OutEdgeValueChange =
            new Dictionary<IVertex, List<GraphChangeTransactionAtom>>(
                ReferenceEqualityComparer.Instance);
        public Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_InEdge =
            new Dictionary<IVertex, List<GraphChangeTransactionAtom>>(
                ReferenceEqualityComparer.Instance);
        public Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_MetaEdge =
            new Dictionary<IVertex, List<GraphChangeTransactionAtom>>(
                ReferenceEqualityComparer.Instance);

        List<ISecondStageCommitAction> secondStageCommitActionList =
            new List<ISecondStageCommitAction>();
        readonly HashSet<ISecondStageCommitAction> queuedSecondStageCommitActions =
            new HashSet<ISecondStageCommitAction>(
                ReferenceEqualityComparer.Instance);
        int secondStageCommitActionsQueued;
        int duplicateSecondStageCommitActionsSuppressed;
        int secondStageCommitActionPeakQueueCount;

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
            long t0 = TxPerfLog.Timestamp();

            foreach (ITransactionAtom a in atoms)
                a.Commit();

            foreach (GraphChangeTransactionAtom atom in rollbackJournal)
                atom.Commit();

            TxPerfLog.Record("Transaction.CommitAtoms", TxPerfLog.Timestamp() - t0,
                atoms.Count + rollbackJournal.Count, "atoms");
        }

        bool IsFilterMatch_OutEdgeValueChange(WatcherEntry we, GraphChangeTransactionAtom ga)
        {
            if (we.graphChangeFilter == null || we.graphChangeFilter.Count() == 0)
                return true;

            switch (ga.Type)
            {
                case AtomGraphChangeTypeEnum.ValueChange:
                    if (we.graphChangeFilter.Contains(GraphChangeFilterEnum.ValueChange))
                        return true;
                    break;

                case AtomGraphChangeTypeEnum.EdgeAdded:
                    if (we.graphChangeFilter.Contains(GraphChangeFilterEnum.OutputEdgeAdded))
                        return true;
                    break;

                case AtomGraphChangeTypeEnum.EdgeRemoved:
                    if (we.graphChangeFilter.Contains(GraphChangeFilterEnum.OutputEdgeRemoved))
                        return true;
                    break;

                case AtomGraphChangeTypeEnum.OutputEdgeDisposed:
                    if (we.graphChangeFilter.Contains(GraphChangeFilterEnum.OutputEdgeDisposed))
                        return true;
                    break;
            }

            return false;
        }

        bool IsFilterMatch_InEdge(WatcherEntry we, GraphChangeTransactionAtom ga)
        {
            if (we.graphChangeFilter == null || we.graphChangeFilter.Count() == 0)
                return true;

            switch (ga.Type)
            {
                case AtomGraphChangeTypeEnum.EdgeAdded:
                    if (we.graphChangeFilter.Contains(GraphChangeFilterEnum.InputEdgeAdded))
                        return true;
                    break;

                case AtomGraphChangeTypeEnum.EdgeRemoved:
                    if (we.graphChangeFilter.Contains(GraphChangeFilterEnum.InputEdgeRemoved))
                        return true;
                    break;
            }

            return false;
        }

        bool IsFilterMatch_MetaEdge(WatcherEntry we, GraphChangeTransactionAtom ga)
        {
            if (we.graphChangeFilter == null || we.graphChangeFilter.Count() == 0)
                return true;

            switch (ga.Type)
            {
                case AtomGraphChangeTypeEnum.EdgeAdded:
                    if (we.graphChangeFilter.Contains(GraphChangeFilterEnum.MetaEdgeAdded))
                        return true;
                    break;

                case AtomGraphChangeTypeEnum.EdgeRemoved:
                    if (we.graphChangeFilter.Contains(GraphChangeFilterEnum.MetaEdgeRemoved))
                        return true;
                    break;
            }

            return false;
        }

        private Dictionary<IVertex, List<IVertex>> getTriggerEventDictionary_byWatchedVertexDictionary(
            Dictionary<IVertex, List<WatcherEntry>> watchedVertexDictionary,
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_OutEdgeValueChange_copy,
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_InEdge_copy,
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_MetaEdge_copy)
        {
            Dictionary<IVertex, List<IVertex>> triggerEventDictionary = new Dictionary<IVertex, List<IVertex>>();

            foreach(KeyValuePair<IVertex, List<WatcherEntry>> kvp in watchedVertexDictionary)
            {
                if (graphChangeTransactionAtoms_OutEdgeValueChange_copy.ContainsKey(kvp.Key))
                    foreach (GraphChangeTransactionAtom a in graphChangeTransactionAtoms_OutEdgeValueChange_copy[kvp.Key])
                        foreach (WatcherEntry we in kvp.Value)
                            if(IsFilterMatch_OutEdgeValueChange(we, a))
                            {                            
                                    IVertex eventVertex = a.CreateEventVertex_GraphChange(we.triggerVertex, we.sourceVertex, EdgeDirectionEnum.Out);
                                    if (eventVertex != null)
                                    {
                                        GeneralUtil.DictionaryAdd<IVertex, IVertex>(triggerEventDictionary, we.triggerVertex, eventVertex);
                                        eventVertex.AddExternalReference();
                                    }
                            }                          

                if (graphChangeTransactionAtoms_InEdge_copy.ContainsKey(kvp.Key))
                    foreach (GraphChangeTransactionAtom a in graphChangeTransactionAtoms_InEdge_copy[kvp.Key])
                        foreach (WatcherEntry we in kvp.Value)
                            if(IsFilterMatch_InEdge(we, a))
                                {
                                    IVertex eventVertex = a.CreateEventVertex_GraphChange(we.triggerVertex, we.sourceVertex, EdgeDirectionEnum.In);
                                    if (eventVertex != null)
                                    {
                                        GeneralUtil.DictionaryAdd<IVertex, IVertex>(triggerEventDictionary, we.triggerVertex, eventVertex);
                                        eventVertex.AddExternalReference();
                                    }
                                }

                if (graphChangeTransactionAtoms_MetaEdge_copy.ContainsKey(kvp.Key))
                    foreach (GraphChangeTransactionAtom a in graphChangeTransactionAtoms_MetaEdge_copy[kvp.Key])
                        foreach (WatcherEntry we in kvp.Value)
                            if (IsFilterMatch_MetaEdge(we, a))
                            {
                                IVertex eventVertex = a.CreateEventVertex_GraphChange(we.triggerVertex, we.sourceVertex, EdgeDirectionEnum.Meta);
                                if (eventVertex != null)
                                {
                                    GeneralUtil.DictionaryAdd<IVertex, IVertex>(triggerEventDictionary, we.triggerVertex, eventVertex);
                                    eventVertex.AddExternalReference();
                                }
                            }
            }
               
            return triggerEventDictionary;
        }

        private Dictionary<IVertex, List<IVertex>> getTriggerEventDictionary_byGraphChangeTransactionAtoms(
            Dictionary<IVertex, List<WatcherEntry>> watchedVertexDictionary,
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_OutEdgeValueChange_copy,
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_InEdge_copy,
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_MetaEdge_copy)
        {
            Dictionary<IVertex, List<IVertex>> triggerEventDictionary = new Dictionary<IVertex, List<IVertex>>();

            foreach (KeyValuePair<IVertex, List<GraphChangeTransactionAtom>> kvp in graphChangeTransactionAtoms_OutEdgeValueChange_copy)
                if (watchedVertexDictionary.ContainsKey(kvp.Key))
                    foreach (WatcherEntry we in watchedVertexDictionary[kvp.Key])
                        foreach (GraphChangeTransactionAtom a in kvp.Value)
                            if (IsFilterMatch_OutEdgeValueChange(we, a))
                            {
                                    IVertex eventVertex = a.CreateEventVertex_GraphChange(we.triggerVertex, we.sourceVertex, EdgeDirectionEnum.Out);
                                    if (eventVertex != null)
                                    {
                                        GeneralUtil.DictionaryAdd<IVertex, IVertex>(triggerEventDictionary, we.triggerVertex, eventVertex);
                                        eventVertex.AddExternalReference();
                                    }
                            }

            foreach (KeyValuePair<IVertex, List<GraphChangeTransactionAtom>> kvp in graphChangeTransactionAtoms_InEdge_copy)
                if (watchedVertexDictionary.ContainsKey(kvp.Key))
                    foreach (WatcherEntry we in watchedVertexDictionary[kvp.Key])
                        foreach (GraphChangeTransactionAtom a in kvp.Value)
                            if (IsFilterMatch_InEdge(we, a))
                                {
                                    IVertex eventVertex = a.CreateEventVertex_GraphChange(we.triggerVertex, we.sourceVertex, EdgeDirectionEnum.In);
                                    if (eventVertex != null)
                                    {
                                        GeneralUtil.DictionaryAdd<IVertex, IVertex>(triggerEventDictionary, we.triggerVertex, eventVertex);
                                        eventVertex.AddExternalReference();
                                    }
                                }

            foreach (KeyValuePair<IVertex, List<GraphChangeTransactionAtom>> kvp in graphChangeTransactionAtoms_MetaEdge_copy)
                if (watchedVertexDictionary.ContainsKey(kvp.Key))
                    foreach (WatcherEntry we in watchedVertexDictionary[kvp.Key])
                        foreach (GraphChangeTransactionAtom a in kvp.Value)
                            if (IsFilterMatch_MetaEdge(we, a))
                            {
                                IVertex eventVertex = a.CreateEventVertex_GraphChange(we.triggerVertex, we.sourceVertex, EdgeDirectionEnum.Meta);
                                if (eventVertex != null)
                                {
                                    GeneralUtil.DictionaryAdd<IVertex, IVertex>(triggerEventDictionary, we.triggerVertex, eventVertex);
                                    eventVertex.AddExternalReference();
                                }
                            }

            return triggerEventDictionary;
        }

        private void SendGrahChangeEvents_log(Dictionary<IVertex, List<IVertex>> triggerEventDictionary, bool fast)
        {
            m0.MinusZero.Instance.Log(2, "SendGrahChangeEvents", "START");
            

            foreach (KeyValuePair<IVertex, List<IVertex>> kvp in triggerEventDictionary)
            {
                IVertex triggerVertex = kvp.Key;
                            
                m0.MinusZero.Instance.Log(2, "SendGrahChangeEvents", "Trigger Vertex:"+ GraphUtil.GetVertexIdString(triggerVertex) + " events: " + kvp.Value.Count());

                if (fast)
                    continue;

                foreach (IEdge e in triggerVertex.GetAll(false, @"Listener:"))
                {
                    m0.MinusZero.Instance.Log(2, "SendGrahChangeEvents", "\tListener Vertex:" + GraphUtil.GetVertexIdString(e.To));

                    foreach (IVertex eventVertex in kvp.Value)
                    {
                        m0.MinusZero.Instance.Log(2, "SendGrahChangeEvents", "\t\tevent");

                        foreach (IEdge ee in eventVertex)
                        {
                            if(ee.Meta.Value.ToString() == "Edge")
                            {
                                m0.MinusZero.Instance.Log(2, "SendGrahChangeEvents", "\t\t\t"
                                    + ee.Meta.Value.ToString()
                                    + " :: " + ee.To.Value.ToString()
                                    + " // " + GraphUtil.GetVertexIdString(ee.To));

                                m0.MinusZero.Instance.Log(2, "SendGrahChangeEvents", "\t\t\t\t"
                                    + "From " + ee.To.Get(false, "From:"));
                                m0.MinusZero.Instance.Log(2, "SendGrahChangeEvents", "\t\t\t\t"
                                    + "Meta " + ee.To.Get(false, "Meta:"));
                                m0.MinusZero.Instance.Log(2, "SendGrahChangeEvents", "\t\t\t\t"
                                    + "To " + ee.To.Get(false, "To:"));
                            }
                            else
                                m0.MinusZero.Instance.Log(2, "SendGrahChangeEvents", "\t\t\t"
                                    + ee.Meta.Value.ToString()
                                    + " :: " + ee.To.Value.ToString()
                                    + " // " + GraphUtil.GetVertexIdString(ee.To));
                        }
                    }
                }
            }
        }

        private void SendGrahChangeEvents(IExecution exe, Dictionary<IVertex, List<IVertex>> triggerEventDictionary)
        {
            //SendGrahChangeEvents_log(triggerEventDictionary, true);

            long t0 = TxPerfLog.Timestamp();
            int listenersCalled = 0;
            int eventsDelivered = 0;
            int triggerVertices = triggerEventDictionary.Count;
            int listenersFound = 0;

            foreach (KeyValuePair<IVertex, List<IVertex>> kvp in triggerEventDictionary)
            {
                IVertex triggerVertex = kvp.Key;
                int eventCount = kvp.Value.Count;
                eventsDelivered += eventCount;

                foreach (IEdge e in triggerVertex.GetAll(false, @"Listener:"))
                {
                    listenersFound++;
                    IVertex parameters = InstructionHelpers.CreateStack();

                    foreach (IVertex eventVertex in kvp.Value)
                        parameters.AddEdge(GenericEventHandler_event_meta, eventVertex);

                    string listenerKey = GetListenerPerfKey(e.To);
                    long tListener = TxPerfLog.Timestamp();
                    ZeroCodeExecutonUtil.FuncionCall(exe, e.To, parameters);
                    TxPerfLog.Record("Transaction.Listener." + listenerKey,
                        TxPerfLog.Timestamp() - tListener, eventCount, "events");
                    listenersCalled++;
                }
            }

            TxPerfLog.Record("Transaction.SendGrahChangeEvents", TxPerfLog.Timestamp() - t0,
                listenersCalled, "listeners");
            TxPerfLog.CountWithExtra("Transaction.SendGrahChangeEvents.events", 1,
                eventsDelivered, "events");
            TxPerfLog.CountWithExtra("Transaction.SendGrahChangeEvents.triggers", 1,
                triggerVertices, "triggers");
            TxPerfLog.CountWithExtra("Transaction.SendGrahChangeEvents.listenersFound", 1,
                listenersFound, "listeners");
        }

        public static string GetListenerPerfKey(IVertex listenerVertex)
        {
            if (listenerVertex == null)
                return "null";

            IVertex typeNameVertex = GraphUtil.GetQueryOutFirst(listenerVertex, "DotNetTypeName", null);
            IVertex methodNameVertex = GraphUtil.GetQueryOutFirst(listenerVertex, "DotNetMethodName", null);

            string typeName = GraphUtil.GetStringValueOrNull(typeNameVertex);
            string methodName = GraphUtil.GetStringValueOrNull(methodNameVertex);

            if (!string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(methodName))
            {
                int lastDot = typeName.LastIndexOf('.');
                string shortType = lastDot >= 0 ? typeName.Substring(lastDot + 1) : typeName;
                return shortType + "." + methodName;
            }

            string listenerName = listenerVertex.Value == null ? null : listenerVertex.Value.ToString();
            if (!string.IsNullOrEmpty(listenerName))
                return "Named." + listenerName;

            return "Id." + GraphUtil.GetVertexIdString(listenerVertex);
        }

        private void PrepareAndSendGrahChangeEvents_Loop(IExecution exe)
        {
            long t0 = TxPerfLog.Timestamp();
            int loopIterations = 0;

            Dictionary<IVertex, List<WatcherEntry>> watchedVertexDictionary;

            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_OutEdgeValueChange_copy;
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_InEdge_copy;
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_MetaEdge_copy;

            while (graphChangeTransactionAtoms_OutEdgeValueChange.Count() > 0 ||
                graphChangeTransactionAtoms_InEdge.Count() > 0 ||
                graphChangeTransactionAtoms_MetaEdge.Count() > 0)
            {
                loopIterations++;

                long tWatch = TxPerfLog.Timestamp();
                watchedVertexDictionary = GraphChangeTriggerWatcher.GetWatchedVertexDictionary();
                TxPerfLog.Record("Transaction.GetWatchedVertexDictionary", TxPerfLog.Timestamp() - tWatch,
                    watchedVertexDictionary.Count, "watched");

                int outKeys = graphChangeTransactionAtoms_OutEdgeValueChange.Count;
                int inKeys = graphChangeTransactionAtoms_InEdge.Count;
                int metaKeys = graphChangeTransactionAtoms_MetaEdge.Count;

                graphChangeTransactionAtoms_OutEdgeValueChange_copy =
                    new Dictionary<IVertex, List<GraphChangeTransactionAtom>>(graphChangeTransactionAtoms_OutEdgeValueChange);
                graphChangeTransactionAtoms_InEdge_copy =
                    new Dictionary<IVertex, List<GraphChangeTransactionAtom>>(graphChangeTransactionAtoms_InEdge);
                graphChangeTransactionAtoms_MetaEdge_copy =
                    new Dictionary<IVertex, List<GraphChangeTransactionAtom>>(graphChangeTransactionAtoms_MetaEdge);

                graphChangeTransactionAtoms_OutEdgeValueChange.Clear();
                graphChangeTransactionAtoms_InEdge.Clear();
                graphChangeTransactionAtoms_MetaEdge.Clear();

                TxPerfLog.CountWithExtra("Transaction.ChangeKeys.Out", 1, outKeys, "keys");
                TxPerfLog.CountWithExtra("Transaction.ChangeKeys.In", 1, inKeys, "keys");
                TxPerfLog.CountWithExtra("Transaction.ChangeKeys.Meta", 1, metaKeys, "keys");
                TxPerfLog.CountWithExtra("Transaction.PrepareAndSendLoop.pendingKeys", 1,
                    outKeys + inKeys + metaKeys, "keys");

                PrepareAndSendGrahChangeEvents(exe, 
                    watchedVertexDictionary,
                    graphChangeTransactionAtoms_OutEdgeValueChange_copy,
                    graphChangeTransactionAtoms_InEdge_copy,
                    graphChangeTransactionAtoms_MetaEdge_copy);
            }

            TxPerfLog.Record("Transaction.PrepareAndSendLoop", TxPerfLog.Timestamp() - t0,
                loopIterations, "iterations");
        }

        private void PrepareAndSendGrahChangeEvents(IExecution exe, 
            Dictionary<IVertex, List<WatcherEntry>> watchedVertexDictionary,
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_OutEdgeValueChange_copy,
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_InEdge_copy,
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_MetaEdge_copy)
        {
            ExecutionFlowHelper.GraphChangeWatchOff();

            Dictionary<IVertex, List<IVertex>> triggerEventDictionary = null;
            long buildTicks = 0;
            bool usedWatchedPath = false;
            try
            {
                int graphChangeTransactionAtoms_TotalCount =
                    graphChangeTransactionAtoms_OutEdgeValueChange_copy.Keys.Count +
                    graphChangeTransactionAtoms_InEdge_copy.Keys.Count;

                long tBuild = TxPerfLog.Timestamp();
                if (graphChangeTransactionAtoms_TotalCount > watchedVertexDictionary.Count)
                {
                    usedWatchedPath = true;
                    triggerEventDictionary = getTriggerEventDictionary_byWatchedVertexDictionary(watchedVertexDictionary,
                        graphChangeTransactionAtoms_OutEdgeValueChange_copy,
                        graphChangeTransactionAtoms_InEdge_copy,
                        graphChangeTransactionAtoms_MetaEdge_copy);
                }
                else
                    triggerEventDictionary = getTriggerEventDictionary_byGraphChangeTransactionAtoms(watchedVertexDictionary,
                        graphChangeTransactionAtoms_OutEdgeValueChange_copy,
                        graphChangeTransactionAtoms_InEdge_copy,
                        graphChangeTransactionAtoms_MetaEdge_copy);
                buildTicks = TxPerfLog.Timestamp() - tBuild;
            }
            finally
            {
                ExecutionFlowHelper.GraphChangeWatchOn();
            }

            TxPerfLog.Record("Transaction.BuildTriggerEventDictionary", buildTicks,
                triggerEventDictionary == null ? 0 : triggerEventDictionary.Count, "triggers");
            TxPerfLog.CountWithExtra("Transaction.BuildTriggerEventDictionary.path", 1,
                usedWatchedPath ? 1 : 0, "byWatched");

            if (triggerEventDictionary == null)
                return;

            try
            {
                SendGrahChangeEvents(
                    exe,
                    triggerEventDictionary);
            }
            finally
            {
                long tCleanup = TxPerfLog.Timestamp();
                RemoveExternalReferences(
                    triggerEventDictionary);
                TxPerfLog.Record("Transaction.RemoveExternalReferences", TxPerfLog.Timestamp() - tCleanup);
            }
        }

        void RemoveExternalReferences(Dictionary<IVertex, List<IVertex>> triggerEventDictionary)
        {
            foreach (List<IVertex> eventList in triggerEventDictionary.Values)
                foreach (IVertex v in eventList)
                    v.RemoveExternalReference();
        }

        public void Commit_SecondStage()
        {
            long t0 = TxPerfLog.Timestamp();
            int actionsExecuted = 0;
            int processingRounds = 0;
            Dictionary<Type, (long TotalTicks, int ActionCount)> actionTypeStatistics =
                new Dictionary<Type, (long TotalTicks, int ActionCount)>();

            while (secondStageCommitActionList.Count > 0)
            {
                processingRounds++;
                List<ISecondStageCommitAction> actionsToExecute =
                    secondStageCommitActionList;
                secondStageCommitActionList =
                    new List<ISecondStageCommitAction>();

                foreach (ISecondStageCommitAction action in actionsToExecute)
                {
                    // Remove immediately before execution. This suppresses duplicate
                    // requests while an action is pending, but still permits an action
                    // to schedule itself again when its execution changes graph state.
                    queuedSecondStageCommitActions.Remove(action);

                    long tAction = TxPerfLog.Timestamp();
                    action.ExecuteSecondStageCommitAction();
                    long actionTicks = TxPerfLog.Timestamp() - tAction;

                    Type actionType = action.GetType();
                    if (actionTypeStatistics.TryGetValue(
                        actionType,
                        out (long TotalTicks, int ActionCount) statistics))
                    {
                        actionTypeStatistics[actionType] =
                            (statistics.TotalTicks + actionTicks,
                             statistics.ActionCount + 1);
                    }
                    else
                    {
                        actionTypeStatistics.Add(
                            actionType,
                            (actionTicks, 1));
                    }

                    actionsExecuted++;
                }
            }

            foreach (KeyValuePair<Type, (long TotalTicks, int ActionCount)> statistics
                in actionTypeStatistics)
            {
                TxPerfLog.Record(
                    "Transaction.SecondStage." + statistics.Key.Name + ".ExecuteBatch",
                    statistics.Value.TotalTicks,
                    statistics.Value.ActionCount,
                    "actions");
            }

            TxPerfLog.CountWithExtra(
                "Transaction.SecondStage.Queue",
                1,
                secondStageCommitActionsQueued,
                "queued");
            TxPerfLog.CountWithExtra(
                "Transaction.SecondStage.DuplicatesSuppressed",
                1,
                duplicateSecondStageCommitActionsSuppressed,
                "duplicates");
            TxPerfLog.CountWithExtra(
                "Transaction.SecondStage.PeakQueue",
                1,
                secondStageCommitActionPeakQueueCount,
                "actions");
            TxPerfLog.CountWithExtra(
                "Transaction.SecondStage.ProcessingRounds",
                1,
                processingRounds,
                "rounds");
            TxPerfLog.Record("Transaction.Commit_SecondStage", TxPerfLog.Timestamp() - t0,
                actionsExecuted, "actions");
        }

        //static object lockObject = new object();

        public void Commit(IExecution exe)
        {
            //lock (lockObject)
            {
                long t0 = TxPerfLog.Timestamp();

                if (state != TransactionStateEnum.Started)
                    throw new Exception("Transaction Commit while transaction not started.");

                state = TransactionStateEnum.Commiting;

                int pendingOut = graphChangeTransactionAtoms_OutEdgeValueChange.Count;
                int pendingIn = graphChangeTransactionAtoms_InEdge.Count;
                int pendingMeta = graphChangeTransactionAtoms_MetaEdge.Count;
                int pendingAtoms = atoms.Count + rollbackJournal.Count;

                CommitAtoms();

                PrepareAndSendGrahChangeEvents_Loop(exe);

                if (state == TransactionStateEnum.Commiting)
                {
                    Commit_SecondStage();

                    atoms.Clear();
                    rollbackJournal.Clear();
                    state = TransactionStateEnum.Commited;

                    TxPerfLog.Record("Transaction.Commit.total", TxPerfLog.Timestamp() - t0,
                        pendingOut + pendingIn + pendingMeta, "pendingChangeKeys");
                    TxPerfLog.CountWithExtra("Transaction.Commit.pendingAtoms", 1,
                        pendingAtoms, "atoms");
                    return;
                }

                if (state == TransactionStateEnum.Rolledback)
                {
                    TxPerfLog.Record("Transaction.Commit.total", TxPerfLog.Timestamp() - t0,
                        pendingOut + pendingIn + pendingMeta, "pendingChangeKeys");
                    return;
                }
            }
        }

        private void RollbackAtoms()
        {
            ExecutionFlowHelper.GraphChangeWatchOff();
            try
            {
                for (int index = atoms.Count - 1;
                     index >= 0;
                     index--)
                {
                    atoms[index].Rollback();
                }

                for (int index = rollbackJournal.Count - 1;
                     index >= 0;
                     index--)
                {
                    rollbackJournal[index].Rollback();
                }
            }
            finally
            {
                ExecutionFlowHelper.GraphChangeWatchOn();
            }

            atoms.Clear();
            rollbackJournal.Clear();
            graphChangeTransactionAtoms_OutEdgeValueChange.Clear();
            graphChangeTransactionAtoms_InEdge.Clear();
            graphChangeTransactionAtoms_MetaEdge.Clear();
            secondStageCommitActionList.Clear();
            queuedSecondStageCommitActions.Clear();
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

            if (previous != null)
                graphChangeWatch =
                    previous.GraphChangeWatchActive;

            state = TransactionStateEnum.NotStarted;
        }

        public void AddAtom(ITransactionAtom atom)
        {
            if (!(atom is GraphChangeTransactionAtom gcta))
            {
                atoms.Add(atom);
                return;
            }

            if (!GraphChangeWatchActive ||
                ShouldIgnoreGraphChangeAtom(gcta))
            {
                return;
            }

            if (ShouldRecordRollbackJournalAtom(gcta))
            {
                rollbackJournal.Add(gcta);
            }

            AddOutListenerChange(gcta);

            if (gcta.Type == AtomGraphChangeTypeEnum.EdgeAdded ||
                gcta.Type == AtomGraphChangeTypeEnum.EdgeRemoved)
            {
                if (gcta.Edge.To != null)
                {
                    var inEdgeAtom =
                        new GraphChangeTransactionAtom(gcta);
                    inEdgeAtom.ChangedVertex = gcta.Edge.To;
                    AddInListenerChange(inEdgeAtom);
                }

                if (gcta.Edge.Meta != null)
                {
                    var metaEdgeAtom =
                        new GraphChangeTransactionAtom(gcta);
                    AddMetaListenerChange(
                        gcta.Edge.Meta,
                        metaEdgeAtom);
                }
            }
        }

        private bool ShouldRecordRollbackJournalAtom(
            GraphChangeTransactionAtom atom)
        {
            if (previous != null)
                return true;

            if (!atom.ChangedVertex
                .HasOnlyNonTransactedRootVertexEventsEdge)
            {
                return true;
            }

            if (atom.Type != AtomGraphChangeTypeEnum.EdgeAdded &&
                atom.Type != AtomGraphChangeTypeEnum.EdgeRemoved)
            {
                return false;
            }

            if (atom.Edge.To != null &&
                !atom.Edge.To
                    .HasOnlyNonTransactedRootVertexEventsEdge)
            {
                return true;
            }

            return atom.Edge.Meta != null &&
                !atom.Edge.Meta
                    .HasOnlyNonTransactedRootVertexEventsEdge;
        }

        private bool ShouldIgnoreGraphChangeAtom(
            GraphChangeTransactionAtom atom)
        {
            if (atom.Type == AtomGraphChangeTypeEnum.EdgeAdded ||
                atom.Type == AtomGraphChangeTypeEnum.EdgeRemoved)
            {
                if (atom.Edge.Meta != null &&
                    GeneralUtil.CompareStrings(
                        atom.Edge.Meta,
                        "$GraphChangeTrigger"))
                {
                    return true;
                }

                if (atom.Edge.From != null &&
                    GraphUtil.ExistQueryIn(
                        atom.Edge.From,
                        "$GraphChangeTrigger",
                        null))
                {
                    return true;
                }
            }

            if (atom.Type == AtomGraphChangeTypeEnum.ValueChange &&
                GraphUtil.ExistQueryIn(
                    atom.ChangedVertex,
                    "$GraphChangeTrigger",
                    null))
            {
                return true;
            }

            return false;
        }

        private void AddOutListenerChange(
            GraphChangeTransactionAtom atom)
        {
            if (atom.ChangedVertex
                .HasOnlyNonTransactedRootVertexEventsEdge)
            {
                if (previous == null)
                    NonTransactedEvent
                        .HandleOutEdgeValueChange(atom);

                return;
            }

            AddCoalescedListenerChange(
                graphChangeTransactionAtoms_OutEdgeValueChange,
                atom.ChangedVertex,
                atom);
        }

        private void AddInListenerChange(
            GraphChangeTransactionAtom atom)
        {
            if (atom.ChangedVertex
                .HasOnlyNonTransactedRootVertexEventsEdge)
            {
                if (previous == null)
                    NonTransactedEvent.HandleInEdge(atom);

                return;
            }

            AddCoalescedListenerChange(
                graphChangeTransactionAtoms_InEdge,
                atom.ChangedVertex,
                atom);
        }

        private void AddMetaListenerChange(
            IVertex metaVertex,
            GraphChangeTransactionAtom atom)
        {
            if (metaVertex
                .HasOnlyNonTransactedRootVertexEventsEdge)
            {
                if (previous == null)
                    NonTransactedEvent.HandleMetaEdge(atom);

                return;
            }

            AddCoalescedListenerChange(
                graphChangeTransactionAtoms_MetaEdge,
                metaVertex,
                atom);
        }

        private static void AddCoalescedListenerChange(
            Dictionary<IVertex,
                List<GraphChangeTransactionAtom>> changeSet,
            IVertex changedVertex,
            GraphChangeTransactionAtom atom)
        {
            if (!changeSet.TryGetValue(
                changedVertex,
                out List<GraphChangeTransactionAtom> changes))
            {
                changes =
                    new List<GraphChangeTransactionAtom>();
                changeSet.Add(changedVertex, changes);
            }

            if (atom.Type == AtomGraphChangeTypeEnum.ValueChange)
            {
                for (int index = 0;
                     index < changes.Count;
                     index++)
                {
                    GraphChangeTransactionAtom existing =
                        changes[index];

                    if (existing.Type !=
                        AtomGraphChangeTypeEnum.ValueChange)
                    {
                        continue;
                    }

                    existing.NewValue = atom.NewValue;

                    if (object.Equals(
                        existing.OldValue,
                        existing.NewValue))
                    {
                        changes.RemoveAt(index);
                        RemoveEmptyChangeSetBucket(
                            changeSet,
                            changedVertex,
                            changes);
                    }

                    return;
                }

                if (object.Equals(
                    atom.OldValue,
                    atom.NewValue))
                {
                    RemoveEmptyChangeSetBucket(
                        changeSet,
                        changedVertex,
                        changes);
                    return;
                }
            }

            if (atom.Type == AtomGraphChangeTypeEnum.EdgeAdded ||
                atom.Type == AtomGraphChangeTypeEnum.EdgeRemoved)
            {
                for (int index = 0;
                     index < changes.Count;
                     index++)
                {
                    GraphChangeTransactionAtom existing =
                        changes[index];

                    if (!ReferenceEquals(
                        existing.Edge,
                        atom.Edge))
                    {
                        continue;
                    }


                    if (existing.Type == atom.Type)
                        return;

                    if ((existing.Type ==
                         AtomGraphChangeTypeEnum.EdgeAdded &&
                         atom.Type ==
                         AtomGraphChangeTypeEnum.EdgeRemoved) ||
                        (existing.Type ==
                         AtomGraphChangeTypeEnum.EdgeRemoved &&
                         atom.Type ==
                         AtomGraphChangeTypeEnum.EdgeAdded))
                    {
                        changes.RemoveAt(index);
                        RemoveEmptyChangeSetBucket(
                            changeSet,
                            changedVertex,
                            changes);
                        return;
                    }
                }
            }

            changes.Add(
                new GraphChangeTransactionAtom(atom));
        }

        private static void RemoveEmptyChangeSetBucket(
            Dictionary<IVertex,
                List<GraphChangeTransactionAtom>> changeSet,
            IVertex changedVertex,
            List<GraphChangeTransactionAtom> changes)
        {
            if (changes.Count == 0)
                changeSet.Remove(changedVertex);
        }

        public void AddSecondStageCommitAction(ISecondStageCommitAction commitAction)
        {
            if (!queuedSecondStageCommitActions.Add(commitAction))
            {
                duplicateSecondStageCommitActionsSuppressed++;
                return;
            }

            secondStageCommitActionList.Add(commitAction);
            secondStageCommitActionsQueued++;

            if (secondStageCommitActionList.Count >
                secondStageCommitActionPeakQueueCount)
            {
                secondStageCommitActionPeakQueueCount =
                    secondStageCommitActionList.Count;
            }
        }
    }
}
