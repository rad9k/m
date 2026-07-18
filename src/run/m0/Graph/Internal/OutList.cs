using m0.Foundation;
using m0.Graph.ExecutionFlow;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.Internal
{
    [Serializable]
    public class OutList : ExtandableList<IEdge>
    {
        EdgeDictionaries edgeDictionaries;

        public OutList(EdgeDictionaries _ed)
        {
            edgeDictionaries = _ed;
        }

        public override IEdge Get(IEdge toCheckEdge)
        {
            if (Contains(toCheckEdge))
                return toCheckEdge;
            else
                if (edgeDictionaries.NoInEdgeInOutVertexVertexMode)
                {
                    foreach (IEdge e in this)
                        if (e.Meta == toCheckEdge.Meta && e.To == toCheckEdge.To)
                            return e;
                }
                else                
                    foreach (IEdge e in this)
                    {                        
                        if ( (toCheckEdge.From is INoInEdgeInOutVertexVertex && e.Meta == toCheckEdge.Meta && e.To == toCheckEdge.To) ||
                        (e.From == toCheckEdge.From && e.Meta == toCheckEdge.Meta && e.To == toCheckEdge.To) )
                            return e;
                    }
                
            return null; 
        }

        internal int AddRangeOriginalStackEdges(
            IEnumerable<IEdge> edges)
        {
            if (edges == null)
                throw new ArgumentNullException(
                    nameof(edges));

            if (!edgeDictionaries
                .NoInEdgeInOutVertexVertexMode)
            {
                throw new InvalidOperationException(
                    "Original-edge batches are valid only " +
                    "for NoInEdgeInOutVertexVertex.");
            }

            int initialCount = Count;

            try
            {
                AddRangeWithoutCallbacks(edges);
            }
            finally
            {
                int addedCount = Count - initialCount;

                if (addedCount > 0)
                {
                    if (edgeDictionaries.Vertex is
                        EasyVertex easyVertex)
                    {
                        easyVertex
                            .InvalidateOutIndexesAfterBatchMutation();
                    }
                    else
                    {
                        edgeDictionaries.Vertex
                            .OutEdgesDictionariesNeedsRebuild =
                            true;
                    }

                }
            }

            return Count - initialCount;
        }

        public override void OnAdd(IEdge item)
        {
            if (!edgeDictionaries.NoInEdgeInOutVertexVertexMode)
            {
                if (item.Meta != null)
                    item.Meta.MetaInEdgesRaw.Add(item);                

                if (item.To != null)
                    item.To.InEdgesRaw.Add(item);                
            }

            if (edgeDictionaries.Vertex is EasyVertex easyVertex)
                easyVertex.HandleLocalOutEdgeMutation(
                    item,
                    true,
                    !edgeDictionaries
                        .NoInEdgeInOutVertexVertexMode);
            else
                edgeDictionaries.Vertex
                    .OutEdgesDictionariesNeedsRebuild = true;

            if (!edgeDictionaries.NoInEdgeInOutVertexVertexMode)
                edgeDictionaries.Vertex.NotifyOutEdgesChanged();

            //edgeDictionaries.vertex.FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeAdded, item));

            GraphUtil.Debug(item.From, GraphUtil.DebugOperationEnum.OutEdgeAdd);
            GraphUtil.Debug(item.To, GraphUtil.DebugOperationEnum.InEdgeAdd);
        }

        public override void OnRemove(IEdge item)
        {
            if (item.EdgeRemovalExecuting == false && !edgeDictionaries.NoInEdgeInOutVertexVertexMode)
            {
                item.EdgeRemovalExecuting = true;

                try
                {
                    if (item.Meta != null)
                        item.Meta.MetaInEdgesRaw.Remove(item);

                    if (item.To != null)
                        item.To.InEdgesRaw.Remove(item);
                }
                finally
                {
                    item.EdgeRemovalExecuting = false;
                }

                GraphUtil.Debug(item.From, GraphUtil.DebugOperationEnum.OutEdgeRemove);

                if (item.To != null)
                    GraphUtil.Debug(item.To, GraphUtil.DebugOperationEnum.InEdgeRemove);
            }

            if (edgeDictionaries.Vertex is EasyVertex easyVertex)
                easyVertex.HandleLocalOutEdgeMutation(
                    item,
                    false,
                    !edgeDictionaries
                        .NoInEdgeInOutVertexVertexMode);
            else
                edgeDictionaries.Vertex
                    .OutEdgesDictionariesNeedsRebuild = true;

            if (!edgeDictionaries.NoInEdgeInOutVertexVertexMode)
                edgeDictionaries.Vertex.NotifyOutEdgesChanged();

            edgeDictionaries.Vertex.DetachEdge(item);

            if (item.To != null)
                item.To.DetachInEdge(item);

            //

            //edgeDictionaries.vertex.FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, item));
        }
    }
}
