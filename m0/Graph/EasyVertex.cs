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

namespace m0.Graph
{
    [Serializable]
    public class EasyVertex: VertexBase, IDisposable, IImplementedVertex
    {
        protected EdgeDictionaries ed;

        public object _Identifier;
        
        public override object Identifier { get { return _Identifier; }}


        protected object _Value;

        public override object Value {
            get{
                return _Value;
            }
            set{
                _Value = value;

                ValueChanged();

                FireChange(new VertexChangeEventArgs(VertexChangeType.ValueChanged, null));
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

        public int InheritanceCount { get; set; }

        public override IList<IEdge> InEdgesRaw { get { return ed.In; } }

        private IList<IEdge> _InEdges;

        public override IList<IEdge> InEdges
        {
            get
            {
                if (InEdgesDictionariesNeedsRebuild_Edges)
                {
                    InEdgesDictionariesRebuild_Edges();
                    InEdgesDictionariesNeedsRebuild_Edges = false;
                    return _InEdges;
                }
                else
                    return _InEdges;
            }                       
        }

        private void InEdgesDictionariesRebuild_Edges()
        {
            if (HasInheritance && AllowInheritance)
            {
                List<IEdge> FullEdges = InEdgesRaw.ToList();

                HashSet<IVertex> parents = GraphUtil.GetInheritParents_RawEnumerate(this);

                foreach (IVertex v in parents)
                    FullEdges.AddRange(v.InEdgesRaw);

                _InEdges = FullEdges;
            }
            else
                _InEdges = InEdgesRaw;
        }

        public override IList<IEdge> OutEdgesRaw { get { return ed.Out; } }

        private IList<IEdge> _OutEdges;

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

        public override IList<IEdge> MetaInEdgesRaw { get { return ed.MetaIn; } }

        private void OutEdgesDictionariesRebuild_Edges()
        {
            if (HasInheritance && AllowInheritance)
            {
                List<IEdge> FullEdges = OutEdgesRaw.ToList();

                HashSet<IVertex> parents = GraphUtil.GetInheritParents_RawEnumerate(this);

                foreach (IVertex v in parents)
                    FullEdges.AddRange(v.OutEdgesRaw);    

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
                object key = e.Meta.Value.ToString();
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

        public override IEdge AddEdge(IVertex metaVertex, IVertex destVertex)
        {
            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex disposed");

            if (destVertex == null)
                destVertex = MinusZero.Instance.Empty; // can be    

            EdgeBase ne = new EasyEdge(this, metaVertex, destVertex);

            OutEdgesRaw.Add(ne);

            AttachEdge(ne);                       

            return ne;
        }

        public override void AttachEdge(IEdge edge)
        {            
            if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
            {
                InheritanceCount++;

                HasInheritance = true;
            }
        }

        public override void AddEdgesList(IEnumerable<IEdge> edges)
        {            
            foreach (IEdge e in edges) // possibly not optimal implementation
                AddEdge(e.Meta, e.To);
        }
        
        public override void DeleteEdge(IEdge _edge)
        {
            if (Value is string && ((String)Value).StartsWith("Tree"))
            {
                int x = 0;
            }

            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex disposed");

            IEdge edge = ed.Out.Get(_edge);

            if (edge != null)
            {                
                OutEdgesRaw.Remove(edge);
                
               
            }
        }

        public override void DeleteEdgesList(IEnumerable<IEdge> edges)
        {            
            foreach (IEdge e in edges) // possibly not optimal implementation
                DeleteEdge(e); // Meta/To check to be performed
        }       

        private static IDictionary<String, IVertex> QueryParseChache = new Dictionary<String, IVertex>();
        private static IDictionary<String, IVertex> QueryParseChache_metaMode = new Dictionary<String, IVertex>();        

        public override void Dispose()
        {
            if (DisposedState == DisposeStateEnum.Live)
            {
                DisposedState = DisposeStateEnum.Disposing;

                ChangeRemoveAllHandlers();

                DeleteAllInEdges();
                DeleteAllMetaInEdges();
                DeleteAllEdges();

                Store.RemoveVertexIdentifier(this);

                DisposedState = DisposeStateEnum.Disposed;
            }
        }

        public void DeleteAllInEdges()
        {
            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex disposed");            

            foreach (IEdge edge in InEdgesRaw.ToList())          
                InEdgesRaw.Remove(edge);                        
        }

        public void DeleteAllMetaInEdges()
        {
            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex disposed");

            foreach (IEdge edge in MetaInEdgesRaw.ToList())
                MetaInEdgesRaw.Remove(edge);
        }

        private void DeleteAllEdges()
        {
            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex disposed");

            foreach (IEdge edge in OutEdgesRaw.ToList()) {             
                OutEdgesRaw.Remove(edge);                

                FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, edge)); // moved from before edge.Meta.DeleteMetaInEdge(edge); XXX !!!
            }            
        }

        public void InheritChildsDictionariesNeedsRebuild(bool inDictiories)
        {
            HashSet<IVertex> inheritsSet = GraphUtil.GetInheritChilds_RawEnumerate(this);

            foreach (IVertex v in inheritsSet)
                if (inDictiories)
                    v.InEdgesDictionariesNeedsRebuild = true;
                else
                    v.OutEdgesDictionariesNeedsRebuild = true;
        }

        public override void QueryOutEdges(object meta, object to, out IEdge result, out IList<IEdge> results)
        {
            result = null;
            results = null;

            if(meta!=null && to == null)
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

            IDictionary<String, IVertex> chache;

            if (metaMode)
                chache = QueryParseChache_metaMode;
            else
                chache = QueryParseChache;

            if (chache.ContainsKey(query))
                queryVertex = chache[query];
            else
            {
                queryVertex = MinusZero.Instance.CreateTempVertex();

                parseError = MinusZero.Instance.DefaultParser.Parse(queryVertex, query);

                if (parseError == null || parseError.Count() == 0)
                    chache.Add(query, queryVertex);
            }

            if (parseError != null && parseError.Count() > 0)
                return null;

            return MinusZero.Instance.DefaultExecuter.Get(metaMode, this, queryVertex);
        }

        public override IVertex GetAll(bool metaMode, string query)
        {
            IVertex queryVertex = null;
            IVertex parseError = null;

            IDictionary<String, IVertex> chache;

            if (metaMode)
                chache = QueryParseChache_metaMode;
            else
                chache = QueryParseChache;

            if (chache.ContainsKey(query))
                queryVertex = chache[query];
            else
            {
                queryVertex = MinusZero.Instance.CreateTempVertex();

                parseError = MinusZero.Instance.DefaultParser.Parse(queryVertex, query);

                if (parseError == null || parseError.Count() == 0 || !chache.ContainsKey(query)) // it happens to exist there so need to check again
                    chache.Add(query, queryVertex);
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
            bool dummy;

            IVertex ExecutableEndPointVertex = GraphUtil.GetQueryOutFirst(this, "$ExecutableEndPoint", null);

            if(ExecutableEndPointVertex==null)
                return InstructionHelpers.SequentiallyExecuteInstructions(exe, exe.stack, this, out dummy, false);
            else
                return CallableEndPointDictionary_INIEIOV_ZCE.CallEndPoint(exe, ExecutableEndPointVertex);
        }

        protected void VertexInit_First()
        {
            ed = new EdgeDictionaries(this);

            InheritanceCount = 0;

            InEdgesDictionariesNeedsRebuild = true;
            OutEdgesDictionariesNeedsRebuild = true;

            Value = "";
        }

        protected virtual void VertexInit()
        {
            VertexInit_First();

            _Identifier = Store.VertexIdentifierCount++;

            Store.StoreVertexIdentifier(this);
        }

        public EasyVertex(IStore _store) : base(_store)
        {
            VertexInit();
        }

        public EasyVertex(IStore _store, object toBeIdentifier) : base(_store)
        {
            DisposedState = DisposeStateEnum.Live;
 
            VertexInit_First();

            _Identifier = toBeIdentifier;

            if (toBeIdentifier is int)
            {
                int val = (int)toBeIdentifier + 1;

                if (val > Store.VertexIdentifierCount)
                    Store.VertexIdentifierCount = val;
            }

            Store.StoreVertexIdentifier(this);
        }

    }
}
