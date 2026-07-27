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
        static bool _GraphChangeWatch = true;

        public bool GraphChangeWatchActive { get { return _GraphChangeWatch; } set { _GraphChangeWatch = value; } }

        static IVertex r = m0.MinusZero.Instance.root;

        public static IVertex GenericEventHandler_event_meta;

        TransactionStateEnum state;
        public TransactionStateEnum State { get => state; }

        IList<ITransactionAtom> atoms = new List<ITransactionAtom>();

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
        }

        private void CommitAtoms()
        {
            foreach (ITransactionAtom a in atoms)
                a.Commit();

            foreach (List<GraphChangeTransactionAtom> al in graphChangeTransactionAtoms_OutEdgeValueChange.Values)
                foreach (GraphChangeTransactionAtom a in al)
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
            GraphChangeWatchActive = false;

            Dictionary<IVertex, List<IVertex>> triggerEventDictionary;            

            int graphChangeTransactionAtoms_TotalCount =
                graphChangeTransactionAtoms_OutEdgeValueChange_copy.Keys.Count +
                graphChangeTransactionAtoms_InEdge_copy.Keys.Count;

            if (graphChangeTransactionAtoms_TotalCount > watchedVertexDictionary.Count)
                triggerEventDictionary = getTriggerEventDictionary_byWatchedVertexDictionary(watchedVertexDictionary,
                    graphChangeTransactionAtoms_OutEdgeValueChange_copy,
                    graphChangeTransactionAtoms_InEdge_copy,
                    graphChangeTransactionAtoms_MetaEdge_copy);
            else
                triggerEventDictionary = getTriggerEventDictionary_byGraphChangeTransactionAtoms(watchedVertexDictionary,
                    graphChangeTransactionAtoms_OutEdgeValueChange_copy,
                    graphChangeTransactionAtoms_InEdge_copy,
                    graphChangeTransactionAtoms_MetaEdge_copy);

            GraphChangeWatchActive = true;

            SendGrahChangeEvents(exe, triggerEventDictionary);

            //

            RemoveExternalReferences(triggerEventDictionary);
        }

        void RemoveExternalReferences(Dictionary<IVertex, List<IVertex>> triggerEventDictionary)
        {
            foreach (List<IVertex> eventList in triggerEventDictionary.Values)
                foreach (IVertex v in eventList)
                    v.RemoveExternalReference();
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

        //static object lockObject = new object();

        public void Commit(IExecution exe)
        {
            //lock (lockObject)
            {
                if (state != TransactionStateEnum.Started)
                    throw new Exception("Transaction Commit while transaction not started.");

                state = TransactionStateEnum.Commiting;

                CommitAtoms();

                PrepareAndSendGrahChangeEvents_Loop(exe);

                if (state == TransactionStateEnum.Commiting)
                {
                    Commit_SecondStage();

                    state = TransactionStateEnum.Commited;

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
            GraphChangeWatchActive = false;

            foreach (ITransactionAtom a in atoms)
                a.Rollback();

            foreach (List<GraphChangeTransactionAtom> al in graphChangeTransactionAtoms_OutEdgeValueChange.Values)
                foreach (GraphChangeTransactionAtom a in al)
                    a.Rollback();

            // no need to rollback graphChangeTransactionAtoms_InEdge

            GraphChangeWatchActive = true;
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
            if (GraphChangeWatchActive)
            {
                GraphChangeTransactionAtom gcta = (GraphChangeTransactionAtom)atom;

                if (gcta.Type == AtomGraphChangeTypeEnum.EdgeAdded || gcta.Type == AtomGraphChangeTypeEnum.EdgeRemoved)
                {
                    if (GeneralUtil.CompareStrings(gcta.Edge.Meta, "$GraphChangeTrigger"))
                        return;

                    if (GraphUtil.ExistQueryIn(gcta.Edge.From, "$GraphChangeTrigger", null))
                        return;
                }

                if (gcta.Type == AtomGraphChangeTypeEnum.ValueChange)
                    if (GraphUtil.ExistQueryIn(gcta.ChangedVertex, "$GraphChangeTrigger", null))
                        return;

                if (gcta.ChangedVertex.HasOnlyNonTransactedRootVertexEventsEdge)
                    NonTransactedEvent.HandleOutEdgeValueChange(gcta);
                else
                    GeneralUtil.DictionaryAdd<IVertex, GraphChangeTransactionAtom>(
                        graphChangeTransactionAtoms_OutEdgeValueChange,
                        gcta.ChangedVertex,
                        gcta);

                if (gcta.Type == AtomGraphChangeTypeEnum.EdgeAdded || gcta.Type == AtomGraphChangeTypeEnum.EdgeRemoved)
                {                    
                    GraphChangeTransactionAtom gcta_inEdge = new GraphChangeTransactionAtom(gcta);
                    gcta_inEdge.ChangedVertex = gcta.Edge.To;

                    if (gcta_inEdge.ChangedVertex.HasOnlyNonTransactedRootVertexEventsEdge)
                        NonTransactedEvent.HandleInEdge(gcta_inEdge);
                    else
                        GeneralUtil.DictionaryAdd<IVertex, GraphChangeTransactionAtom>(
                            graphChangeTransactionAtoms_InEdge,
                            gcta_inEdge.ChangedVertex,
                            gcta_inEdge);

                    //

                    GraphChangeTransactionAtom gcta_metaEdge = new GraphChangeTransactionAtom(gcta);
                    //gcta_inEdge.ChangedVertex = gcta.Edge.To;

                    if (gcta.Edge.Meta.HasOnlyNonTransactedRootVertexEventsEdge)
                        NonTransactedEvent.HandleMetaEdge(gcta);
                    else
                        GeneralUtil.DictionaryAdd<IVertex, GraphChangeTransactionAtom>(
                            graphChangeTransactionAtoms_MetaEdge,
                            gcta.Edge.Meta, // is this ok???
                            gcta_metaEdge);
                }
            }
        }

        public void AddSecondStageCommitAction(ISecondStageCommitAction commitAction)
        {
            secondStageCommitActionList.Add(commitAction);
        }
    }
}
