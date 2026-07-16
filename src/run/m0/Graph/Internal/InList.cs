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
    public class InList : ExtandableList<IEdge>
    {
        EdgeDictionaries edgeDictionaries;

        public InList(EdgeDictionaries _ed)
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
                {
                    foreach (IEdge e in this)
                        if (e.From == toCheckEdge.From && e.Meta == toCheckEdge.Meta && e.To == toCheckEdge.To)
                            return e;
                }

            return null;
        }

        public override void OnAdd(IEdge item)
        {
            edgeDictionaries.Vertex.InEdgesDictionariesNeedsRebuild = true;
        }

        public override void OnRemove(IEdge item)
        {
            bool shouldCascadeRemoval =
                item.From != null &&
                item.From.Store.DetachState == DetachStateEnum.Attached;

            if (shouldCascadeRemoval && !item.EdgeRemovalExecuting)
            {
                item.EdgeRemovalExecuting = true;

                try
                {
                    item.From.OutEdgesRaw.Remove(item);

                    if (item.Meta != null)
                        item.Meta.MetaInEdgesRaw.Remove(item);
                }
                finally
                {
                    item.EdgeRemovalExecuting = false;
                }
            }

            //

            edgeDictionaries.Vertex.InEdgesDictionariesNeedsRebuild = true;

            //

            if (shouldCascadeRemoval)
                edgeDictionaries.Vertex.CheckIfShouldDispose();

            /*int cumulativeEdgesCount = 0;

            cumulativeEdgesCount += edgeDictionaries.In.Count;
            cumulativeEdgesCount += edgeDictionaries.MetaIn.Count;

            if (cumulativeEdgesCount == 0 && edgeDictionaries.Vertex.ExternalReferenceCount == 0
                && edgeDictionaries.Vertex.Store.DetachState == DetachStateEnum.Attached
                && !edgeDictionaries.Vertex.IsRoot)
            {                
                ExecutionFlowHelper.AddSecondStageCommitAction(edgeDictionaries.Vertex);
            }*/
        }
    }
}
