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
    public class ZeroUMLInstructionHelpers_COPY
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
            _MoveEdgesIntoVertex(toMoveList, moveTarget, new List<IVertex>(), false, false);
        }

        public static void MoveEdgesIntoVertex_IncludeEverythingBesidesList(IEnumerable<IEdge> toMoveList, IVertex moveTarget, IList<IVertex> vertexesToLink)
        {
            _MoveEdgesIntoVertex(toMoveList, moveTarget, vertexesToLink, true, false);
        }

        public static void MoveEdgesIntoVertex_IncludeEverythingBesidesList_NoLocalMeta(IEnumerable<IEdge> toMoveList, IVertex moveTarget, IList<IVertex> vertexesToLink)
        {
            _MoveEdgesIntoVertex(toMoveList, moveTarget, vertexesToLink, true, true);
        }

        public static void MoveEdgesIntoVertex_SkipLinkInfo(IEnumerable<IEdge> toMoveList, IVertex moveTarget)
        {
            _MoveEdgesIntoVertex(toMoveList, moveTarget, new List<IVertex>(), true, false);
        }

        public static void MoveEdgesIntoVertex_NoLocalMeta(IEnumerable<IEdge> toMoveList, IVertex moveTarget)
        {
            _MoveEdgesIntoVertex(toMoveList, moveTarget, new List<IVertex>(), true, true);
        }

        private static IVertex _MoveEdgesIntoVertex_getMeta(IVertex meta, IVertex moveTarget, IList<IVertex> vertexesToLink, Dictionary<IVertex, IVertex> outsideMetaToLocalMetaDictionary, HashSet<IVertex> allToMoveSubGraphVertexes)
        {
            if (meta.Store.AlwaysPresent)
                return meta;

            bool badMeta = false;

            if (!allToMoveSubGraphVertexes.Contains(meta))
                badMeta = true;
            
            //= meta.Store.Identifier != moveTarget.Store.Identifier; that was wrong

            if (vertexesToLink.Count > 0 && meta.Store.Identifier == vertexesToLink[0].Store.Identifier)
                badMeta = false;

            if (badMeta) // a bit hacky                    
            {
                IVertex metaToUse = null;

                if (outsideMetaToLocalMetaDictionary.ContainsKey(meta))
                    metaToUse = outsideMetaToLocalMetaDictionary[meta];
                else
                {
                    metaToUse = moveTarget.AddVertex(null, meta.Value);
                    outsideMetaToLocalMetaDictionary.Add(meta, metaToUse);
                }
                return metaToUse;
            }
            return meta;
        }

        private static void _MoveEdgesIntoVertex(IEnumerable<IEdge> toMoveList, IVertex moveTarget, IList<IVertex> vertexesToLink, bool skipLinkInfo, bool noLocalMeta)
        {
            List<IVertex> allToMoveSubGraphVertexes;


            if (toMoveList is IVertex)
            {
                IVertex baseVertex = (IVertex)toMoveList;

                moveTarget.Value = baseVertex.Value;

                allToMoveSubGraphVertexes = GraphUtil.GetSubGraphWithoutLinksAsList(baseVertex).ToList();
            }
            else
            {
                allToMoveSubGraphVertexes = new List<IVertex>();

                foreach (IEdge e in toMoveList)
                {
                    IEnumerable<IVertex> localList = GraphUtil.GetSubGraphWithoutLinksAsList(e.To);

                    allToMoveSubGraphVertexes.AddRange(localList);
                }
            }

            Dictionary<IVertex, IVertex> oldToNewVertexDictionary = new Dictionary<IVertex, IVertex>();
            List<moveTargetAndIEdge> toProcessEdges = new List<moveTargetAndIEdge>();

            Dictionary<IVertex, IVertex> outsideMetaToLocalMetaDictionary = new Dictionary<IVertex, IVertex>();

            __MoveEdgesIntoVertex(toMoveList, moveTarget, vertexesToLink, oldToNewVertexDictionary, toProcessEdges, skipLinkInfo, outsideMetaToLocalMetaDictionary, allToMoveSubGraphVertexes, noLocalMeta);

            foreach (moveTargetAndIEdge mtae in toProcessEdges)
            {
                IVertex meta = mtae.edge.Meta;

                IVertex to = mtae.edge.To;

                if (oldToNewVertexDictionary.ContainsKey(meta))
                    meta = oldToNewVertexDictionary[meta];

                if (oldToNewVertexDictionary.ContainsKey(to))
                    to = oldToNewVertexDictionary[to];

                if (!noLocalMeta)
                    meta = _MoveEdgesIntoVertex_getMeta(meta, moveTarget, vertexesToLink, outsideMetaToLocalMetaDictionary, new HashSet<IVertex>(allToMoveSubGraphVertexes));
                    
                mtae.moveTarget.AddEdge(meta, to);
            }
        }        

        private static void __MoveEdgesIntoVertex(IEnumerable<IEdge> toMoveList, IVertex moveTarget, IList<IVertex> vertexesToLink, Dictionary<IVertex, IVertex> oldToNewVertexDictionary, List<moveTargetAndIEdge> toProcessEdges, bool skipLinkInfo, Dictionary<IVertex, IVertex> outsideMetaToLocalMetaDictionary, IList<IVertex> allToMoveSubGraphVertexes, bool noLocalMeta)
        {           
            if(toMoveList is IVertex)
            {
                IVertex toMoveList_asVertex = (IVertex)toMoveList;

                toMoveList = toMoveList_asVertex.OutEdgesRaw;
            }


            foreach (IEdge e in toMoveList.ToArray()) {                
                if (e.To.Store.AlwaysPresent || // LINK ONLY
                    oldToNewVertexDictionary.ContainsKey(e.To) ||
                    vertexesToLink.Contains(e.To) ||
                    e.To == MinusZero.Instance.root
                    || (!skipLinkInfo && VertexOperations.IsLink(e))) 
                {
                    moveTargetAndIEdge mtae = new moveTargetAndIEdge();

                    mtae.moveTarget = moveTarget;
                    mtae.edge = e;

                    toProcessEdges.Add(mtae);
                }
                else
                { // FULL COPY
                    IVertex meta = e.Meta;
                    
                    if (oldToNewVertexDictionary.ContainsKey(meta))
                        meta = oldToNewVertexDictionary[meta];

                    if (!noLocalMeta)
                        meta = _MoveEdgesIntoVertex_getMeta(meta, moveTarget, vertexesToLink, outsideMetaToLocalMetaDictionary, new HashSet<IVertex>(allToMoveSubGraphVertexes));

                    IVertex newVertex = moveTarget.AddVertex(meta, e.To.Value);

                    oldToNewVertexDictionary.Add(e.To, newVertex);                    

                    vertexesToLink.Add(newVertex);

                    foreach (IEdge edgeToETo in e.To.InEdgesRaw.ToArray())
                    {
                        bool theSame = true;

                        if (oldToNewVertexDictionary.ContainsKey(edgeToETo.From))
                        {
                            if(oldToNewVertexDictionary[edgeToETo.From] != moveTarget)
                                theSame = false;
                        }
                        else
                        {
                            if (edgeToETo.From != moveTarget)
                                theSame = false;
                        } 

                        if (edgeToETo.Meta != meta)
                            theSame = false;

                        if(!theSame)
                            edgeToETo.From.AddEdge(edgeToETo.Meta, newVertex);

                        // if (edgeToETo.From != moveTarget && edgeToETo.Meta != e.Meta) // allready done when creating newVertex
                        //if (edgeToETo.From != moveTarget || edgeToETo.Meta != meta ) // allready done when creating newVertex
                        
                        edgeToETo.From.DeleteEdge(edgeToETo);
                    }

                    foreach (IEdge edgeToETo in e.To.MetaInEdgesRaw.ToArray())
                    {                        
                        edgeToETo.From.AddEdge(newVertex, edgeToETo.To);                        

                        edgeToETo.From.DeleteEdge(edgeToETo);
                    }

                    __MoveEdgesIntoVertex(e.To, newVertex, vertexesToLink, oldToNewVertexDictionary, toProcessEdges, skipLinkInfo, outsideMetaToLocalMetaDictionary, allToMoveSubGraphVertexes, noLocalMeta);
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
