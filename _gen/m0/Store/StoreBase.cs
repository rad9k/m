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
    public class StoreBase : IStore
    {
        protected IStoreUniverse _StoreUniverse;

        public virtual IStoreUniverse StoreUniverse
        {
            get { return _StoreUniverse; }
        }

        public virtual long VertexIdentifierCount { get; set; }

        public virtual string TypeName
        {
            get { return GeneralUtil.GetTypeName(this); }
        }

        protected string _Identifier;

        public virtual string Identifier
        {
            get { return _Identifier; }
        }

        protected IVertex root;

        public virtual IVertex Root
        {
            get { return root; }
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

        public virtual void StartTransaction() { }

        public virtual void RollbackTransaction() { }

        public virtual void CommitTransaction() { }

        public virtual void UpdateDetachStateData()
        {            
            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {         
                foreach (IEdge e in v.OutEdgesRaw) 
                    if (e is IDetachableEdge)
                    {
                        IDetachableEdge de = (IDetachableEdge)e;

                        if (de.To.Store != this || (de.Meta != null && de.Meta.Store != this)) // WHY NOT ALL ????????
                            de.UpdateDetachStateData();
                    }
            }
        }

        public virtual void Detach()
        {
            if (DetachState != DetachStateEnum.Attached)
                throw new Exception("Store not Attached");

            //            

            _DetachState = DetachStateEnum.Detaching;
            
            foreach (IVertex v in VertexIdentifiersDictionary.Values)
            {                
                if(!(v is NoInEdgeInOutVertexVertex))
                //foreach (IEdge e in v.OutEdges)
                //foreach (IEdge e in v.OutEdgesRaw) // ToList was bit beeded
                foreach (IEdge e in v.OutEdgesRaw.ToList()) // ToList was bit beeded
                    if (e is IDetachableEdge)
                    {                        
                        IDetachableEdge de = (IDetachableEdge)e;

                        
                         if (de.To.Store != this || (de.Meta != null && de.Meta.Store != this)) // WHY NOT ALL ????????
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
                //foreach (IEdge e in v.InEdges)
                foreach (IEdge e in v.InEdgesRaw)
                    if (e.From.Store == InDetachStore)
                        v.InEdgesRaw.Remove(e);

                foreach (IEdge e in v.MetaInEdgesRaw) // XXX ??? I do not know what this function does but I think that there should be also this part :)
                    if (e.From.Store == InDetachStore)
                        v.MetaInEdgesRaw.Remove(e);
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
                if (!(v is NoInEdgeInOutVertexVertex))
                    //foreach (IEdge e in v.OutEdges)
                foreach (IEdge e in v.OutEdgesRaw)
                    if (e is IDetachableEdge)
                    {                                                
                        IDetachableEdge de = (IDetachableEdge)e;

                        //IVertex meta = GeneralUtil.GetVertexByStoreIdAndId(de.MetaStoreIdentifier, de.MetaIdentifier);
                        //IVertex to = GeneralUtil.GetVertexByStoreIdAndId(de.ToStoreIdentifier, de.ToIdentifier);

                        if (de.DetachState == DetachStateEnum.Detached)
                            de.Attach();

                        if (de.To.Store.DetachState == DetachStateEnum.InDetached)
                            de.To.Store.Attach();
                    }
                        
            }           

            _DetachState = DetachStateEnum.Attached;
        }

        public virtual void Close() { }

        protected DetachStateEnum _DetachState;

        public virtual DetachStateEnum DetachState
        {
            get { return _DetachState; }
        }

        IList<AccessLevelEnum> _AcessLevel;

        public virtual IList<AccessLevelEnum> AccessLevel
        {
            get { return _AcessLevel; }
        }

        protected bool alwaysPresent;

        public bool AlwaysPresent
        {
            get { return alwaysPresent; }
        }

        public Dictionary<object, IVertex> VertexIdentifiersDictionary; // XXX was protected
       
        public virtual void StoreVertexIdentifier(IVertex Vertex)
        {            
            if (!VertexIdentifiersDictionary.ContainsKey(Vertex.Identifier))
                VertexIdentifiersDictionary.Add(Vertex.Identifier,Vertex);
            else // XXX
            {
                if (VertexIdentifiersDictionary[Vertex.Identifier] != Vertex)
                {
                    int x = 0;
                }
            }
        }

        public virtual void RemoveVertexIdentifier(IVertex Vertex)
        {            
            VertexIdentifiersDictionary.Remove(Vertex.Identifier);            
        }

        public virtual IVertex GetVertexByIdentifier(object VertexIdentifier)
        {
            if (!VertexIdentifiersDictionary.ContainsKey(VertexIdentifier))
            {                
                UserInteractionUtil.ShowError("Store " + Identifier, VertexIdentifier + " not found in Vertex Identifiers Dictionary");

                return null;
            }

            return VertexIdentifiersDictionary[VertexIdentifier]; 
        }

        public object GetRootIdentifier()
        {            
            if (VertexIdentifiersDictionary.ContainsKey((long)0))
                return VertexIdentifiersDictionary[(long)0].Identifier;            

            if (VertexIdentifiersDictionary.Count == 0)
                return null;

            return VertexIdentifiersDictionary[VertexIdentifiersDictionary.Keys.ElementAt(0)].Identifier;
        }

        public virtual void Backup() { }

        public StoreBase(string identifier, IStoreUniverse storeUniverse, AccessLevelEnum[] accessLeveList)
        {
            _Identifier = identifier;

            _StoreUniverse = storeUniverse;

            VertexIdentifiersDictionary = new Dictionary<object, IVertex>();

            _AcessLevel = GeneralUtil.CreateAndCopyList<AccessLevelEnum>(accessLeveList);

            storeUniverse.Stores.Add(this);
        }
    }
}
