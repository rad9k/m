using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroUML.Instructions
{
    public class ZeroUMLInstructionHelpers
    {
        public static IVertex AddInstance(IVertex baseVertex, IVertex metaVertex)
        {
            IVertex nv;

            if (baseVertex == null)
                return null;

            nv = baseVertex.AddVertex(metaVertex, null);
            

            if (MinusZero.Instance.Root.Store.DetachState == DetachStateEnum.Attached)
                nv.AddEdge(MinusZero.Instance.Is, metaVertex);

            ///

            if (GraphUtil.ExistQueryOut(metaVertex, "$IsAggregation", null))
                //if (metaVertex.Get(false, "$IsAggregation:") != null)
                nv.AddEdge(MinusZero.Instance.IsAggregation, MinusZero.Instance.Empty);

            ///

            //IVertex children = metaVertex.GetAll(false, "{$MinCardinality:1}"); 

            IVertex children = metaVertex; // can use VertexOperations.GetChildEdges, but $DefaultValue: should be OK

            foreach (IEdge child in children)
            {
                if (GraphUtil.ExistQueryOut(child.To, "$DefaultValue", null))
                    //if (child.To.Get(false, "$DefaultValue:")!=null)
                    nv.AddEdge(child.To, GraphUtil.GetQueryOutFirst(child.To, "$DefaultValue", null));
                //nv.AddEdge(child.To, child.To.Get(false, "$DefaultValue:"));
                //       else
                //         nv.AddVertex(child.To, null);
            }

            return nv;
        }
    }
}
