using m0.Foundation;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.Internal
{
    public class OutList : ExtandableHashSetAsList<IEdge>
    {
        EdgeDictionaries ed;

        public OutList(EdgeDictionaries _ed)
        {
            ed = _ed;
        }

        public IEdge Get(IEdge toCheckEdge)
        {
            if (Contains(toCheckEdge))
                return toCheckEdge;
            else
                foreach (IEdge e in this)
                    if (e.From == toCheckEdge.From && e.Meta == toCheckEdge.Meta && e.To == toCheckEdge.To)
                        return e;

            return null;
        }

        public override void OnAdd(IEdge item)
        {
            if(item.Meta != null)
                item.Meta.MetaInEdgesRaw.Add(item);

            if(item.To!=null)
                item.To.InEdgesRaw.Add(item);

            ed.v.OutEdgesDictionariesNeedsRebuild = true;

            ed.v.InheritChildsDictionariesNeedsRebuild(false);
        }

        public override void OnRemove(IEdge item)
        {
            if(item.Meta != null)
                item.Meta.MetaInEdgesRaw.Remove(item);

            if(item.To != null)
                item.To.InEdgesRaw.Remove(item);

            ed.v.OutEdgesDictionariesNeedsRebuild = true;
            ed.v.InheritChildsDictionariesNeedsRebuild(false);


            if(item.Meta != null)
            {
                if (GeneralUtil.CompareStrings(item.Meta.Value, "$Inherits"))
                {
                    ed.v.InheritanceCount--;

                    if (InheritanceCount == 0)
                        HasInheritance = false;
                }
            }

            //

            int cumulativeEdgesCount = 0;

            cumulativeEdgesCount += ed.In.Count;
            cumulativeEdgesCount += ed.MetaIn.Count;

            if (cumulativeEdgesCount == 0)
                ed.v.Dispose();
        }
    }
}
