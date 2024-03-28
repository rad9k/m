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

        public override void OnAdd(IEdge item)
        {
            if (!edgeDictionaries.NoInEdgeInOutVertexVertexMode)
            {
                if (item.Meta != null)
                    item.Meta.MetaInEdgesRaw.Add(item);                

                if (item.To != null)
                    item.To.InEdgesRaw.Add(item);                
            }

            edgeDictionaries.Vertex.OutEdgesDictionariesNeedsRebuild = true;

            edgeDictionaries.Vertex.InheritChildsDictionariesNeedsRebuild(false);

            //edgeDictionaries.vertex.FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeAdded, item));

            GraphUtil.Debug(item.From, GraphUtil.DebugOperationEnum.OutEdgeAdd);
            GraphUtil.Debug(item.To, GraphUtil.DebugOperationEnum.InEdgeAdd);
        }

        public override void OnRemove(IEdge item)
        {
            if (item.EdgeRemovalExecuting == false && !edgeDictionaries.NoInEdgeInOutVertexVertexMode)
            {
                item.EdgeRemovalExecuting = true;

                if (item.Meta != null)
                    item.Meta.MetaInEdgesRaw.Remove(item);
                
                item.To.InEdgesRaw.Remove(item);

                item.EdgeRemovalExecuting = false;

                GraphUtil.Debug(item.From, GraphUtil.DebugOperationEnum.OutEdgeRemove);
                GraphUtil.Debug(item.To, GraphUtil.DebugOperationEnum.InEdgeRemove);
            }

            edgeDictionaries.Vertex.OutEdgesDictionariesNeedsRebuild = true;
            edgeDictionaries.Vertex.InheritChildsDictionariesNeedsRebuild(false);

            edgeDictionaries.Vertex.DetachEdge(item);
            item.To.DetachInEdge(item);

            //

            //edgeDictionaries.vertex.FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, item));
        }
    }
}
