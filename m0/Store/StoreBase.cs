using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0.Foundation;
using m0.Util;
using m0.Graph;

namespace m0.Store
{
    public class StoreBase:IStore
    {
        protected IStoreUniverse _StoreUniverse;

        public virtual IStoreUniverse StoreUniverse
        {
            get { return _StoreUniverse; }
        }
        
        public virtual string TypeName
        {
            get { return GeneralUtil.GetTypeName(this); }
        }

        protected string _Identifier;

        public virtual string Identifier
        {
            get { return _Identifier; }
        }

        protected IVertex _root;

        public virtual IVertex Root
        {
            get { return _root; }
        }

        public void RefreshPre()
        {
            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {
                foreach (IEdge e in v.OutEdges)
                    if (e.To.Store.DetachState != DetachStateEnum.InDetached)
                        e.To.Store.InDetach(this);
            }
        }

        public void RefreshPost()
        {
            Attach();
        }

        public virtual void Refresh() { }

        public virtual void BeginTransaction() { }

        public virtual void RollbackTransaction() { }

        public virtual void CommitTransaction() { }

        public virtual void Detach()
        {
            if(DetachState!=DetachStateEnum.Attached)
                throw new Exception("Store not Attached");

            _DetachState = DetachStateEnum.Detaching;

            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {
                foreach (IEdge e in v.OutEdges)
                    if (e is IDetachableEdge)
                    {
                        IDetachableEdge de = (IDetachableEdge)e;
                        if (de.To.Store!=this||(de.Meta!=null&&de.Meta.Store!=this))
                            de.Detach();
                    }
            }

            _DetachState = DetachStateEnum.Detached;
        }

        public virtual void InDetach(IStore InDetachStore)
        {
            _DetachState = DetachStateEnum.InDetaching;          

            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {
                foreach (IEdge e in v.InEdges)
                    if (e.From.Store == InDetachStore)
                        v.DeleteInEdge(e);
            }

            _DetachState = DetachStateEnum.InDetached;
        }

        public virtual void Attach()
        {
            if (_DetachState == DetachStateEnum.InDetached)
            {
                _DetachState = DetachStateEnum.Attached;
                return;
            }
            else
                _DetachState = DetachStateEnum.Attaching;

            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {
               foreach (IEdge e in v.OutEdges)
                    if (e is IDetachableEdge)
                    {
                        IDetachableEdge de = (IDetachableEdge)e;
                        if (de.DetachState == DetachStateEnum.Detached)
                            de.Attach();

                        if (de.To.Store.DetachState == DetachStateEnum.InDetached)
                            de.To.Store.Attach();
                    }
                        
            }

            _DetachState = DetachStateEnum.Attached;
        }

        public virtual void Close() { }

        DetachStateEnum _DetachState;

        public virtual DetachStateEnum DetachState
        {
            get { return _DetachState; }
        }

        IList<AccessLevelEnum> _AcessLevel;

        public virtual IList<AccessLevelEnum> AccessLevel
        {
            get { return _AcessLevel; }
        }

        public StoreBase(string identifier, IStoreUniverse storeUniverse, AccessLevelEnum[] accessLeveList)
        {
            _Identifier = identifier;

            _StoreUniverse = storeUniverse;
            
            VertexIdentifiersDictionary = new Dictionary<string, IVertex>();

            _AcessLevel = GeneralUtil.CreateAndCopyList<AccessLevelEnum>(accessLeveList);

            storeUniverse.Stores.Add(this);
        }

        protected Dictionary<string, IVertex> VertexIdentifiersDictionary;

        protected int StoreVertexIdentifierCnt = 0;
        public virtual void StoreVertexIdentifier(IVertex Vertex)
        {
            VertexIdentifiersDictionary.Add(Vertex.Identifier,Vertex);
           // MinusZero.Instance.Log(4,"StoreVertexIdentifier", this.Identifier+" "+StoreVertexIdentifierCnt.ToString());
            StoreVertexIdentifierCnt++;
        }

        public virtual void RemoveVertexIdentifier(IVertex Vertex)
        {
            VertexIdentifiersDictionary.Remove(Vertex.Identifier);
        }

        public virtual IVertex GetVertexByIdentifier(string VertexIdentifier)
        {
            return VertexIdentifiersDictionary[VertexIdentifier]; 
        }
    }
}
