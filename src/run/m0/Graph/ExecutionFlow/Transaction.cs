using m0.Foundation;
using m0.Util;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    // This ITransaction implementation supports GraphChangeTransactionAtom support
    public class Transaction : ITransaction
    {
        private static long nextDiagnosticId;

        internal long DiagnosticId { get; }

        internal bool IsAmbient { get; }

        private bool graphChangeWatchActive = true;

        public bool GraphChangeWatchActive
        {
            get { return graphChangeWatchActive; }
            set { graphChangeWatchActive = value; }
        }

        static IVertex r = m0.MinusZero.Instance.root;

        public static IVertex GenericEventHandler_event_meta;

        TransactionStateEnum state;
        public TransactionStateEnum State { get => state; }

        private readonly List<ITransactionAtom> atoms =
            new List<ITransactionAtom>();

        public Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_OutEdgeValueChange = new Dictionary<IVertex, List<GraphChangeTransactionAtom>>();
        public Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_InEdge = new Dictionary<IVertex, List<GraphChangeTransactionAtom>>();
        public Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_MetaEdge = new Dictionary<IVertex, List<GraphChangeTransactionAtom>>();

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
            GraphLifecycleLog.TransactionState(
                this,
                "started");
        }

        private void CommitAtoms()
        {
            foreach (ITransactionAtom a in atoms)
                a.Commit();
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

            foreach (KeyValuePair<IVertex, List<IVertex>> kvp in triggerEventDictionary)
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

        private void PrepareAndSendGrahChangeEvents_Loop(IExecution exe)
        {
            Dictionary<IVertex, List<WatcherEntry>> watchedVertexDictionary;

            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_OutEdgeValueChange_copy;
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_InEdge_copy;
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_MetaEdge_copy;

            while (graphChangeTransactionAtoms_OutEdgeValueChange.Count() > 0 ||
                graphChangeTransactionAtoms_InEdge.Count() > 0 ||
                graphChangeTransactionAtoms_MetaEdge.Count() > 0)
            {
                watchedVertexDictionary = GraphChangeTriggerWatcher.GetWatchedVertexDictionary();

                graphChangeTransactionAtoms_OutEdgeValueChange_copy =
                    new Dictionary<IVertex, List<GraphChangeTransactionAtom>>(graphChangeTransactionAtoms_OutEdgeValueChange);
                graphChangeTransactionAtoms_InEdge_copy =
                    new Dictionary<IVertex, List<GraphChangeTransactionAtom>>(graphChangeTransactionAtoms_InEdge);
                graphChangeTransactionAtoms_MetaEdge_copy =
                    new Dictionary<IVertex, List<GraphChangeTransactionAtom>>(graphChangeTransactionAtoms_MetaEdge);

                graphChangeTransactionAtoms_OutEdgeValueChange.Clear();
                graphChangeTransactionAtoms_InEdge.Clear();
                graphChangeTransactionAtoms_MetaEdge.Clear();

                PrepareAndSendGrahChangeEvents(exe, 
                    watchedVertexDictionary,
                    graphChangeTransactionAtoms_OutEdgeValueChange_copy,
                    graphChangeTransactionAtoms_InEdge_copy,
                    graphChangeTransactionAtoms_MetaEdge_copy);
            }
        }

        private void PrepareAndSendGrahChangeEvents(IExecution exe, 
            Dictionary<IVertex, List<WatcherEntry>> watchedVertexDictionary,
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_OutEdgeValueChange_copy,
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_InEdge_copy,
            Dictionary<IVertex, List<GraphChangeTransactionAtom>> graphChangeTransactionAtoms_MetaEdge_copy)
        {
            bool previousGraphChangeWatchActive =
                GraphChangeWatchActive;
            Dictionary<IVertex, List<IVertex>>
                triggerEventDictionary;

            GraphChangeWatchActive = false;

            try
            {
                int graphChangeTransactionAtoms_TotalCount =
                    graphChangeTransactionAtoms_OutEdgeValueChange_copy.Keys.Count +
                    graphChangeTransactionAtoms_InEdge_copy.Keys.Count;

                if (graphChangeTransactionAtoms_TotalCount >
                    watchedVertexDictionary.Count)
                {
                    triggerEventDictionary =
                        getTriggerEventDictionary_byWatchedVertexDictionary(
                            watchedVertexDictionary,
                            graphChangeTransactionAtoms_OutEdgeValueChange_copy,
                            graphChangeTransactionAtoms_InEdge_copy,
                            graphChangeTransactionAtoms_MetaEdge_copy);
                }
                else
                {
                    triggerEventDictionary =
                        getTriggerEventDictionary_byGraphChangeTransactionAtoms(
                            watchedVertexDictionary,
                            graphChangeTransactionAtoms_OutEdgeValueChange_copy,
                            graphChangeTransactionAtoms_InEdge_copy,
                            graphChangeTransactionAtoms_MetaEdge_copy);
                }
            }
            finally
            {
                GraphChangeWatchActive =
                    previousGraphChangeWatchActive;
            }

            try
            {
                SendGrahChangeEvents(
                    exe,
                    triggerEventDictionary);
            }
            finally
            {
                RemoveExternalReferences(
                    triggerEventDictionary);
            }
        }

        void RemoveExternalReferences(Dictionary<IVertex, List<IVertex>> triggerEventDictionary)
        {
            foreach (List<IVertex> eventList in triggerEventDictionary.Values)
                foreach (IVertex v in eventList)
                {
                    GraphLifecycleLog.EventExternalReference(
                        v,
                        "remove-begin");
                    v.RemoveExternalReference();
                    GraphLifecycleLog.EventExternalReference(
                        v,
                        "remove-end");
                }
        }

        public void Commit_SecondStage()
        {
            IList<ISecondStageCommitAction> secondStageCommitActionList_copy;
            int wave = 0;

            while(secondStageCommitActionList.Count() > 0)
            {
                wave++;
                secondStageCommitActionList_copy = secondStageCommitActionList.ToList();

                secondStageCommitActionList.Clear();

                GraphLifecycleLog.SecondStageWave(
                    this,
                    "begin",
                    wave,
                    secondStageCommitActionList_copy.Count);

                foreach (ISecondStageCommitAction a in secondStageCommitActionList_copy)
                    a.ExecuteSecondStageCommitAction();

                GraphLifecycleLog.SecondStageWave(
                    this,
                    "end",
                    wave,
                    secondStageCommitActionList_copy.Count);
            }
        }

        //static object lockObject = new object();

        public void Commit(IExecution exe)
        {
            //lock (lockObject)
            {
                if (state != TransactionStateEnum.Started)
                    throw new Exception("Transaction Commit while transaction not started.");

                GraphLifecycleLog.TransactionState(
                    this,
                    "commit-begin");
                state = TransactionStateEnum.Commiting;

                CommitAtoms();

                try
                {
                    PrepareAndSendGrahChangeEvents_Loop(exe);
                }
                finally
                {
                    Commit_SecondStage();
                }

                if (state == TransactionStateEnum.Commiting)
                {
                    state = TransactionStateEnum.Commited;
                    GraphLifecycleLog.TransactionState(
                        this,
                        "commit-end");

                    return;
                }

                if (state == TransactionStateEnum.Rolledback)
                {
                    return;
                }
            }
        }

        private void RollbackAtoms()
        {
            bool previousGraphChangeWatchActive =
                GraphChangeWatchActive;
            GraphChangeWatchActive = false;

            try
            {
                for (int index = atoms.Count - 1;
                    index >= 0;
                    index--)
                {
                    atoms[index].Rollback();
                }
            }
            finally
            {
                GraphChangeWatchActive =
                    previousGraphChangeWatchActive;
            }
        }

        public void Rollback(IExecution exe)
        {
            if (state != TransactionStateEnum.Started && state != TransactionStateEnum.Commiting)
                throw new Exception("Transaction Rollingback while not transaction started or not commiting");

            GraphLifecycleLog.TransactionState(
                this,
                "rollback-begin");

            if(state == TransactionStateEnum.Commiting)
                state = TransactionStateEnum.RollingbackWhileCommiting;
            else
                state = TransactionStateEnum.Rollingback;

            RollbackAtoms();

            state = TransactionStateEnum.Rolledback;
            GraphLifecycleLog.TransactionState(
                this,
                "rollback-end");
        }

        public Transaction(
            ITransaction prevTransaction,
            bool isAmbient = false)
        {
            DiagnosticId =
                Interlocked.Increment(
                    ref nextDiagnosticId);
            IsAmbient = isAmbient;
            previous = prevTransaction;

            state = TransactionStateEnum.NotStarted;
            GraphLifecycleLog.TransactionState(
                this,
                "created");
        }

        public void AddAtom(ITransactionAtom atom)
        {
            if (!GraphChangeWatchActive)
                return;

            atoms.Add(atom);

            GraphChangeTransactionAtom graphChangeAtom =
                atom as GraphChangeTransactionAtom;

            if (graphChangeAtom == null ||
                IsGraphChangeInfrastructureMutation(
                    graphChangeAtom))
            {
                return;
            }

            if (graphChangeAtom.ChangedVertex
                .HasOnlyNonTransactedRootVertexEventsEdge)
            {
                NonTransactedEvent.HandleOutEdgeValueChange(
                    graphChangeAtom);
            }
            else
            {
                AddOrCoalesceListenerAtom(
                    graphChangeTransactionAtoms_OutEdgeValueChange,
                    graphChangeAtom.ChangedVertex,
                    graphChangeAtom);
            }

            if (graphChangeAtom.Type !=
                    AtomGraphChangeTypeEnum.EdgeAdded &&
                graphChangeAtom.Type !=
                    AtomGraphChangeTypeEnum.EdgeRemoved)
            {
                return;
            }

            GraphChangeTransactionAtom inEdgeAtom =
                new GraphChangeTransactionAtom(
                    graphChangeAtom);
            inEdgeAtom.ChangedVertex =
                graphChangeAtom.Edge.To;

            if (inEdgeAtom.ChangedVertex
                .HasOnlyNonTransactedRootVertexEventsEdge)
            {
                NonTransactedEvent.HandleInEdge(
                    inEdgeAtom);
            }
            else
            {
                AddOrCoalesceListenerAtom(
                    graphChangeTransactionAtoms_InEdge,
                    inEdgeAtom.ChangedVertex,
                    inEdgeAtom);
            }

            GraphChangeTransactionAtom metaEdgeAtom =
                new GraphChangeTransactionAtom(
                    graphChangeAtom);
            metaEdgeAtom.ChangedVertex =
                graphChangeAtom.Edge.Meta;

            if (metaEdgeAtom.ChangedVertex
                .HasOnlyNonTransactedRootVertexEventsEdge)
            {
                NonTransactedEvent.HandleMetaEdge(
                    metaEdgeAtom);
            }
            else
            {
                AddOrCoalesceListenerAtom(
                    graphChangeTransactionAtoms_MetaEdge,
                    metaEdgeAtom.ChangedVertex,
                    metaEdgeAtom);
            }
        }

        private static bool IsGraphChangeInfrastructureMutation(
            GraphChangeTransactionAtom graphChangeAtom)
        {
            if (graphChangeAtom.Type ==
                    AtomGraphChangeTypeEnum.EdgeAdded ||
                graphChangeAtom.Type ==
                    AtomGraphChangeTypeEnum.EdgeRemoved)
            {
                if (GeneralUtil.CompareStrings(
                    graphChangeAtom.Edge.Meta,
                    "$GraphChangeTrigger"))
                {
                    return true;
                }

                return GraphUtil.ExistQueryIn(
                    graphChangeAtom.Edge.From,
                    "$GraphChangeTrigger",
                    null);
            }

            return graphChangeAtom.Type ==
                    AtomGraphChangeTypeEnum.ValueChange &&
                GraphUtil.ExistQueryIn(
                    graphChangeAtom.ChangedVertex,
                    "$GraphChangeTrigger",
                    null);
        }

        private static void AddOrCoalesceListenerAtom(
            Dictionary<IVertex,
                List<GraphChangeTransactionAtom>>
                listenerAtomsByVertex,
            IVertex changedVertex,
            GraphChangeTransactionAtom graphChangeAtom)
        {
            if (!listenerAtomsByVertex.TryGetValue(
                changedVertex,
                out List<GraphChangeTransactionAtom>
                    listenerAtoms))
            {
                listenerAtoms =
                    new List<GraphChangeTransactionAtom>();
                listenerAtomsByVertex.Add(
                    changedVertex,
                    listenerAtoms);
            }

            if (graphChangeAtom.Type ==
                AtomGraphChangeTypeEnum.ValueChange)
            {
                foreach (GraphChangeTransactionAtom
                    existingAtom in listenerAtoms)
                {
                    if (existingAtom.Type !=
                        AtomGraphChangeTypeEnum.ValueChange)
                    {
                        continue;
                    }

                    existingAtom.NewValue =
                        graphChangeAtom.NewValue;
                    return;
                }
            }
            else if (graphChangeAtom.Type ==
                    AtomGraphChangeTypeEnum.EdgeAdded ||
                graphChangeAtom.Type ==
                    AtomGraphChangeTypeEnum.EdgeRemoved)
            {
                for (int index =
                        listenerAtoms.Count - 1;
                    index >= 0;
                    index--)
                {
                    GraphChangeTransactionAtom
                        existingAtom =
                            listenerAtoms[index];

                    if (!ReferenceEquals(
                            existingAtom.Edge,
                            graphChangeAtom.Edge) ||
                        existingAtom.Type ==
                            graphChangeAtom.Type ||
                        (existingAtom.Type !=
                                AtomGraphChangeTypeEnum
                                    .EdgeAdded &&
                            existingAtom.Type !=
                                AtomGraphChangeTypeEnum
                                    .EdgeRemoved))
                    {
                        continue;
                    }

                    listenerAtoms.RemoveAt(index);

                    if (listenerAtoms.Count == 0)
                    {
                        listenerAtomsByVertex.Remove(
                            changedVertex);
                    }

                    return;
                }
            }

            listenerAtoms.Add(
                graphChangeAtom);
        }

        public void AddSecondStageCommitAction(ISecondStageCommitAction commitAction)
        {
            secondStageCommitActionList.Add(commitAction);
        }
    }
}
