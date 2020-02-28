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

        public static void MoveEdgesIntoVertex(IVertex source, IVertex target)
        {

        }

        public static void MoveEdgesIntoVertex_SkipLinkInfo(IVertex source, IVertex target)
        {
            IList<IVertex> sourceGraph_Flat = GraphUtil.GetSubGraphWithLinksAsListButExcludeRoot(source);

            _MoveEdgesIntoVertex(source, target, sourceGraph_Flat);
        }


        public static void MoveEdgesIntoVertex_IncludeEverythingBesidesList_NoLocalMeta(IVertex source, IVertex target, IList<IVertex> vertexesToLink)
        {

        }

        public static void MoveEdgesIntoVertex_IncludeEverythingBesidesList(IVertex source, IVertex target, IList<IVertex> vertexesToLink)
        {

        }

        private static void _MoveEdgesIntoVertex(IVertex source, IVertex target, IList<IVertex> sourceGraph_Flat)
        {
            Dictionary<IVertex, IVertex> source2targetDictionary = new Dictionary<IVertex, IVertex>();
            List<IEdge> toDeleteEdges = new List<IEdge>();

            IVertex tempRoot = target;

            foreach(IVertex v in sourceGraph_Flat)
            {
                IEdge e = tempRoot.AddVertex
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
