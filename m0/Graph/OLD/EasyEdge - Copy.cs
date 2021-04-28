/*using System;
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

        public EasyEdge(string _MetaStoreTypeName, string _MetaStoreIdentifier, object _MetaIdentifier,
            string _ToStoreTypeName, string _ToStoreIdentifier, object _ToIdentifier)
        {
            ToStoreIdentifier = _ToStoreIdentifier;
            ToStoreTypeName = _ToStoreTypeName;
            ToIdentifier = _ToIdentifier;
            MetaStoreIdentifier = _MetaStoreIdentifier;
            MetaStoreTypeName = _MetaStoreTypeName;
            MetaIdentifier = _MetaIdentifier;
        }

        public string ToStoreIdentifier { get; set; }
        public string ToStoreTypeName { get; set; }
        public object ToIdentifier { get; set; }
        public string MetaStoreIdentifier { get; set; }
        public string MetaStoreTypeName { get; set; }
        public object MetaIdentifier { get; set; }

        public DetachStateEnum _DetachState;

        public DetachStateEnum DetachState { get { return _DetachState; } }

        public void UpdateDetachStateData() {
            ToStoreIdentifier = To.Store.Identifier;

            ToStoreTypeName = To.Store.TypeName;

            ToIdentifier = To.Identifier;

            //

            if (Meta != null)
            {
                MetaStoreIdentifier = Meta.Store.Identifier;

                MetaStoreTypeName = Meta.Store.TypeName;

                MetaIdentifier = Meta.Identifier;
            }            
        }

        public void Detach()
        {
            UpdateDetachStateData();

            To.DeleteInEdgeOnlyIn(this);

            if (_meta != null)
                Meta.DeleteMetaInEdge(this);


            _to = null;
           
            _meta = null;

            _DetachState = DetachStateEnum.Detached;
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

            if (Meta != null)
                Meta.AddMetaInEdge(this);

            From.AttachEdge(this);

            _DetachState = DetachStateEnum.Attached;
        }      

    }
}
*/