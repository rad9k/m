using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;

namespace m0.Graph
{
    [Serializable]
    public class EasyEdge:EdgeBase, IDetachableEdge
    {
        public EasyEdge(IVertex From, IVertex Meta, IVertex To)
            : base(From, Meta, To)
        {
        }

        protected string ToIdentifier, ToStoreIdentifier, ToStoreTypeName;
        protected string MetaIdentifier, MetaStoreIdentifier, MetaStoreTypeName;

        protected DetachStateEnum _detachState;

        public DetachStateEnum DetachState { get { return _detachState; } }

        public void Detach()
        {
            ToStoreIdentifier = To.Store.Identifier;

            ToStoreTypeName = To.Store.TypeName;

            ToIdentifier = To.Identifier;

            To.DeleteInEdge(this);

            _to = null;

            if (Meta != null)
            {
                MetaStoreIdentifier = Meta.Store.Identifier;

                MetaStoreTypeName = Meta.Store.TypeName;

                MetaIdentifier = Meta.Identifier;
            }

            _meta = null;

            _detachState = DetachStateEnum.Detached;
        }

        public void Attach()
        {
            if (DetachState != DetachStateEnum.Detached)
                throw new Exception("Edge not in Detached state");

            // to

            IStore store = From.Store.StoreUniverse.GetStore(ToStoreTypeName, ToStoreIdentifier);

            if (store == null)
                throw new Exception(ToStoreIdentifier + " store not found");

            _to = store.GetVertexByIdentifier(ToIdentifier);

            To.AddInEdge(this);

            // meta

            store = From.Store.StoreUniverse.GetStore(MetaStoreTypeName, MetaStoreIdentifier);

            if (store == null)
                throw new Exception(MetaStoreIdentifier + " store not found");

            _meta = store.GetVertexByIdentifier(MetaIdentifier);
            

            _detachState = DetachStateEnum.Attached;
        }
    }
}
