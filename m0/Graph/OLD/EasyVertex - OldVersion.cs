/*using System;
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

namespace m0.Graph.OLD
{
    [Serializable]
    public class EasyVertex: VertexBase, IDisposable
    {
        public override int UsageCounter
        {
            get
            {            
                return base.UsageCounter;
            }
            set
            {
             //   if (base.UsageCounter < value && UsageCounter == 0 && Store.DetachState == DetachStateEnum.Attached)
               //         Store.StoreVertexIdentifier(this);

             //   if (base.UsageCounter > value && UsageCounter == 1 && Store.DetachState == DetachStateEnum.Attached)
             //           Store.RemoveVertexIdentifier(this); // EAT THIS!!!

                base.UsageCounter = value;
            }
        }

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

        protected bool HasInheritance = false;

        public bool AllowInheritance = true;

        protected int InheritanceCount = 0;

        protected IList<IEdge> _InEdgesRaw;

        public override IList<IEdge> InEdgesRaw { get { return _InEdgesRaw; } }

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

        protected IList<IEdge> _OutEdgesRaw;

        public override IList<IEdge> OutEdgesRaw { get { return _OutEdgesRaw; } }

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

        protected IList<IEdge> _MetaInEdgesRaw;

        public override IList<IEdge> MetaInEdgesRaw { get { return _MetaInEdgesRaw; } }

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

        public override void AddMetaInEdge(IEdge edge)
        {            
            MetaInEdgesRaw.Add(edge);

            UsageCounter++;         
        }

        public override void DeleteMetaInEdge(IEdge _edge)
        {            
            IEdge edge = null;

            if (MetaInEdgesRaw.Contains(_edge))
                edge = _edge;
            else
                foreach (IEdge e in MetaInEdgesRaw)
                    if (e.From == _edge.From && e.Meta == _edge.Meta && e.From == _edge.From)
                        edge = e;

            if (edge != null)
            {
                MetaInEdgesRaw.Remove(edge);

                UsageCounter--;
            }
        }

        public override void AddInEdge(IEdge edge)
        {            
            InEdgesRaw.Add(edge);

            UsageCounter++;

            InEdgesDictionariesNeedsRebuild = true;

            InheritChildsDictionariesNeedsRebuild(true);

            //FireChange(new VertexChangeEventArgs(VertexChangeType.InEdgeAdded,edge));
            // not needed as for now
        }

        public override void DeleteInEdge(IEdge _edge)
        {
            IEdge edge = null;

            if (InEdgesRaw.Contains(_edge))
                edge = _edge;
            else
                foreach (IEdge e in InEdgesRaw)
                  if (e.From == _edge.From && e.Meta == _edge.Meta && e.From == _edge.From)
                     edge = e;

            if (edge != null)
            {
                DeleteInEdgeOnlyIn(edge);

                edge.From.DeleteEdgeOnlyOut(edge);
            }

            //FireChange(new VertexChangeEventArgs(VertexChangeType.InEdgeRemoved, edge));
            // not needed as for now
        }

        public override void DeleteInEdgeOnlyIn(IEdge edge)
        {
            if (edge != null)
            {
                InEdgesRaw.Remove(edge);

                UsageCounter--;

                InEdgesDictionariesNeedsRebuild = true;

                InheritChildsDictionariesNeedsRebuild(true);
            }

            //FireChange(new VertexChangeEventArgs(VertexChangeType.InEdgeRemoved, edge));
            // not needed as for now
        }

        public void AddOutEdgesRaw(IEdge e)
        {
            OutEdgesRaw.Add(e);

            UsageCounter++;
        }

        public override IEdge AddEdge(IVertex metaVertex, IVertex destVertex)
        {             
            if (destVertex == null)
                destVertex = MinusZero.Instance.Empty; // can be    

            EdgeBase ne = new EasyEdge(this, metaVertex, destVertex);

            AddOutEdgesRaw(ne);

            AttachEdge(ne);

            FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeAdded, ne));

            return ne;
        }

        public override void AttachEdge(IEdge edge)
        {            
            OutEdgesDictionariesNeedsRebuild = true;

            InheritChildsDictionariesNeedsRebuild(false);

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
            IEdge edge = null;

            if (OutEdgesRaw.Contains(_edge))
                edge = _edge;
            else
                foreach (IEdge e in OutEdgesRaw)
                    if(e.From == _edge.From && e.Meta ==_edge.Meta && e.To ==_edge.To)
                       edge = e;

            if (edge != null)
            {
                DeleteEdgeOnlyOut(edge);

                // FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, edge)); // it is in DeleteEdgeOnlyOut 

                edge.Meta.DeleteMetaInEdge(edge); // XXX I think that this is ok            

                edge.To.DeleteInEdgeOnlyIn(edge);
            }
            //else // becouse of inheritance this may happen
                //throw new Exception(_edge.Meta + " : " + _edge.To + " edge does not exist in given Vertex");
        }

        public override void DeleteEdgeOnlyOut(IEdge edge)
        {
            if (edge != null)
            {
                OutEdgesRaw.Remove(edge);

                UsageCounter--;

                OutEdgesDictionariesNeedsRebuild = true;
                InheritChildsDictionariesNeedsRebuild(false);

                if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
                {
                    InheritanceCount--;

                    if (InheritanceCount == 0)
                        HasInheritance = false;
                }

                FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, edge));
            }
            //else // becouse of inheritance this may happen
            //throw new Exception(_edge.Meta + " : " + _edge.To + " edge does not exist in given Vertex");
        }

        public override void DeleteEdgesList(IEnumerable<IEdge> edges)
        {
            foreach (IEdge e in edges) // possibly not optimal implementation
                DeleteEdge(e); // Meta/To check to be performed
        }

        public EasyVertex(IStore _store):base(_store)
        {
            _InEdgesRaw = new List<IEdge>();
            _OutEdgesRaw = new List<IEdge>();
            _MetaInEdgesRaw = new List<IEdge>();


            _Identifier = Store.VertexIdentifierCount++;            

            InEdgesDictionariesNeedsRebuild = true;
            OutEdgesDictionariesNeedsRebuild = true;

            Value = "";

            Store.StoreVertexIdentifier(this);
        }        

        private static IDictionary<String, IVertex> QueryParseChache = new Dictionary<String, IVertex>();
        private static IDictionary<String, IVertex> QueryParseChache_metaMode = new Dictionary<String, IVertex>();        

        bool hasBeenDisposed = false;
        public void Dispose()
        {
            if (!hasBeenDisposed)
            {
                DeleteAllInEdges();
                DeleteAllEdges();

                hasBeenDisposed = true;
            }
        }

        public void DeleteAllInEdges()
        {
            //foreach(IEdge edge in InEdgesRaw) // constant "collection modified during enumeration" exceptions
            foreach (IEdge edge in InEdgesRaw.ToList()) 
            {
                InEdgesRaw.Remove(edge);

                UsageCounter--;

                edge.From.DeleteEdgeOnlyOut(edge);

                //FireChange(new VertexChangeEventArgs(VertexChangeType.InEdgeRemoved, edge));
                // not needed as for now
            }

            InEdgesDictionariesNeedsRebuild = true;

            InheritChildsDictionariesNeedsRebuild(true);
        }

        private void DeleteAllEdges()
        {
            foreach (IEdge edge in InEdgesRaw)
            {
                OutEdgesRaw.Remove(edge);

                UsageCounter--;

                if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
                {
                    InheritanceCount--;

                    if (InheritanceCount == 0)
                        HasInheritance = false;
                }                

                edge.Meta.DeleteMetaInEdge(edge); // XXX I think that this is 

                edge.To.DeleteInEdgeOnlyIn(edge);

                FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, edge)); // moved from before edge.Meta.DeleteMetaInEdge(edge); XXX !!!
            }

            OutEdgesDictionariesNeedsRebuild = true;
            InheritChildsDictionariesNeedsRebuild(false);
        }

        protected void InheritChildsDictionariesNeedsRebuild(bool inDictiories)
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

                if (parseError == null || parseError.Count() == 0)
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

        public override void Destroy()
        {
            Dispose();
        }

    }
}


*/