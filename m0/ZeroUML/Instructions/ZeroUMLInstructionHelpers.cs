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

        public static void MoveEdgesIntoVertex_IncludeLinkedVertexes(IVertex source, IVertex target)
        {
            IList<IVertex> sourceGraph_Flat = GraphUtil.GetSubGraphWithLinksAsListButExcludeRoot(source);

            sourceGraph_Flat = RemoveAlwaysPresent(sourceGraph_Flat);

            _MoveEdgesIntoVertex(source, target, sourceGraph_Flat);
        }

        static IList<IVertex> RemoveAlwaysPresent(IList<IVertex> vertexList)
        {
            IList<IVertex> afterRemoval = new List<IVertex>();

            foreach (IVertex v in vertexList)
                if (!v.Store.AlwaysPresent)
                    afterRemoval.Add(v);

            return afterRemoval;
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

            foreach(IVertex sourceVertex in sourceGraph_Flat) // create new vertexes (copy)               
            {                
                IEdge e = tempRoot.AddVertexAndReturnEdge(null, sourceVertex.Value);

                source2targetDictionary.Add(sourceVertex, e.To);

                toDeleteEdges.Add(e);
            }

            foreach(IVertex sourceVertex in sourceGraph_Flat) // create new edges                
            {
                IVertex targetFrom = source2targetDictionary[sourceVertex];

                foreach(IEdge sourceEdge in sourceVertex)
                {
                    IVertex targetMeta, targetTo;

                    if (source2targetDictionary.ContainsKey(sourceEdge.Meta))
                        targetMeta = source2targetDictionary[sourceEdge.Meta];
                    else
                        targetMeta = sourceEdge.Meta;

                    if (source2targetDictionary.ContainsKey(sourceEdge.To))
                        targetTo = source2targetDictionary[sourceEdge.To];
                    else
                        targetTo = sourceEdge.To;

                    targetFrom.AddEdge(targetMeta, targetTo);
                }
            }

            foreach (IVertex targetVertex in source2targetDictionary.Values)
                foreach (IEdge metaInEdge in targetVertex.MetaInEdgesRaw.ToList())
                    if (!sourceGraph_Flat.Contains(metaInEdge.From))
                    {
                        IVertex outsideFrom = metaInEdge.From;
                        IVertex targetMeta;
                        IVertex targetTo;

                        if (source2targetDictionary.ContainsKey(metaInEdge.Meta))
                            targetMeta = source2targetDictionary[metaInEdge.Meta];
                        else
                            targetMeta = metaInEdge.Meta;

                        if (source2targetDictionary.ContainsKey(metaInEdge.To))
                            targetTo = source2targetDictionary[metaInEdge.To];
                        else
                            targetTo = metaInEdge.To;

                        outsideFrom.AddEdge(targetMeta, targetTo);
                    }
                                    

            foreach (IEdge e in toDeleteEdges) // delete rest
                e.From.DeleteEdge(e);
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
