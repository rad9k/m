using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using System.Collections.Generic;
using System.Linq;

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
                if (!InstructionHelpers.CheckIfInherits_WRONG(child.Meta, "Selector"))
                    continue;

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
                    minCardinalityValue = (int)GraphUtil.GetIntegerValue(minCardinality);                
                else
                    minCardinalityValue = 1;
                
                    for (int x = 0; x < minCardinalityValue; x++)
                        nv.AddVertex(child.To, value);
            }
                
            return nv;
        }                

        public static void MoveEdgesIntoVertex(IVertex source, IVertex target)
        {
            IEnumerable<IVertex> sourceGraph_Flat = GraphUtil.GetSubGraphWithLinksAsListButExcludeRoot(source);

            sourceGraph_Flat = RemoveAlwaysPresent(sourceGraph_Flat);

            _MoveEdgesIntoVertex(source, target, sourceGraph_Flat);
        }

        public static void MoveEdgesIntoVertex_NoBootstrap(IVertex source, IVertex target)
        {
            IEnumerable<IVertex> sourceGraph_Flat = GraphUtil.GetSubGraphWithLinksAsListButExcludeList(source, new HashSet<IVertex>(MinusZero.Instance.BootstrapVertexes));

            sourceGraph_Flat = RemoveAlwaysPresent(sourceGraph_Flat);

            _MoveEdgesIntoVertex(source, target, sourceGraph_Flat);
        }

        public static void MoveEdgesIntoVertex_NoLinks(IVertex source, IVertex target)
        {
            IEnumerable<IVertex> sourceGraph_Flat = GraphUtil.GetSubGraphWithoutLinksAsList(source);

            sourceGraph_Flat = RemoveAlwaysPresent(sourceGraph_Flat);

            _MoveEdgesIntoVertex(source, target, sourceGraph_Flat);
        }

        public static void MoveEdgesIntoVertex_NoLinksNoBootstrap(IVertex source, IVertex target)
        {
            IEnumerable<IVertex> sourceGraph_Flat = GraphUtil.GetSubGraphWithoutLinksAsListButExcludeList(source, new HashSet<IVertex>(MinusZero.Instance.BootstrapVertexes));

            sourceGraph_Flat = RemoveAlwaysPresent(sourceGraph_Flat);

            _MoveEdgesIntoVertex(source, target, sourceGraph_Flat);
        }


        static IList<IVertex> RemoveAlwaysPresent(IEnumerable<IVertex> vertexList)
        {
            IList<IVertex> afterRemoval = new List<IVertex>();

            foreach (IVertex v in vertexList)
                if (!v.Store.AlwaysPresent)
                    afterRemoval.Add(v);

            return afterRemoval;
        }

        public static void MoveEdgesIntoVertex_IncludeEverythingBesidesList(IVertex source, IVertex target, HashSet<IVertex> excludeList)
        {
            IEnumerable<IVertex> sourceGraph_Flat = GraphUtil.GetSubGraphWithLinksAsListButExcludeRoot(source);

            sourceGraph_Flat = RemoveAlwaysPresent(sourceGraph_Flat);

            IList<IVertex> sourceGraph_Flat_afterRemoval = new List<IVertex>();

            foreach (IVertex v in sourceGraph_Flat)            
                if (!excludeList.Contains(v))
                    sourceGraph_Flat_afterRemoval.Add(v);            

            _MoveEdgesIntoVertex(source, target, sourceGraph_Flat_afterRemoval);
        }

        private static void _MoveEdgesIntoVertex(IVertex sourceRoot, IVertex targetRoot, IEnumerable<IVertex> sourceGraph_Flat)
        {
            Dictionary<IVertex, IVertex> source2targetDictionary = new Dictionary<IVertex, IVertex>();
            List<IEdge> toDeleteEdges = new List<IEdge>();

            IVertex tempRoot = targetRoot;

            foreach(IVertex sourceVertex in sourceGraph_Flat) // create new vertexes (copy)                           
                if (sourceVertex == sourceRoot) // root 
                {
                    targetRoot.Value = sourceVertex.Value;

                    source2targetDictionary.Add(sourceRoot, targetRoot);
                }
                else // rest
                {
                    IEdge e = tempRoot.AddVertexAndReturnEdge(null, sourceVertex.Value);
                    
                    source2targetDictionary.Add(sourceVertex, e.To);

                    toDeleteEdges.Add(e);
                }            

            foreach (IVertex sourceVertex in sourceGraph_Flat) // create new edges                
            {
                IVertex targetFrom = source2targetDictionary[sourceVertex];

                foreach (IEdge sourceEdge in sourceVertex.OutEdgesRaw)
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

            foreach (IVertex sourceVertex in sourceGraph_Flat) // replace old edges with new vertexes
                foreach (IEdge sourceInEdge in sourceVertex.InEdgesRaw.ToList())
                    if(!source2targetDictionary.ContainsKey(sourceInEdge.From)) // if the edge comes from outside
                    {
                        IVertex sourceFrom = sourceInEdge.From;
                        IVertex targetMeta;

                        if (source2targetDictionary.ContainsKey(sourceInEdge.Meta))
                            targetMeta = source2targetDictionary[sourceInEdge.Meta];
                        else
                            targetMeta = sourceInEdge.Meta;

                        IVertex targetTo = source2targetDictionary[sourceVertex];

                        // this is done below but with proper order
                        //sourceFrom.AddEdge(targetMeta, targetTo); 
                        //sourceFrom.DeleteEdge(sourceInEdge);

                        IList<IEdge> sourceFromEdges = new List<IEdge>();

                        foreach (IEdge sourceFromEdge in sourceFrom.OutEdgesRaw)
                            sourceFromEdges.Add(sourceFromEdge);

                        //sourceFrom.DeleteEdgesList(sourceFromEdges);

                        foreach(IEdge sourceFromEdgeToAdd in sourceFromEdges)
                        {
                            if (GraphUtil.CompareEdges(sourceFromEdgeToAdd, sourceInEdge))
                                sourceFrom.AddEdge(targetMeta, targetTo);
                            else
                                sourceFrom.AddEdge(sourceFromEdgeToAdd.Meta, sourceFromEdgeToAdd.To);
                        }

                        sourceFrom.DeleteEdgesList(sourceFromEdges); // MOVED
                    }

            foreach (IVertex sourceVertex in sourceGraph_Flat) // META replace old edges with new vertexes
                foreach (IEdge metaInEdge in sourceVertex.MetaInEdgesRaw.ToList())
                    if (!source2targetDictionary.ContainsKey(metaInEdge.From)) // if the edge comes from outside                        
                    {
                        IVertex sourceFrom = metaInEdge.From;
                        IVertex targetMeta, targetTo;

                        if (source2targetDictionary.ContainsKey(metaInEdge.Meta))
                            targetMeta = source2targetDictionary[metaInEdge.Meta];
                        else
                            targetMeta = metaInEdge.Meta;

                        if (source2targetDictionary.ContainsKey(metaInEdge.To))
                            targetTo = source2targetDictionary[metaInEdge.To];
                        else
                            targetTo = metaInEdge.To;

                        // this is done below but with proper order
                        // sourceFrom.AddEdge(targetMeta, targetTo);
                        // sourceFrom.DeleteEdge(metaInEdge);

                        IList<IEdge> sourceFromEdges = new List<IEdge>();

                        foreach (IEdge sourceFromEdge in sourceFrom.OutEdgesRaw)
                            sourceFromEdges.Add(sourceFromEdge);

                        //sourceFrom.DeleteEdgesList(sourceFromEdges);

                        foreach (IEdge sourceFromEdgeToAdd in sourceFromEdges)
                        {
                            if (GraphUtil.CompareEdges(sourceFromEdgeToAdd, metaInEdge))
                                sourceFrom.AddEdge(targetMeta, targetTo);
                            else
                                sourceFrom.AddEdge(sourceFromEdgeToAdd.Meta, sourceFromEdgeToAdd.To);
                        }

                        sourceFrom.DeleteEdgesList(sourceFromEdges); // MOVED
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
