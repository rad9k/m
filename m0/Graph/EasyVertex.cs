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

namespace m0.Graph
{
    [Serializable]
    public class EasyVertex:VertexBase, IDisposable
    {        
        public override int UsageCounter
        {
            get
            {            
                return base.UsageCounter;
            }
            set
            {
                if(base.UsageCounter < value && UsageCounter == 0 && Store.DetachState == DetachStateEnum.Attached)
                        Store.StoreVertexIdentifier(this);

                if (base.UsageCounter > value && UsageCounter == 1 && Store.DetachState == DetachStateEnum.Attached)
                        Store.RemoveVertexIdentifier(this);

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

                FireChange(new VertexChangeEventArgs(VertexChangeType.ValueChanged, null));
            }
        }

        protected bool HasInheritance=false;

        protected int InheritanceCount = 0;

        protected IList<IEdge> _InEdgesRaw;

        public override IList<IEdge> InEdgesRaw { get { return _InEdgesRaw; } }

        private IList<IEdge> _InEdges;

        public override IEnumerable<IEdge> InEdges
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
            if (HasInheritance)
            {
                List<IEdge> FullEdges = InEdgesRaw.ToList();

                HashSet<IVertex> parents = GraphUtil.GetInheritParents(this);

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

        public override IEnumerable<IEdge> OutEdges
        {
            get
            {
                if(OutEdgesDictionariesNeedsRebuild_Edges)
                {
                    OutEdgesDictionariesRebuild_Edges();                    
                    return _OutEdges;
                }
                else
                    return _OutEdges;                
            }
        }

        private void OutEdgesDictionariesRebuild_Edges()
        {
            if (HasInheritance)
            {
                List<IEdge> FullEdges = OutEdgesRaw.ToList();

                HashSet<IVertex> parents = GraphUtil.GetInheritParents(this);

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
                object key = e.Meta.Value;
                object value = e.To.Value;

                if (_InEdgesByMeta.ContainsKey(key))
                {
                    object existingValue = _InEdgesByMeta[key];

                    if (existingValue is List_VertexBase) // list exists
                    {
                        ((IList<object>)existingValue).Add(value);
                    }
                    else // need to create list
                    {
                        IList<object> list = new List_VertexBase();
                        list.Add(existingValue);

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
                object key = e.Meta.Value;
                object value = e.To.Value;

                if (_OutEdgesByMeta.ContainsKey(key))
                {
                    object existingValue = _OutEdgesByMeta[key];

                    if (existingValue is List_VertexBase) // list exists
                    {
                        ((IList<object>)existingValue).Add(value);
                    }
                    else // need to create list
                    {
                        IList<object> list = new List_VertexBase();
                        list.Add(existingValue);

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
                object key = e.To.Value;
                object value = e.To.Value;

                if (_InEdgesByValue.ContainsKey(key))
                {
                    object existingValue = _InEdgesByValue[key];

                    if (existingValue is List_VertexBase) // list exists
                    {
                        ((IList<object>)existingValue).Add(value);
                    }
                    else // need to create list
                    {
                        IList<object> list = new List_VertexBase();
                        list.Add(existingValue);

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
                object key = e.To.Value;
                object value = e.To.Value;

                if (_OutEdgesByValue.ContainsKey(key))
                {
                    object existingValue = _OutEdgesByValue[key];

                    if (existingValue is List_VertexBase) // list exists
                    {
                        ((IList<object>)existingValue).Add(value);
                    }
                    else // need to create list
                    {
                        IList<object> list = new List_VertexBase();
                        list.Add(existingValue);

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
                object key = e.Meta.Value.ToString()+"|"+e.To.Value.ToString();
                object value = e.To.Value;

                if (_InEdgesByMetaAndValue.ContainsKey(key))
                {
                    object existingValue = _InEdgesByMetaAndValue[key];

                    if (existingValue is List_VertexBase) // list exists
                    {
                        ((IList<object>)existingValue).Add(value);
                    }
                    else // need to create list
                    {
                        IList<object> list = new List_VertexBase();
                        list.Add(existingValue);

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
                object key = e.Meta.Value.ToString() + "|" + e.To.Value.ToString();
                object value = e.To.Value;

                if (_OutEdgesByMetaAndValue.ContainsKey(key))
                {
                    object existingValue = _OutEdgesByMetaAndValue[key];

                    if (existingValue is List_VertexBase) // list exists
                    {
                        ((IList<object>)existingValue).Add(value);
                    }
                    else // need to create list
                    {
                        IList<object> list = new List_VertexBase();
                        list.Add(existingValue);

                        _OutEdgesByMetaAndValue[key] = list;
                    }
                }
                else
                    _OutEdgesByMetaAndValue.Add(key, value);
            }

            OutEdgesDictionariesNeedsRebuild_MetaAndValue = false;
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

            foreach (IEdge e in InEdgesRaw)
                if (e.Meta == _edge.Meta && e.From == _edge.From)
                    edge = e;

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

        public override IEdge AddEdge(IVertex metaVertex, IVertex destVertex)
        {
            if (destVertex == null)
                throw new Exception("target vertex can not be null");

            EdgeBase ne = new EasyEdge(this, metaVertex, destVertex);

            OutEdgesRaw.Add(ne);

            UsageCounter++;

            OutEdgesDictionariesNeedsRebuild = true;

            InheritChildsDictionariesNeedsRebuild(false);

            if (GeneralUtil.CompareStrings(ne.Meta.Value, "$Inherits"))
            {
                InheritanceCount++;

                HasInheritance = true;
            }

            FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeAdded, ne));

            return ne;
        }

        public override void DeleteEdge(IEdge _edge)
        {
            IEdge edge = null;

            foreach (IEdge e in OutEdgesRaw)
                if(e.Meta==_edge.Meta && e.To==_edge.To)
                   edge = e;

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

                edge.To.DeleteInEdge(edge);
            }
            //else // becouse of inheritance this may happen
                //throw new Exception(_edge.Meta + " : " + _edge.To + " edge does not exist in given Vertex");
        }        

        public EasyVertex(IStore _store):base(_store)
        {
            _InEdgesRaw = new List<IEdge>();
            _OutEdgesRaw = new List<IEdge>();


            _Identifier = Store.VertexIdentifierCount++;

            Value = "";

            InEdgesDictionariesNeedsRebuild = true;
            OutEdgesDictionariesNeedsRebuild = true;
        }

        public override IVertex Execute(IVertex inputVertex, IVertex expression)
        { 
            //
            MinusZero m0 = MinusZero.Instance;
            //m0.Log(2,"Execute", "\"" + expression + "\"");
            bool prevDoLog = m0.DoLog;
            m0.DoLog = false;
            //

            IVertex ret= MinusZero.Instance.DefaultExecuter.Execute(this, inputVertex, expression);

            //
            m0.DoLog = prevDoLog;
            //

            return ret;
        }

        private static IDictionary<String, IVertex> ParseChache= new Dictionary<String,IVertex>();

        public override IVertex Get(string query)
        {
            //
            MinusZero m0=MinusZero.Instance;
            //m0.Log(2,"Get", "\"" + query + "\"");
            bool prevDoLog = m0.DoLog;
            m0.DoLog = false;
            //

            IVertex queryVertex = null;
            IVertex parseError = null;

            if (ParseChache.ContainsKey(query))
                queryVertex = ParseChache[query];
            else            
            {
                queryVertex = MinusZero.Instance.CreateTempVertex();

                parseError = MinusZero.Instance.DefaultParser.Parse(queryVertex, query);

                if (parseError == null)
                    ParseChache.Add(query, queryVertex);
            }
                            
            //
            m0.DoLog = prevDoLog;
            //

            if (parseError != null)
                return null;

            return MinusZero.Instance.DefaultExecuter.Get(this, queryVertex);            
        }

        public override IVertex GetAll(string query)
        {
            //
            MinusZero m0 = MinusZero.Instance;
            //m0.Log(2, "GetAll", "\"" + query + "\"");
            bool prevDoLog = m0.DoLog;
            m0.DoLog = false;
            //

            IVertex queryVertex = null;
            IVertex parseError = null;

            if (ParseChache.ContainsKey(query))
                queryVertex = ParseChache[query];
            else
            {
                queryVertex = MinusZero.Instance.CreateTempVertex();

                parseError = MinusZero.Instance.DefaultParser.Parse(queryVertex, query);

                if (parseError == null)
                    ParseChache.Add(query, queryVertex);
            }

            //
            m0.DoLog = prevDoLog;
            //

            if (parseError != null)
                return null;

            return MinusZero.Instance.DefaultExecuter.GetAll(this, queryVertex);
        }

        public void Dispose()
        {
            GraphUtil.RemoveAllEdges(this);                        
        }

        private void InheritChildsDictionariesNeedsRebuild(bool inDictiories)
        {
            HashSet<IVertex> inheritsSet = GraphUtil.GetInheritChilds(this);

            foreach (IVertex v in inheritsSet)
                if (inDictiories)
                    v.InEdgesDictionariesNeedsRebuild = true;
                else
                    v.OutEdgesDictionariesNeedsRebuild = true;
        }
        
    }
}
