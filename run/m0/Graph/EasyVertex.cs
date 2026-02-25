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
    [Serializable]
    public class EasyVertex: VertexBase, IDisposable, IImplementedVertex, ISecondStageCommitAction
    {
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
            foreach (IEdge e in InEdges)
                if(e.From!=null) // there could be artificial edge, with From==null
                    e.From.OutEdgesDictionariesNeedsRebuild = true;

            foreach (IEdge e in OutEdges)
                e.To.InEdgesDictionariesNeedsRebuild = true;
        }

        public bool HasInheritance { get; set; }

        public bool AllowInheritance = true;

        // InEdges
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

        private IList<IEdge> _InEdges;

        public override IList<IEdge> InEdges
        {
            get
            {                
                return InEdgesRaw;
            }                       
        }        

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

            OutEdgesDictionariesNeedsRebuild_Edges = false;
        }

        private void InEdgesDictionariesRebuild_Meta()
        {
            _InEdgesByMeta = new Dictionary<object, object>();

            foreach(IEdge e in InEdges)
            {
                //object key = e.Meta.Value;
                object key = e.Meta.Value.ToString();
                IEdge value = e;

                if (_InEdgesByMeta.ContainsKey(key))
                {
                    object existingValue = _InEdgesByMeta[key];

                    if (existingValue is List_VertexBase) // list exists
                    {
                        ((IList<IEdge>)existingValue).Add(value);
                    }
                    else // need to create list
                    {
                        IList<IEdge> list = new List_VertexBase();
                        list.Add((IEdge)existingValue);
                        list.Add(value);

                        _InEdgesByMeta[key] = list;
                    }                        
                }
                else
                    _InEdgesByMeta.Add(key, value);
            }

            InEdgesDictionariesNeedsRebuild_Meta = false;
        }

        private void OutEdgesDictionariesRebuild_Meta()
        {
            _OutEdgesByMeta = new Dictionary<object, object>();

            foreach (IEdge e in OutEdges)
            {
                //object key = e.Meta.Value;
                object key;

                if (e.Meta == null)
                    key = "";
                else
                    key = e.Meta.Value.ToString();

                IEdge value = e;

                if (_OutEdgesByMeta.ContainsKey(key))
                {
                    object existingValue = _OutEdgesByMeta[key];

                    if (existingValue is List_VertexBase) // list exists
                    {
                        ((IList<IEdge>)existingValue).Add(value);
                    }
                    else // need to create list
                    {
                        IList<IEdge> list = new List_VertexBase();
                        list.Add((IEdge)existingValue);
                        list.Add(value);

                        _OutEdgesByMeta[key] = list;
                    }
                }
                else
                    _OutEdgesByMeta.Add(key, value);
            }

            OutEdgesDictionariesNeedsRebuild_Meta = false;
        }

        private void InEdgesDictionariesRebuild_Value()
        {
            _InEdgesByValue = new Dictionary<object, object>();

            foreach (IEdge e in InEdges)
            {
                //object key = e.From.Value;
                object key = e.From.Value.ToString();
                IEdge value = e;

                if (_InEdgesByValue.ContainsKey(key))
                {
                    object existingValue = _InEdgesByValue[key];

                    if (existingValue is List_VertexBase) // list exists
                    {
                        ((IList<IEdge>)existingValue).Add(value);
                    }
                    else // need to create list
                    {
                        IList<IEdge> list = new List_VertexBase();
                        list.Add((IEdge)existingValue);
                        list.Add(value);

                        _InEdgesByValue[key] = list;
                    }
                }
                else
                    _InEdgesByValue.Add(key, value);
            }

            InEdgesDictionariesNeedsRebuild_Value = false;
        }

        private void OutEdgesDictionariesRebuild_Value()
        {
            _OutEdgesByValue = new Dictionary<object, object>();

            foreach (IEdge e in OutEdges)
            {
                //object key = e.To.Value;
                object key = e.To.Value.ToString();
                IEdge value = e;

                if (_OutEdgesByValue.ContainsKey(key))
                {
                    object existingValue = _OutEdgesByValue[key];

                    if (existingValue is List_VertexBase) // list exists
                    {
                        ((IList<IEdge>)existingValue).Add(value);
                    }
                    else // need to create list
                    {
                        IList<IEdge> list = new List_VertexBase();
                        list.Add((IEdge)existingValue);
                        list.Add(value);

                        _OutEdgesByValue[key] = list;
                    }
                }
                else
                    _OutEdgesByValue.Add(key, value);
            }

            OutEdgesDictionariesNeedsRebuild_Value = false;
        }

        private void InEdgesDictionariesRebuild_MetaAndValue()
        {
            _InEdgesByMetaAndValue = new Dictionary<object, object>();

            foreach (IEdge e in InEdges)
            {
                object key = GraphUtil.GetMetaAndValueObject(e.Meta.Value, e.From.Value);
                IEdge value = e;

                if (_InEdgesByMetaAndValue.ContainsKey(key))
                {
                    object existingValue = _InEdgesByMetaAndValue[key];

                    if (existingValue is List_VertexBase) // list exists
                    {
                        ((IList<IEdge>)existingValue).Add(value);
                    }
                    else // need to create list
                    {
                        IList<IEdge> list = new List_VertexBase();
                        list.Add((IEdge)existingValue);
                        list.Add(value);

                        _InEdgesByMetaAndValue[key] = list;
                    }
                }
                else
                    _InEdgesByMetaAndValue.Add(key, value);
            }

            InEdgesDictionariesNeedsRebuild_MetaAndValue = false;
        }

        private void OutEdgesDictionariesRebuild_MetaAndValue()
        {
            _OutEdgesByMetaAndValue = new Dictionary<object, object>();

            foreach (IEdge e in OutEdges)
            {
                //object key = GraphUtil.GetMetaAndValueObject(e.Meta.Value,e.To.Value);
                object key = GraphUtil.GetMetaAndValueObject(e.Meta.Value.ToString(), e.To.Value.ToString());
                IEdge value = e;

                if (_OutEdgesByMetaAndValue.ContainsKey(key))
                {
                    object existingValue = _OutEdgesByMetaAndValue[key];

                    if (existingValue is List_VertexBase) // list exists
                    {
                        ((IList<IEdge>)existingValue).Add(value);
                    }
                    else // need to create list
                    {
                        IList<IEdge> list = new List_VertexBase();
                        list.Add((IEdge)existingValue);
                        list.Add(value);

                        _OutEdgesByMetaAndValue[key] = list;
                    }
                }
                else
                    _OutEdgesByMetaAndValue.Add(key, value);
            }

            OutEdgesDictionariesNeedsRebuild_MetaAndValue = false;
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
            if (_edge.Meta.Value.ToString() == "Item")
            {
                int x = 0;
            }

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

        private static IDictionary<String, IVertex> QueryParseCache = new Dictionary<String, IVertex>();
        private static IDictionary<String, IVertex> QueryParseCache_metaMode = new Dictionary<String, IVertex>();        

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

        private void DeleteAllEdges()
        {
            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");

            foreach (IEdge edge in OutEdgesRaw.ToList()) {             
                OutEdgesRaw.Remove(edge);                

                //FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, edge)); // moved from before edge.Meta.DeleteMetaInEdge(edge); XXX !!!
            }            
        }

        public void InheritChildsDictionariesNeedsRebuild(bool inDictiories)
        {
            HashSet<IVertex> inheritsSet = VertexHelper.GetInheritChilds(this);

            foreach (IVertex v in inheritsSet)
                if (inDictiories)
                    v.InEdgesDictionariesNeedsRebuild = true;
                else
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
                if (OutEdgesDictionariesNeedsRebuild_Meta)
                    OutEdgesDictionariesRebuild_Meta();

                if (!OutEdgesByMeta.ContainsKey(meta))
                    return; 

                object val = OutEdgesByMeta[meta];

                if (val is List_VertexBase)
                    results = (IList<IEdge>)val;
                else
                    result = (IEdge)val;

                return;
            }

            if (meta == null && to != null)
            {
                if (OutEdgesDictionariesNeedsRebuild_Value)
                    OutEdgesDictionariesRebuild_Value();

                if (!OutEdgesByValue.ContainsKey(to))
                    return;

                object val = OutEdgesByValue[to];

                if (val is List_VertexBase)
                    results = (IList<IEdge>)val;
                else
                    result = (IEdge)val;

                return;
            }

            if (meta != null && to != null)
            {
               if (OutEdgesDictionariesNeedsRebuild_MetaAndValue)
                    OutEdgesDictionariesRebuild_MetaAndValue();

                object searchKey = GraphUtil.GetMetaAndValueObject(meta, to);

                if (!OutEdgesByMetaAndValue.ContainsKey(searchKey))
                    return;

                object val = OutEdgesByMetaAndValue[searchKey];

                if (val is List_VertexBase)
                    results = (IList<IEdge>)val;
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

                if (!InEdgesByMeta.ContainsKey(meta))
                    return;

                object val = InEdgesByMeta[meta];

                if (val is List_VertexBase)
                    results = (IList<IEdge>)val;
                else
                    result = (IEdge)val;

                return;
            }

            if (meta == null && from != null)
            {
                if (InEdgesDictionariesNeedsRebuild_Value)
                    InEdgesDictionariesRebuild_Value();

                if (!InEdgesByValue.ContainsKey(from))
                    return;

                object val = InEdgesByValue[from];

                if (val is List_VertexBase)
                    results = (IList<IEdge>)val;
                else
                    result = (IEdge)val;

                return;
            }

            if (meta != null && from != null)
            {
                if (InEdgesDictionariesNeedsRebuild_MetaAndValue)
                    InEdgesDictionariesRebuild_MetaAndValue();

                object searchKey = GraphUtil.GetMetaAndValueObject(meta, from);

                if (!InEdgesByMetaAndValue.ContainsKey(searchKey))
                    return;

                object val = InEdgesByMetaAndValue[searchKey];

                if (val is List_VertexBase)
                    results = (IList<IEdge>)val;
                else
                    result = (IEdge)val;

                return;
            }

            results = InEdges.ToList();
        }

        public override IVertex Get(bool metaMode, string query)
        {
            IVertex queryVertex = null;
            IVertex parseError = null;

            IDictionary<String, IVertex> cache;

            if (metaMode)
                cache = QueryParseCache_metaMode;
            else
                cache = QueryParseCache;

            if (cache.ContainsKey(query))
                queryVertex = cache[query];
            else
            {
                queryVertex = MinusZero.Instance.CreateTempVertex();

                IEdge baseEdge_new;
                parseError = MinusZero.Instance.DefaultFormalTextParser.Parse( new EdgeBase(null, null, queryVertex), query, CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new);

                if (parseError == null || parseError.Count() == 0 /* && !cache.ContainsKey(query)*/)
                {
                    cache.Add(query, queryVertex);
                    queryVertex.AddExternalReference();
                }
            }

            if (parseError != null && parseError.Count() > 0)
                return null;

            return MinusZero.Instance.DefaultExecuter.Get(metaMode, this, queryVertex);
        }

        public override IVertex GetAll(bool metaMode, string query)
        {
            IVertex queryVertex = null;
            IVertex parseError = null;

            IDictionary<String, IVertex> cache;

            if (metaMode)
                cache = QueryParseCache_metaMode;
            else
                cache = QueryParseCache;

            if (cache.ContainsKey(query))
                queryVertex = cache[query];
            else
            {
                queryVertex = MinusZero.Instance.CreateTempVertex();

                IEdge baseEdge_new;
                parseError = MinusZero.Instance.DefaultFormalTextParser.Parse(new EdgeBase(null, null, queryVertex), query, CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new);

                if (parseError == null || parseError.Count() == 0/* && || !cache.ContainsKey(query)*/)  // it happens to exist there so need to check again
                {
                    cache.Add(query, queryVertex);
                    queryVertex.AddExternalReference();
                }
            }

            if (parseError != null && parseError.Count() > 0)
                return null;

            return MinusZero.Instance.DefaultExecuter.GetAll(metaMode, this, queryVertex);            
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
            lock (lock_object)
            {
                VertexInit_First();

                _Identifier = Store.VertexIdentifierCount++;

                //Store.VertexIdentifierCount += RND.Next(10) + 1;

                //_Identifier = Store.VertexIdentifierCount;

                GraphUtil.Debug(this, DebugOperationEnum.Init);

                Store.StoreVertexIdentifier(this);
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
            _OutEdgesByValue = null;
            _OutEdgesByMetaAndValue = null;
            _InEdgesByMeta = null;
            _InEdgesByValue = null;
            _InEdgesByMetaAndValue = null;
        }
    }
}
