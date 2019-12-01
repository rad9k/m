using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
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
                        
            nv.AddEdge(MinusZero.Instance.Is, metaVertex);

            IVertex children = metaVertex; // can use VertexOperations.GetChildEdges, but $DefaultValue: should be OK

            foreach (IEdge child in children)
            {
                bool shouldAdd = false;

                IVertex defaultValue = GraphUtil.GetQueryOutFirst(child.To, "$DefaultValue", null);

                object value = null;

                if (defaultValue != null) {
                    shouldAdd = true;
                    value = defaultValue.Value;
                }
                else
                    value = "";

                IVertex minCardinality = GraphUtil.GetQueryOutFirst(child.To, "$MinCardinality", null);

                int minCardinalityValue;

                if (minCardinality != null)
                {
                    shouldAdd = true;
                    minCardinalityValue = (int)GraphUtil.GetIntegerValue(minCardinality);
                }
                else
                    minCardinalityValue = 1;

                if (shouldAdd)
                    for (int x = 0; x < minCardinalityValue; x++)
                        nv.AddVertex(child.To, value);
            }
                
            return nv;
        }

        public static void MoveEdgesIntoVertex(IEnumerable<IEdge> toMoveList, IVertex moveTarget)
        {
            foreach(IEdge e in toMoveList.ToArray())
                if (!VertexOperations.IsLink(e))
                {
                    IVertex newVertex = moveTarget.AddVertex(e.Meta, e.To.Value);

                    foreach(IEdge edgeToETo in e.To.InEdgesRaw.ToArray())
                    {
                        if(edgeToETo is m0.Graph.NoInEdgeInOutVertexEdge)
                        {
                            m0.Graph.NoInEdgeInOutVertexEdge no = (m0.Graph.NoInEdgeInOutVertexEdge)edgeToETo;
                        }

                        edgeToETo.From.AddEdge(edgeToETo.Meta, newVertex);
                      //  edgeToETo.From.DeleteEdge(edgeToETo);
                    }

                    //MoveEdgesIntoVertex(e.To, newVertex);
                }

        }
    }
}
