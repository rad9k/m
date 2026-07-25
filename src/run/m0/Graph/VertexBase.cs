using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using m0.Foundation;
using m0.Util;

namespace m0.Graph
{
    [Serializable]
    public class List_VertexBase : List<IEdge> { } // to be used in dictionaries, to identify list of List<IEdge> :)

    [Serializable]
    public class VertexBase : IVertex, IDisposable
    {
        public DisposeStateEnum DisposedState { get; set; }

        public bool IsRoot { get; set; }

        [Serializable]
        private sealed class VertexIndexDictionaries
        {
            public IDictionary<object, object> OutEdgesByMeta;
            public Dictionary<string, object> OutEdgesByValue;
            public Dictionary<GraphUtil.MetaAndValueKey, object>
                OutEdgesByMetaAndValue;
            public Dictionary<string, object> InEdgesByMeta;
            public Dictionary<string, object> InEdgesByValue;
            public Dictionary<GraphUtil.MetaAndValueKey, object>
                InEdgesByMetaAndValue;
        }

        private VertexIndexDictionaries vertexIndexDictionaries;

        private VertexIndexDictionaries GetVertexIndexDictionaries()
        {
            return vertexIndexDictionaries ??=
                new VertexIndexDictionaries();
        }

        protected IDictionary<object, object> _OutEdgesByMeta
        {
            get
            {
                return vertexIndexDictionaries
                    ?.OutEdgesByMeta;
            }
            set
            {
                if (value == null)
                {
                    if (vertexIndexDictionaries != null)
                        vertexIndexDictionaries.OutEdgesByMeta =
                            null;
                    return;
                }

                GetVertexIndexDictionaries()
                    .OutEdgesByMeta = value;
            }
        }

        public IDictionary<object, object> OutEdgesByMeta { get { return _OutEdgesByMeta; } }

        protected Dictionary<string, object> _OutEdgesByValue
        {
            get
            {
                return vertexIndexDictionaries
                    ?.OutEdgesByValue;
            }
            set
            {
                if (value == null)
                {
                    if (vertexIndexDictionaries != null)
                        vertexIndexDictionaries.OutEdgesByValue =
                            null;
                    return;
                }

                GetVertexIndexDictionaries()
                    .OutEdgesByValue = value;
            }
        }

        public Dictionary<string, object> OutEdgesByValue { get { return _OutEdgesByValue; } }

        protected Dictionary<GraphUtil.MetaAndValueKey, object>
            _OutEdgesByMetaAndValue
        {
            get
            {
                return vertexIndexDictionaries
                    ?.OutEdgesByMetaAndValue;
            }
            set
            {
                if (value == null)
                {
                    if (vertexIndexDictionaries != null)
                        vertexIndexDictionaries
                            .OutEdgesByMetaAndValue = null;
                    return;
                }

                GetVertexIndexDictionaries()
                    .OutEdgesByMetaAndValue = value;
            }
        }

        public Dictionary<GraphUtil.MetaAndValueKey, object> OutEdgesByMetaAndValue { get { return _OutEdgesByMetaAndValue; } }

        protected Dictionary<string, object> _InEdgesByMeta
        {
            get
            {
                return vertexIndexDictionaries
                    ?.InEdgesByMeta;
            }
            set
            {
                if (value == null)
                {
                    if (vertexIndexDictionaries != null)
                        vertexIndexDictionaries.InEdgesByMeta =
                            null;
                    return;
                }

                GetVertexIndexDictionaries()
                    .InEdgesByMeta = value;
            }
        }

        public Dictionary<string, object> InEdgesByMeta { get { return _InEdgesByMeta; } }

        protected Dictionary<string, object> _InEdgesByValue
        {
            get
            {
                return vertexIndexDictionaries
                    ?.InEdgesByValue;
            }
            set
            {
                if (value == null)
                {
                    if (vertexIndexDictionaries != null)
                        vertexIndexDictionaries.InEdgesByValue =
                            null;
                    return;
                }

                GetVertexIndexDictionaries()
                    .InEdgesByValue = value;
            }
        }

        public Dictionary<string, object> InEdgesByValue { get { return _InEdgesByValue; } }

        protected Dictionary<GraphUtil.MetaAndValueKey, object>
            _InEdgesByMetaAndValue
        {
            get
            {
                return vertexIndexDictionaries
                    ?.InEdgesByMetaAndValue;
            }
            set
            {
                if (value == null)
                {
                    if (vertexIndexDictionaries != null)
                        vertexIndexDictionaries
                            .InEdgesByMetaAndValue = null;
                    return;
                }

                GetVertexIndexDictionaries()
                    .InEdgesByMetaAndValue = value;
            }
        }

        public Dictionary<GraphUtil.MetaAndValueKey, object> InEdgesByMetaAndValue { get { return _InEdgesByMetaAndValue; } }

        private bool _InEdgesDictionariesNeedsRebuild;
        public bool InEdgesDictionariesNeedsRebuild {
            get {
                return _InEdgesDictionariesNeedsRebuild;
            }
            set {
                if (value)
                {
                    _InEdgesDictionariesNeedsRebuild = true;
         
                    InEdgesDictionariesNeedsRebuild_Meta = true;
                    InEdgesDictionariesNeedsRebuild_Value = true;
                    InEdgesDictionariesNeedsRebuild_MetaAndValue = true;
                }
                else
                {
                    _InEdgesDictionariesNeedsRebuild = false;

                    InEdgesDictionariesNeedsRebuild_Meta = false;
                    InEdgesDictionariesNeedsRebuild_Value = false;
                    InEdgesDictionariesNeedsRebuild_MetaAndValue = false;
                }
            }
        }        

        protected bool InEdgesDictionariesNeedsRebuild_Meta { get; set; }

        protected bool InEdgesDictionariesNeedsRebuild_Value { get; set; }

        protected bool InEdgesDictionariesNeedsRebuild_MetaAndValue { get; set; }

        private bool _OutEdgesDictionariesNeedsRebuild;
        public bool OutEdgesDictionariesNeedsRebuild
        {
            get
            {
                return _OutEdgesDictionariesNeedsRebuild;
            }
            set
            {
                if (value)
                {
                    _OutEdgesDictionariesNeedsRebuild = true;

                    OutEdgesDictionariesNeedsRebuild_Edges = true;
                    OutEdgesDictionariesNeedsRebuild_Meta = true;
                    OutEdgesDictionariesNeedsRebuild_QueryMeta = true;
                    OutEdgesDictionariesNeedsRebuild_Value = true;
                    OutEdgesDictionariesNeedsRebuild_MetaAndValue = true;
                }
                else
                {
                    _OutEdgesDictionariesNeedsRebuild = false;

                    OutEdgesDictionariesNeedsRebuild_Edges = false;
                    OutEdgesDictionariesNeedsRebuild_Meta = false;
                    OutEdgesDictionariesNeedsRebuild_QueryMeta = false;
                    OutEdgesDictionariesNeedsRebuild_Value = false;
                    OutEdgesDictionariesNeedsRebuild_MetaAndValue = false;
                }
            }
        }

        protected bool OutEdgesDictionariesNeedsRebuild_Edges { get; set; }

        protected bool OutEdgesDictionariesNeedsRebuild_Meta { get; set; }

        protected bool OutEdgesDictionariesNeedsRebuild_QueryMeta { get; set; }

        protected bool OutEdgesDictionariesNeedsRebuild_Value { get; set; }

        protected bool OutEdgesDictionariesNeedsRebuild_MetaAndValue { get; set; }

        public IEdge this[string meta] // XXX serveral optimisations needed!
        {
            get {
                IVertex r = this.GetAll(false, meta + ":");

                if (r.Count() > 0)
                    return r.First();

                IVertex metavertexs = this.GetAll(false, @"$Is:\" + meta);

                if (metavertexs.Count() > 0)
                {
                    IEdge metavertex = metavertexs.First();

                    IEdge newedge = new EasyEdge(this, metavertex.To, null);

                    return newedge;
                }
                else
                {
                    if (meta == "$EdgeTarget")
                    {
                        IEdge metavertex = m0.MinusZero.Instance.Root.GetAll(false, @"System\Meta\Base\Vertex\$EdgeTarget").FirstOrDefault();

                        IEdge newedge = new EasyEdge(this, metavertex.To, null);

                        return newedge;
                    }
                }

                // inherits from Vertex. hiddenly
                // this is for Table Visualiser
                IEdge vertexMetaVertex = m0.MinusZero.Instance.Root.GetAll(false, @"System\Meta\Base\Vertex\" + meta).FirstOrDefault();

                if (vertexMetaVertex != null)
                    return new EasyEdge(this, vertexMetaVertex.To, null);

                return null;
            }
        }

        public virtual INoInEdgeInOutVertexVertex Execute(IExecution exe)
        {
            throw new NotImplementedException();
        }

        public virtual void Destroy()
        {
            throw new NotImplementedException();
        }

        public bool CanFireChangeEvent = true;

        /*public virtual void FireChange(VertexChangeEventArgs e) {
            if (Change != null && CanFireChangeEvent)
                Change(this, e);
        }*/

        public virtual object Identifier
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual object Value
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual IList<IEdge> InEdgesRaw
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IList<IEdge> OutEdges
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IList<IEdge> OutEdgesRaw
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IList<IEdge> MetaInEdgesRaw
        {
            get { throw new NotImplementedException(); }
        }

        protected virtual IVertex CreateVertexInstance()
        {
            return (IVertex)Activator.CreateInstance(this.GetType(), new object[] { this.Store });
        }

        protected virtual void InitializeNewVertexValue(
            IVertex vertex,
            object value)
        {
            vertex.Value = value;
        }

        internal IVertex CreateVertexInstanceForCopy(object value)
        {
            IVertex vertex = CreateVertexInstance();

            if (value != null)
                InitializeNewVertexValue(
                    vertex,
                    value);

            return vertex;
        }

        public virtual IVertex AddVertex(IVertex metaVertex, object val)
        {
            return AddVertexAndReturnEdge(metaVertex, val).To;
        }

        public virtual IEdge AddVertexAndReturnEdge(IVertex metaVertex, object val)
        {
            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");

            if (val is IVertex)
                throw new Exception("Trying to add Vertex as Vertex value");

            if (val is IEdge)
                throw new Exception("Trying to add Edge as Vertex value");            

            IVertex nv = CreateVertexInstance();

            if (val != null)
                InitializeNewVertexValue(
                    nv,
                    val);

            return AddEdge(metaVertex, nv);
        }

        public virtual IEdge AddEdge(IVertex metaVertex, IVertex destVertex)
        {
            throw new NotImplementedException();
        }

        public virtual void AttachInEdge(IEdge edge)
        {
            throw new NotImplementedException();
        }

        public virtual void AttachEdge(IEdge edge)
        {
            throw new NotImplementedException();
        }

        public virtual void DetachInEdge(IEdge edge)
        {
            throw new NotImplementedException();
        }

        public virtual void DetachEdge(IEdge edge)
        {
            throw new NotImplementedException();
        }

        public virtual void AddEdgesList(IEnumerable<IEdge> edges)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteEdge(IEdge edge)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteEdgesList(IEnumerable<IEdge> edges)
        {
            throw new NotImplementedException();
        }

        public virtual IVertex Get(bool metaMode, string query)
        {
            throw new NotImplementedException();
        }

        public virtual IVertex GetAll(bool metaMode, string query)
        {
            throw new NotImplementedException();
        }

        public virtual IVertex Get(bool metaMode, IVertex expression)
        {
            throw new NotImplementedException();
        }

        public virtual IVertex GetAll(bool metaMode, IVertex expression)
        {
            throw new NotImplementedException();
        }


        public IStore _Store;

        public virtual IStore Store
        {
            get { return _Store; }
        }

        public VertexBase(IStore _Store)
        {            
            this._Store = _Store;
        }        

        public override string ToString()
        {
            if (Value == null)
                return null;
            else
                return Value.ToString();
        }
                
        IEnumerator<IEdge> IEnumerable<IEdge>.GetEnumerator()
        {
            return OutEdges.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)OutEdges).GetEnumerator();
        }

        public virtual void QueryOutEdges(object meta, object to, out IEdge result, out IList<IEdge> results)
        {
            throw new NotImplementedException();
        }

        public virtual void QueryInEdges(object meta, object from, out IEdge result, out IList<IEdge> results)
        {
            throw new NotImplementedException();
        }
        
        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        public virtual void ExecuteSecondStageCommitAction()
        {
            throw new NotImplementedException();
        }

        private int externalReferenceCount;

        public int ExternalReferenceCount
        {
            get { return Volatile.Read(ref externalReferenceCount); }
        }

        public void AddExternalReference()
        {
            Interlocked.Increment(ref externalReferenceCount);
        }

        public void RemoveExternalReference()
        {
            int remainingReferences =
                Interlocked.Decrement(ref externalReferenceCount);

            if (remainingReferences < 0)
            {
                Interlocked.Increment(ref externalReferenceCount);
                throw new InvalidOperationException(
                    "External reference count cannot be negative.");
            }

            if (remainingReferences == 0)
                CheckIfShouldDispose();
        }


        protected bool _HasOnlyNonTransactedRootVertexEventsEdge;
        public bool HasOnlyNonTransactedRootVertexEventsEdge { get {
                if (HasOnlyNonTransactedRootVertexEventsEdgeNeedsRebuild)
                {
                    HasOnlyNonTransactedRootVertexEventsEdgeRebuild();

                    HasOnlyNonTransactedRootVertexEventsEdgeNeedsRebuild = false;
                }
                return _HasOnlyNonTransactedRootVertexEventsEdge;
            }
        }

        protected bool HasOnlyNonTransactedRootVertexEventsEdgeNeedsRebuild = false;

        protected virtual void HasOnlyNonTransactedRootVertexEventsEdgeRebuild()
        {
            _HasOnlyNonTransactedRootVertexEventsEdge = false;

            foreach (IEdge triggerEdge in GraphUtil.GetQueryOut(this, "$GraphChangeTrigger", null))
                if (GraphUtil.ExistQueryOut(triggerEdge.To, "ChangeTypeFilter", "OnlyNonTransactedRootVertexEvents") &&
                    GraphUtil.ExistQueryOut(triggerEdge.To, "Listener", null))
                    _HasOnlyNonTransactedRootVertexEventsEdge = true;
        }

        public virtual void CheckIfShouldDispose()
        {

        }

        public IDictionary<object, object> GetOutOdgesByMeta()
        {
            throw new NotImplementedException();
        }
    }
}
