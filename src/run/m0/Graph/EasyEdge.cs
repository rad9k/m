using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;

namespace m0.Graph
{
    [Serializable]
    public class EasyEdge : EdgeBase, IDetachableEdge
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

            To.InEdgesRaw.Remove(this);

            if (_meta != null)
                Meta.MetaInEdgesRaw.Remove(this);

            //_to = null; // BELOW
           
            //_meta = null;

            From.DetachEdge(this); // is it ok????? not sure if will not break something
            To.DetachInEdge(this);

            _to = null;

            _meta = null;

            _DetachState = DetachStateEnum.Detached;
        }

        public void Attach()
        {
            if (DetachState != DetachStateEnum.Detached)
                throw new Exception("Edge not in Detached state");

            IStore targetStore = MinusZero.Instance.GetStore(ToStoreTypeName, ToStoreIdentifier);

            if (targetStore == null)
                throw new Exception(ToStoreIdentifier + " store not found");

            IVertex targetVertex = targetStore.GetVertexByIdentifier(ToIdentifier);

            if (targetVertex == null)
                throw new Exception(ToIdentifier + " target vertex not found");

            if (targetVertex.DisposedState != DisposeStateEnum.Live)
                throw new Exception(ToIdentifier + " target vertex not live");

            IStore metaStore = MinusZero.Instance.GetStore(MetaStoreTypeName, MetaStoreIdentifier);

            if (metaStore == null)
                throw new Exception(MetaStoreIdentifier + " store not found");

            IVertex metaVertex = metaStore.GetVertexByIdentifier(MetaIdentifier);

            if (metaVertex == null)
                throw new Exception(MetaIdentifier + " meta vertex not found");

            if (metaVertex.DisposedState != DisposeStateEnum.Live)
                throw new Exception(MetaIdentifier + " meta vertex not live");

            bool targetReverseEdgeAttachmentStarted = false;
            bool metaReverseEdgeAttachmentStarted = false;
            bool sourceHookAttachmentStarted = false;
            bool targetHookAttachmentStarted = false;

            _to = targetVertex;
            _meta = metaVertex;

            try
            {
                targetReverseEdgeAttachmentStarted = true;
                targetVertex.InEdgesRaw.Add(this);

                metaReverseEdgeAttachmentStarted = true;
                metaVertex.MetaInEdgesRaw.Add(this);

                sourceHookAttachmentStarted = true;
                From.AttachEdge(this);

                targetHookAttachmentStarted = true;
                targetVertex.AttachInEdge(this);

                _DetachState = DetachStateEnum.Attached;
            }
            catch
            {
                bool previousEdgeRemovalExecuting = EdgeRemovalExecuting;
                EdgeRemovalExecuting = true;

                try
                {
                    if (targetHookAttachmentStarted)
                        targetVertex.DetachInEdge(this);

                    if (sourceHookAttachmentStarted)
                        From.DetachEdge(this);

                    if (metaReverseEdgeAttachmentStarted &&
                        metaVertex.MetaInEdgesRaw.Contains(this))
                        metaVertex.MetaInEdgesRaw.Remove(this);

                    if (targetReverseEdgeAttachmentStarted &&
                        targetVertex.InEdgesRaw.Contains(this))
                        targetVertex.InEdgesRaw.Remove(this);
                }
                finally
                {
                    EdgeRemovalExecuting = previousEdgeRemovalExecuting;
                    _to = null;
                    _meta = null;
                    _DetachState = DetachStateEnum.Detached;
                }

                throw;
            }
        }      

    }
}
