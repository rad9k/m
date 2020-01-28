using m0.Foundation;
using m0.Graph;
using m0.Util;
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

        struct moveTargetAndIEdge
        {
            public IVertex moveTarget;
            public IEdge edge;
        }

        public static void MoveEdgesIntoVertex(IEnumerable<IEdge> toMoveList, IVertex moveTarget)
        {
            _MoveEdgesIntoVertex(toMoveList, moveTarget, new List<IVertex>(), false, null);
        }

        public static void MoveEdgesIntoVertex_LeaveVertexesFromList(IEnumerable<IEdge> toMoveList, IVertex moveTarget, IList<IVertex> vertexesToLink)
        {
            _MoveEdgesIntoVertex(toMoveList, moveTarget, vertexesToLink, false, null);
        }

        public static void MoveEdgesIntoVertex_SkipLinkInfo(IEnumerable<IEdge> toMoveList, IVertex moveTarget)
        {
            _MoveEdgesIntoVertex(toMoveList, moveTarget, new List<IVertex>(), true);
        }

        private static void _MoveEdgesIntoVertex(IEnumerable<IEdge> toMoveList, IVertex moveTarget, IList<IVertex> vertexesToLink, bool skipLinkInfo, string storeToIncludeIdentifier)
        {
            if (toMoveList is IVertex)
            {
                IVertex baseVertex = (IVertex)toMoveList;

                moveTarget.Value = baseVertex.Value;
            }

            Dictionary<IVertex, IVertex> oldToNewVertexDictionary = new Dictionary<IVertex, IVertex>();
            List<moveTargetAndIEdge> toProcessEdges = new List<moveTargetAndIEdge>();

            __MoveEdgesIntoVertex(toMoveList, moveTarget, vertexesToLink, oldToNewVertexDictionary, toProcessEdges, skipLinkInfo, storeToIncludeIdentifier);

            foreach (moveTargetAndIEdge mtae in toProcessEdges)
            {
                IVertex meta = mtae.edge.Meta;

                IVertex to = mtae.edge.To;

                if (oldToNewVertexDictionary.ContainsKey(meta))
                    meta = oldToNewVertexDictionary[meta];

                if (oldToNewVertexDictionary.ContainsKey(to))
                    to = oldToNewVertexDictionary[to];

                mtae.moveTarget.AddEdge(meta, to);
            }
        }        

        private static void __MoveEdgesIntoVertex(IEnumerable<IEdge> toMoveList, IVertex moveTarget, IList<IVertex> vertexToLink, Dictionary<IVertex, IVertex> oldToNewVertexDictionary, List<moveTargetAndIEdge> toProcessEdges, bool skipLinkInfo, string storeToIncludeIdentifier)
        {            
            foreach (IEdge e in toMoveList.ToArray()) { 

                bool linkRelatedSkip = false;

                linkRelatedSkip = !skipLinkInfo && VertexOperations.IsLink(e);

                if (e.To.Store.Identifier == storeToIncludeIdentifier)
                    linkRelatedSkip = false;

                if (e.To.Store.AlwaysPresent || // LINK ONLY
                    oldToNewVertexDictionary.ContainsKey(e.To) ||
                    vertexToLink.Contains(e.To) ||
                    e.To == MinusZero.Instance.root
                    || linkRelatedSkip) 
                {
                    MinusZero.Instance.Log(-2, "LINK", e.To.Value.ToString());

                    moveTargetAndIEdge mtae = new moveTargetAndIEdge();

                    mtae.moveTarget = moveTarget;
                    mtae.edge = e;

                    toProcessEdges.Add(mtae);
                }
                else
                { // FULL COPY
                    IVertex meta = e.Meta;

                    foreach (IEdge ee in e.To)
                        if (ee.To.Identifier is long && (long)ee.To.Identifier == (long)24757)
                        {
                            int x = 0;
                        }

                    if (oldToNewVertexDictionary.ContainsKey(meta))
                        meta = oldToNewVertexDictionary[meta];

                    IVertex newVertex = moveTarget.AddVertex(meta, e.To.Value);

                    oldToNewVertexDictionary.Add(e.To, newVertex);

                    MinusZero.Instance.Log(-2, "NEW", newVertex.Value.ToString() + " [" + newVertex.GetHashCode() + "]");

                    vertexToLink.Add(newVertex);

                    foreach (IEdge edgeToETo in e.To.InEdgesRaw.ToArray())
                    {
                        if (edgeToETo.From != moveTarget && edgeToETo.Meta != e.Meta) // allready done when creating newVertex
                            edgeToETo.From.AddEdge(edgeToETo.Meta, newVertex);

                        edgeToETo.From.DeleteEdge(edgeToETo);
                    }

                    foreach (IEdge edgeToETo in e.To.MetaInEdgesRaw.ToArray())
                    {
                        if (edgeToETo.To.Value.ToString() == "Drive")
                        {
                            int x = 0;
                        }

                        edgeToETo.From.AddEdge(newVertex, edgeToETo.To);

                        edgeToETo.From.DeleteEdge(edgeToETo);
                    }

                    __MoveEdgesIntoVertex(e.To, newVertex, vertexToLink, oldToNewVertexDictionary, toProcessEdges, skipLinkInfo, storeToIncludeIdentifier);
                }
            }
        }     

        /*private static void _MoveEdgesIntoVertex(IEnumerable<IEdge> toMoveList, IVertex moveTarget, IList<IVertex> vertexToLink)
        {
            foreach (IEdge e in toMoveList.ToArray())
                if(e.To.Store.AlwaysPresent || vertexToLink.Contains(e.To) || VertexOperations.IsLink(e))                
                    moveTarget.AddEdge(e.Meta, e.To); // LINK ONLY
                else {                 
                    vertexToLink.Add(e.To); // FULL COPY

                    IVertex newVertex = moveTarget.AddVertex(e.Meta, e.To.Value);

                    vertexToLink.Add(newVertex); // FULL COPY

                    foreach (IEdge edgeToETo in e.To.InEdgesRaw.ToArray())
                    {
                        edgeToETo.From.AddEdge(edgeToETo.Meta, newVertex);

                        edgeToETo.From.DeleteEdge(edgeToETo);
                    }

                    foreach (IEdge edgeToETo in e.To.MetaInEdgesRaw.ToArray())
                    {
                        edgeToETo.From.AddEdge(newVertex, edgeToETo.To);

                        edgeToETo.From.DeleteEdge(edgeToETo);
                    }

                    _MoveEdgesIntoVertex(e.To, newVertex, vertexToLink);
                }
           
        }*/
    }
}
