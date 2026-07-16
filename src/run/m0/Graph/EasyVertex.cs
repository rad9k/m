using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0.Foundation;
using m0.Store;
using m0;
using m0.Util;
using m0.ZeroCode;
using System.Runtime.InteropServices;
using m0.ZeroCode.Helpers;
using m0.DotNetIntegration;
using m0.Graph.Internal;
using m0.Graph.ExecutionFlow;
using static m0.Graph.GraphUtil;
using m0.ZeroTypes;

namespace m0.Graph
{
    internal enum VertexIdentifierRegistrationMode
    {
        Registered,
        Ephemeral
    }

    [Serializable]
    public class EasyVertex: VertexBase, IDisposable, IImplementedVertex, ISecondStageCommitAction
    {
        private static readonly string[] emptyMetaQueryKeys = new string[] { "" };

        [NonSerialized]
        private string[] metaQueryKeys;

        private Dictionary<string, object> outEdgesByQueryMeta;

        protected bool CanEmitGraphChangeEvents = true;

        protected EdgeDictionaries edgeDictionaries;

        public object _Identifier;
        
        public override object Identifier { get { return _Identifier; }}


        protected object _Value;

        public override object Value {
            get{
                return _Value;
            }
            set{
                object oldValue = _Value;

                if (value == null)
                    return;                

                _Value = value;

                ValueChanged();

                //FireChange(new VertexChangeEventArgs(VertexChangeType.ValueChanged, null));

                if (_Value.ToString() == "piesek")
                {
                    int x = 0;
                }

                if (CanEmitGraphChangeEvents)
                    ExecutionFlowHelper.AddTransactionAtom(new GraphChangeTransactionAtom(
                        this,
                        AtomGraphChangeTypeEnum.ValueChange,
                        oldValue,
                        _Value,
                        null));

                GraphUtil.Debug(this, DebugOperationEnum.Value);
            }
        }

        protected void ValueChanged()
        {
            if (InEdgesRaw.Count == 1)
            {
                IVertex sourceVertex = InEdgesRaw[0].From;

                if (sourceVertex != null)
                    InvalidateOutValueIndexes(sourceVertex);
            }
            else if (InEdgesRaw.Count > 1)
            {
                HashSet<IVertex> affectedSourceVertices = new HashSet<IVertex>();

                foreach (IEdge e in InEdgesRaw)
                    if (e.From != null) // there could be artificial edge, with From==null
                        affectedSourceVertices.Add(e.From);

                foreach (IVertex sourceVertex in affectedSourceVertices)
                    InvalidateOutValueIndexes(sourceVertex);
            }

            foreach (IEdge e in OutEdgesRaw)
                e.To.InEdgesDictionariesNeedsRebuild = true;

            metaQueryKeys = null;

            if (MetaInEdgesRaw.Count > 0 || InheritsInEdges.Count > 0)
                InvalidateMetaQueryIndexesForThisAndInheritChildren(true);
        }

        private static void InvalidateOutValueIndexes(IVertex sourceVertex)
        {
            MarkOutValueIndexesNeedRebuild(sourceVertex);

            if (sourceVertex is EasyVertex easySourceVertex &&
                easySourceVertex.InheritsInEdges.Count == 0)
                return;

            HashSet<IVertex> inheritChildren = VertexHelper.GetInheritChilds(sourceVertex);
            GraphPerformanceCounters.RecordInvalidatedInheritChildren(inheritChildren.Count);

            foreach (IVertex inheritChild in inheritChildren)
                MarkOutValueIndexesNeedRebuild(inheritChild);
        }

        private static void MarkOutValueIndexesNeedRebuild(IVertex vertex)
        {
            if (vertex is EasyVertex easyVertex)
            {
                easyVertex.OutEdgesDictionariesNeedsRebuild_Value = true;
                easyVertex.OutEdgesDictionariesNeedsRebuild_MetaAndValue = true;
                return;
            }

            vertex.OutEdgesDictionariesNeedsRebuild = true;
        }

        public bool HasInheritance { get; set; }

        public bool AllowInheritance = true;

        // InEdgesRaw
        // from == who inherits from me
        // meta == $Inherits
        // to == this

        public IList<IEdge> InheritsInEdges;

        // OutEdgesRaw
        // from == this
        // meta == $Inherits
        // to == who I inherit from

        public IList<IEdge> InheritsOutEdges;

        public override IList<IEdge> InEdgesRaw { get { return edgeDictionaries.In; } }

        public override IList<IEdge> OutEdgesRaw { get { return edgeDictionaries.Out; } }

        protected IList<IEdge> _OutEdges;

        public override IList<IEdge> OutEdges
        {
            get
            {
                if (OutEdgesDictionariesNeedsRebuild_Edges)
                {
                    OutEdgesDictionariesRebuild_Edges();                    
                    return _OutEdges;
                }
                else
                    return _OutEdges;                
            }
        }

        public override IList<IEdge> MetaInEdgesRaw { get { return edgeDictionaries.MetaIn; } }

        protected virtual void OutEdgesDictionariesRebuild_Edges()
        {
            if (HasInheritance && AllowInheritance)
            {
                List<IEdge> FullEdges = OutEdgesRaw.ToList();

                HashSet<IVertex> parents = VertexHelper.GetInheritParents(this);

                foreach (IVertex v in parents)
                    GraphUtil.AddRange_NoNoInherit(FullEdges, v.OutEdgesRaw);                    

                _OutEdges = FullEdges;
            }
            else
                _OutEdges = OutEdgesRaw;

            GraphPerformanceCounters.RecordOutEdgesRebuild(
                OutEdgesRebuildKind.LogicalEdges,
                HasInheritance && AllowInheritance ? _OutEdges.Count : 0);
            OutEdgesDictionariesNeedsRebuild_Edges = false;
        }

        private void InEdgesDictionariesRebuild_Meta()
        {
            IList<IEdge> inEdges = InEdgesRaw;
            Dictionary<string, object> edgesByMeta =
                new Dictionary<string, object>(inEdges.Count);

            _InEdgesByMeta = edgesByMeta;

            foreach (IEdge edge in inEdges)
                AddEdgeToDictionary(
                    edgesByMeta,
                    GetQueryDictionaryKey(edge.Meta?.Value),
                    edge);

            InEdgesDictionariesNeedsRebuild_Meta = false;
        }

        private void OutEdgesDictionariesRebuild_Meta()
        {
            IList<IEdge> outEdges = OutEdges;
            Dictionary<object, object> directEdgesByMeta =
                new Dictionary<object, object>(outEdges.Count);

            _OutEdgesByMeta = directEdgesByMeta;

            foreach (IEdge edge in outEdges)
                AddEdgeToDictionary(
                    directEdgesByMeta,
                    GetQueryDictionaryKey(edge.Meta?.Value),
                    edge);

            GraphPerformanceCounters.RecordOutEdgesRebuild(
                OutEdgesRebuildKind.DirectMeta,
                outEdges.Count);
            OutEdgesDictionariesNeedsRebuild_Meta = false;
        }

        private void OutEdgesDictionariesRebuild_QueryMeta()
        {
            IList<IEdge> outEdges = OutEdges;
            Dictionary<string, object> queryEdgesByMeta =
                new Dictionary<string, object>(outEdges.Count);

            outEdgesByQueryMeta = queryEdgesByMeta;

            foreach (IEdge edge in outEdges)
                foreach (string queryMetaKey in GetMetaQueryKeys(edge.Meta))
                    AddEdgeToDictionary(queryEdgesByMeta, queryMetaKey, edge);

            GraphPerformanceCounters.RecordOutEdgesRebuild(
                OutEdgesRebuildKind.QueryMeta,
                outEdges.Count);
            OutEdgesDictionariesNeedsRebuild_QueryMeta = false;
        }

        private void InEdgesDictionariesRebuild_Value()
        {
            IList<IEdge> inEdges = InEdgesRaw;
            Dictionary<string, object> edgesByValue =
                new Dictionary<string, object>(inEdges.Count);

            _InEdgesByValue = edgesByValue;

            foreach (IEdge edge in inEdges)
                AddEdgeToDictionary(
                    edgesByValue,
                    GetQueryDictionaryKey(edge.From?.Value),
                    edge);

            InEdgesDictionariesNeedsRebuild_Value = false;
        }

        private void OutEdgesDictionariesRebuild_Value()
        {
            IList<IEdge> outEdges = OutEdges;
            Dictionary<string, object> edgesByValue =
                new Dictionary<string, object>(outEdges.Count);

            _OutEdgesByValue = edgesByValue;

            foreach (IEdge edge in outEdges)
                AddEdgeToDictionary(
                    edgesByValue,
                    GetQueryDictionaryKey(edge.To?.Value),
                    edge);

            GraphPerformanceCounters.RecordOutEdgesRebuild(
                OutEdgesRebuildKind.Value,
                outEdges.Count);
            OutEdgesDictionariesNeedsRebuild_Value = false;
        }

        private void InEdgesDictionariesRebuild_MetaAndValue()
        {
            IList<IEdge> inEdges = InEdgesRaw;
            Dictionary<GraphUtil.MetaAndValueKey, object> edgesByMetaAndValue =
                new Dictionary<GraphUtil.MetaAndValueKey, object>(inEdges.Count);

            _InEdgesByMetaAndValue = edgesByMetaAndValue;

            foreach (IEdge edge in inEdges)
                AddEdgeToDictionary(
                    edgesByMetaAndValue,
                    new GraphUtil.MetaAndValueKey(edge.Meta?.Value, edge.From?.Value),
                    edge);

            InEdgesDictionariesNeedsRebuild_MetaAndValue = false;
        }

        private void OutEdgesDictionariesRebuild_MetaAndValue()
        {
            IList<IEdge> outEdges = OutEdges;
            Dictionary<GraphUtil.MetaAndValueKey, object> queryEdgesByMetaAndValue =
                new Dictionary<GraphUtil.MetaAndValueKey, object>(outEdges.Count);

            _OutEdgesByMetaAndValue = queryEdgesByMetaAndValue;

            foreach (IEdge edge in outEdges)
                foreach (string queryMetaKey in GetMetaQueryKeys(edge.Meta))
                    AddEdgeToDictionary(
                        queryEdgesByMetaAndValue,
                        new GraphUtil.MetaAndValueKey(queryMetaKey, edge.To?.Value),
                        edge);

            GraphPerformanceCounters.RecordOutEdgesRebuild(
                OutEdgesRebuildKind.QueryMetaAndValue,
                outEdges.Count);
            OutEdgesDictionariesNeedsRebuild_MetaAndValue = false;
        }

        private static string GetQueryDictionaryKey(object value)
        {
            return value as string ?? value?.ToString() ?? "";
        }

        private static void AddEdgeToDictionary<TKey>(
            Dictionary<TKey, object> dictionary,
            TKey key,
            IEdge edge) where TKey : notnull
        {
            bool exists;
            ref object dictionaryValue = ref CollectionsMarshal.GetValueRefOrAddDefault(
                dictionary,
                key,
                out exists);

            if (!exists)
            {
                dictionaryValue = edge;
                return;
            }

            if (dictionaryValue is List_VertexBase list)
            {
                list.Add(edge);
                return;
            }

            List_VertexBase newList = new List_VertexBase
            {
                (IEdge)dictionaryValue,
                edge
            };

            dictionaryValue = newList;
        }

        private static string[] GetMetaQueryKeys(IVertex metaVertex)
        {
            if (metaVertex == null)
                return emptyMetaQueryKeys;

            if (metaVertex is EasyVertex easyMetaVertex)
            {
                if (easyMetaVertex.metaQueryKeys == null)
                    easyMetaVertex.metaQueryKeys = CreateMetaQueryKeys(metaVertex);

                return easyMetaVertex.metaQueryKeys;
            }

            return CreateMetaQueryKeys(metaVertex);
        }

        private static string[] CreateMetaQueryKeys(IVertex metaVertex)
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
            keys.Add(GetQueryDictionaryKey(metaVertex.Value));

            foreach (IVertex parent in VertexHelper.GetInheritParents(metaVertex))
                keys.Add(GetQueryDictionaryKey(parent.Value));

            return keys.ToArray();
        }

        private void InvalidateMetaQueryIndexesForThisAndInheritChildren(
            bool directMetaValueChanged)
        {
            HashSet<IVertex> affectedMetaVertices = VertexHelper.GetInheritChilds(this);
            affectedMetaVertices.Add(this);

            HashSet<IVertex> affectedSourceVertices = new HashSet<IVertex>();
            HashSet<IVertex> directMetaSourceVertices = directMetaValueChanged
                ? new HashSet<IVertex>()
                : null;

            foreach (IVertex metaVertex in affectedMetaVertices)
            {
                if (metaVertex is EasyVertex easyMetaVertex)
                    easyMetaVertex.metaQueryKeys = null;

                foreach (IEdge metaInEdge in metaVertex.MetaInEdgesRaw)
                    if (metaInEdge.From != null)
                    {
                        affectedSourceVertices.Add(metaInEdge.From);

                        if (directMetaValueChanged && metaVertex == this)
                        {
                            directMetaSourceVertices.Add(metaInEdge.From);

                            if (metaInEdge.To != null)
                                metaInEdge.To.InEdgesDictionariesNeedsRebuild = true;
                        }
                    }
            }

            AddInheritChildren(affectedSourceVertices);

            foreach (IVertex sourceVertex in affectedSourceVertices)
                if (sourceVertex is EasyVertex easySourceVertex)
                    easySourceVertex.MarkMetaQueryIndexesNeedRebuild(false);
                else
                    sourceVertex.OutEdgesDictionariesNeedsRebuild = true;

            if (directMetaSourceVertices != null)
            {
                AddInheritChildren(directMetaSourceVertices);

                foreach (IVertex sourceVertex in directMetaSourceVertices)
                    if (sourceVertex is EasyVertex easySourceVertex)
                        easySourceVertex.MarkMetaQueryIndexesNeedRebuild(true);
                    else
                        sourceVertex.OutEdgesDictionariesNeedsRebuild = true;
            }
        }

        private static void AddInheritChildren(HashSet<IVertex> sourceVertices)
        {
            IVertex[] originalSourceVertices = sourceVertices.ToArray();

            foreach (IVertex sourceVertex in originalSourceVertices)
                foreach (IVertex inheritChild in VertexHelper.GetInheritChilds(sourceVertex))
                    sourceVertices.Add(inheritChild);
        }

        private void MarkMetaQueryIndexesNeedRebuild(bool directMeta)
        {
            OutEdgesDictionariesNeedsRebuild_QueryMeta = true;
            OutEdgesDictionariesNeedsRebuild_MetaAndValue = true;

            if (directMeta)
                OutEdgesDictionariesNeedsRebuild_Meta = true;
        }

        // edge = new Edge in Attached state
        // OutEdgesRaw.OnAdd
        //    edge.Meta.MetaInEdgesRaw.Add(edge);                                
        //    edge.To.InEdgesRaw.Add(edge);
        // this.AttachEdge(edge)
        // edge.To.AttachInEdge(edge)

        public override IEdge AddEdge(IVertex metaVertex, IVertex destVertex)
        {
           /* if (
                metaVertex != null && GeneralUtil.CompareStrings(metaVertex, "If") 
                //&& (String)destVertex.Value == "Arrow"
                )
            {
                int x = 0;
            }*/

            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");

            if (destVertex == null)
                destVertex = MinusZero.Instance.Empty; // can be    

            if (destVertex.DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");

            ValidateInheritanceEdge(metaVertex, destVertex);

            EdgeBase ne = new EasyEdge(this, metaVertex, destVertex);

            OutEdgesRaw.Add(ne);

            AttachEdge(ne);
            destVertex.AttachInEdge(ne);

            if (CanEmitGraphChangeEvents)
                ExecutionFlowHelper.AddTransactionAtom(new GraphChangeTransactionAtom(
                    this,
                    AtomGraphChangeTypeEnum.EdgeAdded,
                    null,
                    null,
                    ne));

            return ne;
        }

        private void ValidateInheritanceEdge(IVertex metaVertex, IVertex parentVertex)
        {
            if (metaVertex == null ||
                !GeneralUtil.CompareStrings(metaVertex.Value, "$Inherits"))
                return;

            if (ReferenceEquals(this, parentVertex))
                throw new InvalidOperationException(
                    "A vertex cannot inherit from itself.");

            if (VertexHelper.GetInheritParents(parentVertex).Contains(this))
                throw new InvalidOperationException(
                    "The $Inherits edge would create an inheritance cycle.");
        }

        public override void AttachInEdge(IEdge edge)
        {
            if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
                InheritsInEdges.Add(edge);
        }

        public override void AttachEdge(IEdge edge)
        {            
            if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
            {
                InheritsOutEdges.Add(edge);                

                HasInheritance = true;

                InvalidateMetaQueryIndexesForThisAndInheritChildren(false);
            }

            if (GeneralUtil.CompareStrings(edge.Meta.Value, "$GraphChangeTrigger"))
            {
                HasOnlyNonTransactedRootVertexEventsEdgeNeedsRebuild = true;
                GraphChangeTriggerWatcher.AddGraphChangeTrigger(edge);
            }

            if (GeneralUtil.CompareStrings(edge.To.Value, "OnlyNonTransactedRootVertexEvents") ||
                GeneralUtil.CompareStrings(edge.Meta.Value, "Listener"))
                OnlyNonTransactedRootVertexEvents_Listener_AddedRemoved();
        }

        public override void DetachInEdge(IEdge edge)
        {
            if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
                InheritsInEdges.Remove(edge);
        }

        public override void DetachEdge(IEdge edge)
        {
            if (edge.Meta != null)
            {
                if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
                {
                    InheritsOutEdges.Remove(edge);                    

                    if (InheritsOutEdges.Count == 0)
                        HasInheritance = false;

                    InvalidateMetaQueryIndexesForThisAndInheritChildren(false);
                }

                if (GeneralUtil.CompareStrings(edge.Meta.Value, "$GraphChangeTrigger")) {
                    HasOnlyNonTransactedRootVertexEventsEdgeNeedsRebuild = true;
                    GraphChangeTriggerWatcher.RemoveGraphChangeTrigger(edge);
                }

                if (GeneralUtil.CompareStrings(edge.To.Value, "OnlyNonTransactedRootVertexEvents") ||
                    GeneralUtil.CompareStrings(edge.Meta.Value, "Listener"))
                    OnlyNonTransactedRootVertexEvents_Listener_AddedRemoved();
            }
        }

        protected void OnlyNonTransactedRootVertexEvents_Listener_AddedRemoved()
        {
            foreach (IEdge e in GraphUtil.GetQueryIn(this, "$GraphChangeTrigger", null))
                if (e.From is EasyVertex)
                {
                    EasyVertex ev = (EasyVertex)e.From;

                    ev.HasOnlyNonTransactedRootVertexEventsEdgeNeedsRebuild = true;
                }
        }

        public override void AddEdgesList(IEnumerable<IEdge> edges)
        {            
            foreach (IEdge e in edges) // possibly not optimal implementation
                AddEdge(e.Meta, e.To);
        }

        // OutEdgesRaw.OnRemove
        //      edge.Meta.MetaInEdgesRaw.Remove(edge);
        //      edge.To.InEdgesRaw.Remove(edge);
        //          edge.To.CheckIfShouldDispose();
        //      edge.From.DetachEdge(item);
        public override void DeleteEdge(IEdge _edge)
        {
            if (_edge == null)
                return;

            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");            

            IEdge edge = edgeDictionaries.Out.Get(_edge);

            if (edge == null)
                foreach (IEdge e in OutEdges)
                    if (e.From == _edge.From && e.Meta == _edge.Meta && e.To == _edge.To)
                        edge = e;

            if (edge != null)
            {                
                OutEdgesRaw.Remove(edge);

                if(CanEmitGraphChangeEvents)
                    ExecutionFlowHelper.AddTransactionAtom(new GraphChangeTransactionAtom(
                        this,
                        AtomGraphChangeTypeEnum.EdgeRemoved,
                        null,
                        null,
                        edge));
            }
        }

        public override void DeleteEdgesList(IEnumerable<IEdge> edges)
        {            
            foreach (IEdge e in edges) // possibly not optimal implementation
                DeleteEdge(e); // Meta/To check to be performed
        }       

        private const int DefaultQueryParseCacheCapacity = 512;
        private const int DefaultMetaQueryParseCacheCapacity = 128;

        private static readonly object queryParseCacheSynchronizationRoot =
            new object();
        private static readonly BoundedQueryParseCache queryParseCache =
            new BoundedQueryParseCache(
                DefaultQueryParseCacheCapacity,
                queryParseCacheSynchronizationRoot);
        private static readonly BoundedQueryParseCache metaQueryParseCache =
            new BoundedQueryParseCache(
                DefaultMetaQueryParseCacheCapacity,
                queryParseCacheSynchronizationRoot);

        public static int QueryParseCacheCapacity
        {
            get { return queryParseCache.Capacity; }
            set { queryParseCache.Capacity = value; }
        }

        public static int MetaQueryParseCacheCapacity
        {
            get { return metaQueryParseCache.Capacity; }
            set { metaQueryParseCache.Capacity = value; }
        }

        public static int QueryParseCacheEntryCount
        {
            get { return queryParseCache.State.Count; }
        }

        public static int MetaQueryParseCacheEntryCount
        {
            get { return metaQueryParseCache.State.Count; }
        }

        public static void ResetQueryParseCaches()
        {
            lock (queryParseCacheSynchronizationRoot)
            {
                queryParseCache.Clear();
                metaQueryParseCache.Clear();
            }
        }

        public override void Dispose()
        {
            if (DisposedState != DisposeStateEnum.Live)
                return;

            DisposedState = DisposeStateEnum.Disposing;

            GraphUtil.Debug(this, DebugOperationEnum.Dispose);

            DeleteAllInEdges();
            DeleteAllMetaInEdges();
            DeleteAllEdges();

            Store.RemoveVertexIdentifier(this);

            DisposedState = DisposeStateEnum.Disposed;
        }

        public void DeleteAllInEdges()
        {
            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");

            foreach (IEdge edge in InEdgesRaw.ToList())
            {
                InEdgesRaw.Remove(edge);                

                if (CanEmitGraphChangeEvents)
                    ExecutionFlowHelper.AddTransactionAtom(new GraphChangeTransactionAtom(
                        edge.From,
                        AtomGraphChangeTypeEnum.OutputEdgeDisposed,
                        null,
                        null,
                        edge));
            }
        }

        public void DeleteAllMetaInEdges()
        {
            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");

            foreach (IEdge edge in MetaInEdgesRaw.ToList())
                MetaInEdgesRaw.Remove(edge);
        }

        public void DeleteAllEdges()
        {
            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");

            foreach (IEdge edge in OutEdgesRaw.ToList()) {             
                OutEdgesRaw.Remove(edge);                

                //FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, edge)); // moved from before edge.Meta.DeleteMetaInEdge(edge); XXX !!!
            }            
        }

        public void InheritChildsOutEdgesDictionariesNeedsRebuild()
        {
            HashSet<IVertex> inheritsSet = VertexHelper.GetInheritChilds(this);

            GraphPerformanceCounters.RecordInvalidatedInheritChildren(inheritsSet.Count);

            foreach (IVertex v in inheritsSet)
                v.OutEdgesDictionariesNeedsRebuild = true;
        }

        public IDictionary<object, object> GetOutOdgesByMeta()
        {
            if (OutEdgesDictionariesNeedsRebuild_Meta)
                OutEdgesDictionariesRebuild_Meta();

            return OutEdgesByMeta;
        }

        public override void QueryOutEdges(object meta, object to, out IEdge result, out IList<IEdge> results)
         {
            result = null;
            results = null;

            if (meta!=null && to == null)
            {
                if (OutEdgesDictionariesNeedsRebuild_QueryMeta || outEdgesByQueryMeta == null)
                    OutEdgesDictionariesRebuild_QueryMeta();

                string metaKey = GetQueryDictionaryKey(meta);
                object val;

                if (!outEdgesByQueryMeta.TryGetValue(metaKey, out val))
                    return;

                if (val is List_VertexBase list)
                    results = list;
                else
                    result = (IEdge)val;

                return;
            }

            if (meta == null && to != null)
            {
                if (OutEdgesDictionariesNeedsRebuild_Value)
                    OutEdgesDictionariesRebuild_Value();

                string toKey = GetQueryDictionaryKey(to);
                object val;

                if (!OutEdgesByValue.TryGetValue(toKey, out val))
                    return;

                if (val is List_VertexBase list)
                    results = list;
                else
                    result = (IEdge)val;

                return;
            }

            if (meta != null && to != null)
            {
               if (OutEdgesDictionariesNeedsRebuild_MetaAndValue)
                    OutEdgesDictionariesRebuild_MetaAndValue();

                GraphUtil.MetaAndValueKey searchKey =
                    new GraphUtil.MetaAndValueKey(meta, to);
                object val;

                if (!OutEdgesByMetaAndValue.TryGetValue(searchKey, out val))
                    return;

                if (val is List_VertexBase list)
                    results = list;
                else
                    result = (IEdge)val;

                return;
            }

            results = OutEdges.ToList();
        }

        public override void QueryInEdges(object meta, object from, out IEdge result, out IList<IEdge> results)
        {
            result = null;
            results = null;

            if (meta != null && from == null)
            {
                if (InEdgesDictionariesNeedsRebuild_Meta)
                    InEdgesDictionariesRebuild_Meta();

                string metaKey = GetQueryDictionaryKey(meta);
                object val;

                if (!InEdgesByMeta.TryGetValue(metaKey, out val))
                    return;

                if (val is List_VertexBase list)
                    results = list;
                else
                    result = (IEdge)val;

                return;
            }

            if (meta == null && from != null)
            {
                if (InEdgesDictionariesNeedsRebuild_Value)
                    InEdgesDictionariesRebuild_Value();

                string fromKey = GetQueryDictionaryKey(from);
                object val;

                if (!InEdgesByValue.TryGetValue(fromKey, out val))
                    return;

                if (val is List_VertexBase list)
                    results = list;
                else
                    result = (IEdge)val;

                return;
            }

            if (meta != null && from != null)
            {
                if (InEdgesDictionariesNeedsRebuild_MetaAndValue)
                    InEdgesDictionariesRebuild_MetaAndValue();

                GraphUtil.MetaAndValueKey searchKey =
                    new GraphUtil.MetaAndValueKey(meta, from);
                object val;

                if (!InEdgesByMetaAndValue.TryGetValue(searchKey, out val))
                    return;

                if (val is List_VertexBase list)
                    results = list;
                else
                    result = (IEdge)val;

                return;
            }

            results = InEdgesRaw.ToList();
        }

        public override IVertex Get(bool metaMode, string query)
        {
            QueryParseCacheLease queryLease =
                GetParsedQuery(
                    metaMode,
                    query,
                    out bool parseFailed);

            try
            {
                if (parseFailed)
                    return null;

                return MinusZero.Instance.DefaultExecuter.Get(
                    metaMode,
                    this,
                    queryLease.Vertex);
            }
            finally
            {
                queryLease.Dispose();
            }
        }

        public override IVertex GetAll(bool metaMode, string query)
        {
            QueryParseCacheLease queryLease =
                GetParsedQuery(
                    metaMode,
                    query,
                    out bool parseFailed);

            try
            {
                if (parseFailed)
                    return null;

                return MinusZero.Instance.DefaultExecuter.GetAll(
                    metaMode,
                    this,
                    queryLease.Vertex);
            }
            finally
            {
                queryLease.Dispose();
            }
        }

        private static QueryParseCacheLease GetParsedQuery(
            bool metaMode,
            string query,
            out bool parseFailed)
        {
            BoundedQueryParseCache cache = metaMode
                ? metaQueryParseCache
                : queryParseCache;
            bool factoryParseFailed = false;

            QueryParseCacheLease queryLease =
                cache.GetOrCreate(
                    query,
                    () =>
                    {
                        IVertex queryVertex =
                            MinusZero.Instance.CreateTempVertex();

                        try
                        {
                            IEdge baseEdge;
                            IVertex parseError =
                                MinusZero.Instance.DefaultFormalTextParser.Parse(
                                    new EdgeBase(
                                        null,
                                        null,
                                        queryVertex),
                                    query,
                                    CodeRepresentationEnum.VertexAndManyLines,
                                    out baseEdge);

                            factoryParseFailed =
                                parseError != null &&
                                parseError.Count() > 0;

                            return new QueryParseCacheValue(
                                queryVertex,
                                !factoryParseFailed);
                        }
                        catch
                        {
                            queryVertex.Dispose();
                            throw;
                        }
                    });

            GraphPerformanceCounters.RecordQueryParseCacheLookup(
                queryLease.Hit,
                QueryParseCacheEntryCount +
                MetaQueryParseCacheEntryCount);

            parseFailed = factoryParseFailed;
            return queryLease;
        }
        
        public override IVertex Get(bool metaMode, IVertex expression)
        {
            return MinusZero.Instance.DefaultExecuter.Get(metaMode, this, expression);
        }

        public override IVertex GetAll(bool metaMode, IVertex expression)
        {
            return MinusZero.Instance.DefaultExecuter.GetAll(metaMode, this, expression);
        }

        public override INoInEdgeInOutVertexVertex Execute(IExecution exe)
        {
            return ExecutionFlowHelper.Execute(this, exe);
        }

        protected void VertexInit_First()
        {
            edgeDictionaries = new EdgeDictionaries(this);

            InheritsInEdges = new List<IEdge>();
            InheritsOutEdges = new List<IEdge>();

            InEdgesDictionariesNeedsRebuild = true;
            OutEdgesDictionariesNeedsRebuild = true;

            bool tempCanEmitGraphChangeEvents = CanEmitGraphChangeEvents;

            CanEmitGraphChangeEvents = false;

            Value = "";

            CanEmitGraphChangeEvents = tempCanEmitGraphChangeEvents;
        }

        static object lock_object = new object();        
        protected virtual void VertexInit()
        {
            VertexInit(
                VertexIdentifierRegistrationMode.Registered);
        }

        private protected void VertexInit(
            VertexIdentifierRegistrationMode registrationMode)
        {
            lock (lock_object)
            {
                VertexInit_First();

                _Identifier = Store.VertexIdentifierCount++;

                //Store.VertexIdentifierCount += RND.Next(10) + 1;

                //_Identifier = Store.VertexIdentifierCount;

                GraphUtil.Debug(this, DebugOperationEnum.Init);

                if (registrationMode ==
                    VertexIdentifierRegistrationMode.Registered)
                {
                    Store.StoreVertexIdentifier(this);
                }
            }
        }

        public override void ExecuteSecondStageCommitAction()
        {
            if (ShouldDispose())
            {
                Dispose();
            }
        }

        public EasyVertex(IStore _store) : base(_store)
        {
            VertexInit();
        }

        private protected EasyVertex(
            IStore _store,
            VertexIdentifierRegistrationMode registrationMode)
            : base(_store)
        {
            VertexInit(registrationMode);
        }

        public EasyVertex(IStore _store, object toBeIdentifier) : base(_store)
        {
            DisposedState = DisposeStateEnum.Live;

            _Identifier = toBeIdentifier;

            VertexInit_First();            

            if (toBeIdentifier is int)
            {
                int val = (int)toBeIdentifier + 1;

                if (val > Store.VertexIdentifierCount)
                    Store.VertexIdentifierCount = val;
            }

            Store.StoreVertexIdentifier(this);
        }

        bool ShouldDispose()
        {
            if (DisposedState != DisposeStateEnum.Live)
                return false;

            int cumulativeEdgesCount = 0;

            cumulativeEdgesCount += edgeDictionaries.In.Count;
            cumulativeEdgesCount += edgeDictionaries.MetaIn.Count;

            if (cumulativeEdgesCount == 0 && ExternalReferenceCount == 0
                && Store.DetachState == DetachStateEnum.Attached
                && !IsRoot)          
                return true;            

            return false;
        }

        public override void CheckIfShouldDispose()
        {
            if(ShouldDispose())
                ExecutionFlowHelper.AddSecondStageCommitAction(edgeDictionaries.Vertex);
        }

        public void ClearDictionaries()
        {
            this.InEdgesDictionariesNeedsRebuild = true;
            
            this.OutEdgesDictionariesNeedsRebuild = true;
            
            //   InEdgesRaw.Clear();
            // OutEdgesRaw.Clear();
            //MetaInEdgesRaw.Clear();


            _OutEdgesByMeta = null;
            outEdgesByQueryMeta = null;
            metaQueryKeys = null;
            _OutEdgesByValue = null;
            _OutEdgesByMetaAndValue = null;
            _InEdgesByMeta = null;
            _InEdgesByValue = null;
            _InEdgesByMetaAndValue = null;
        }
    }
}
