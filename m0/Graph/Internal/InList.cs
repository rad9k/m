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
            edgeDictionaries.vertex.InEdgesDictionariesNeedsRebuild = true;

            edgeDictionaries.vertex.InheritChildsDictionariesNeedsRebuild(true);
        }

        public override void OnRemove(IEdge item)
        {
            if (item.From.Store.DetachState != DetachStateEnum.Attached)
                return;

            if (!item.EdgeRemovalExecuting)
            {
                item.EdgeRemovalExecuting = true;

                if (item.From != null)
                    item.From.OutEdgesRaw.Remove(item);

                if (item.Meta != null)
                    item.Meta.MetaInEdgesRaw.Remove(item);

                item.EdgeRemovalExecuting = false;
            }

            //

            edgeDictionaries.vertex.InEdgesDictionariesNeedsRebuild = true;

            edgeDictionaries.vertex.InheritChildsDictionariesNeedsRebuild(true);

            //

            int cumulativeEdgesCount = 0;

            cumulativeEdgesCount += edgeDictionaries.In.Count;
            cumulativeEdgesCount += edgeDictionaries.MetaIn.Count;

            if (cumulativeEdgesCount == 0
                && edgeDictionaries.vertex.Store.DetachState == DetachStateEnum.Attached
                && !edgeDictionaries.vertex.IsRoot)
            {
                GraphUtil.Debug(edgeDictionaries.vertex);

                ExecutionFlowHelper.AddSecondStageCommitAction(edgeDictionaries.vertex);
            }
        }
    }
}
