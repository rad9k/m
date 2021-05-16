using m0.Foundation;
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
        EdgeDictionaries ed;

        public InList(EdgeDictionaries _ed)
        {
            ed = _ed;
        }

        public override IEdge Get(IEdge toCheckEdge)
        {
            if (Contains(toCheckEdge))
                return toCheckEdge;
            else
                foreach (IEdge e in this)
                    if (/*e.From == toCheckEdge.From &&*/ e.Meta == toCheckEdge.Meta && e.To == toCheckEdge.To)
                        return e;

            return null;
        }

        public override void OnAdd(IEdge item)
        {
            ed.v.InEdgesDictionariesNeedsRebuild = true;

            ed.v.InheritChildsDictionariesNeedsRebuild(true);
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

            ed.v.InEdgesDictionariesNeedsRebuild = true;

            ed.v.InheritChildsDictionariesNeedsRebuild(true);

            //

            int cumulativeEdgesCount = 0;

            cumulativeEdgesCount += ed.In.Count;
            cumulativeEdgesCount += ed.MetaIn.Count;

            if (cumulativeEdgesCount == 0
                && ed.v.Store.DetachState == DetachStateEnum.Attached
                && !ed.v.IsRoot)
                ed.v.Dispose();
        }
    }
}
