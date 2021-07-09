using m0.Foundation;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.Internal
{
    public class OutList : ExtandableList<IEdge>
    {
        EdgeDictionaries ed;

        public OutList(EdgeDictionaries _ed)
        {
            ed = _ed;
        }

        public override IEdge Get(IEdge toCheckEdge)
        {
            if (Contains(toCheckEdge))
                return toCheckEdge;
            else
                if (ed.NoInEdgeInOutVertexVertexMode)
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
            if (!ed.NoInEdgeInOutVertexVertexMode)
            {
                if (item.Meta != null)
                    item.Meta.MetaInEdgesRaw.Add(item);

                if (item.To != null)
                    item.To.InEdgesRaw.Add(item);
            }

            ed.v.OutEdgesDictionariesNeedsRebuild = true;

            ed.v.InheritChildsDictionariesNeedsRebuild(false);

            ed.v.FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeAdded, item));
        }

        public override void OnRemove(IEdge item)
        {
            if (item.EdgeRemovalExecuting == false && !ed.NoInEdgeInOutVertexVertexMode)
            {
                item.EdgeRemovalExecuting = true;

                if (item.Meta != null)
                    item.Meta.MetaInEdgesRaw.Remove(item);
                
                item.To.InEdgesRaw.Remove(item);

                item.EdgeRemovalExecuting = false;
            }

            ed.v.OutEdgesDictionariesNeedsRebuild = true;
            ed.v.InheritChildsDictionariesNeedsRebuild(false);


            if(item.Meta != null)
            {
                if (GeneralUtil.CompareStrings(item.Meta.Value, "$Inherits"))
                {
                    ed.v.InheritanceCount--;

                    if (ed.v.InheritanceCount == 0)
                        ed.v.HasInheritance = false;
                }
            }

            //

            /*int cumulativeEdgesCount = 0;

            cumulativeEdgesCount += ed.In.Count;
            cumulativeEdgesCount += ed.MetaIn.Count;

            if (cumulativeEdgesCount == 0
                && ed.v.Store.DetachState == DetachStateEnum.Attached
                && !ed.v.IsRoot)
                ed.v.Dispose();*/ // this does not metter

            ed.v.FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, item));
        }
    }
}
