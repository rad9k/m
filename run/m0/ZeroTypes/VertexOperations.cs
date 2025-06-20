using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace m0.ZeroTypes
{
    public class VertexOperations
    {
        public static bool CanCopyEdge(IEdge e)
        {
            if (e.Meta.Value.ToString() == "$GraphChangeTrigger")
                return false;

            return true;
        }

        public static bool CanCopyMeta(IVertex v)
        {
            if (v.Value.ToString() == "$GraphChangeTrigger")
                return false;

            return true;
        }

        public static bool IsLink(IVertex e_Meta)
        {
            if (GeneralUtil.CompareStrings(e_Meta.Value, "$EdgeTarget"))
                return true;

            if (GraphUtil.ExistQueryOut(e_Meta, "$EdgeTarget", null) && !GraphUtil.ExistQueryOut(e_Meta, "$IsAggregation", null))
                return true;

            if (GraphUtil.GetQueryOutFirst(e_Meta, "$IsLink", null) != null)
                return true;

            return false;
        }

        public static bool IsLink(IEdge e)
        {
            return IsLink(e.Meta);
        }

        public static bool IsMetaAndToVertexEnoughToIdentifyEdge(IVertex baseVertex, IVertex meta, IVertex to)
        {
            if (GraphUtil.GetQueryOutCount(baseVertex, meta.Value, to.Value) > 1)
                return false;
            else
                return true;
        }

        public static bool IsToVertexEnoughToIdentifyEdge(IVertex baseVertex, IVertex to)
        {
            if (GeneralUtil.CompareStrings(to.Value, ""))
            {
                if (baseVertex.OutEdges.Count() == 1)
                    return true;
                else
                    return false;
            }

            if (GraphUtil.GetQueryOutCount(baseVertex, null, to.Value) > 1)
                return false;
            else
                return true;
        }

        public static bool IsInheritedEdge(IVertex baseVertex, IVertex metaVertex)
        {
            foreach (IEdge e in GraphUtil.GetQueryOut(baseVertex, "$Inherits",null))
                if (_IsInheritedEdge(e.To, metaVertex))
                    return true;

            return false;
        }

        private static bool _IsInheritedEdge(IVertex baseVertex, IVertex metaVertex)
        {
            if (GraphUtil.ExistQueryOut(baseVertex, metaVertex.Value, null))
                return true;

            foreach (IEdge e in GraphUtil.GetQueryOut(baseVertex, "$Inherits", null))
                if (_IsInheritedEdge(e.To, metaVertex))
                    return true;

            return false;
        }

        public static void DeleteOneEdge(IVertex source, IVertex metaVertex, IVertex toVertex)
        {
            if (source == null || toVertex == null)
                return;

            GraphUtil.DeleteEdge(source, metaVertex, toVertex);
        }        

        public static bool IsAtomicVertex(IVertex vertex)
        {
            if (vertex.OutEdges.Count() > 0)
                return false;

            return true;
        }

        public static IVertex GetChildEdges(IVertex metaVertex)
        {
            if (GraphUtil.GetQueryOutCount(metaVertex, "$Is", "Class") > 0)
                return metaVertex.GetAll(false, "{$Inherits:Selector}:");

            IVertex edgeTarget = GraphUtil.GetQueryOutFirst(metaVertex, "$EdgeTarget", null);

            if (edgeTarget != null && edgeTarget != metaVertex)
                return GetChildEdges(edgeTarget);

            IVertex ret = m0.MinusZero.Instance.CreateTempVertex();

            foreach (IEdge e in metaVertex)
            {
                if (GeneralUtil.CompareStrings(e.Meta, "$VertexTarget"))
                    ret.AddEdge(null, m0.MinusZero.Instance.EdgeTarget);
                else
                    if (e.To.Value != null && // && e.To.Value.ToString() != "" && (e.To.Value.ToString()[0] != '$') &&                    
                        
                        (GeneralUtil.CompareStrings(e.Meta, "$Empty") ||
                        (e.Meta.Value.ToString() != "" && e.Meta.Value.ToString()[0] != '$') ) )
                    
                    // is extanded                    // ???
                    // if (e.To.Get(false, "$VertexTarget:") != null || e.To.Get(false, "$EdgeTarget:") != null) // ???
                    ret.AddEdge(null, e.To);
            }

            return ret;
        }

        // as this is one of most important conceptual definitions, the historic version of the method. it does not support meta Vertices that creates edge+vertex
        /*        public static IVertex GetChildEdges(IVertex metaVertex)
                {                        
                    IVertex edgeTarget = metaVertex.Get(false, "$EdgeTarget:");
                    if (edgeTarget != null && edgeTarget!=metaVertex)
                        return GetChildEdges(edgeTarget);

                    IVertex ret = m0.MinusZero.Instance.CreateTempVertex();

                    foreach (IEdge e in metaVertex)
                    {
                        if(GeneralUtil.CompareStrings(e.Meta,"$VertexTarget"))
                            ret.AddEdge(null,m0.MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"));
                        else
                            //if (!GeneralUtil.CompareStrings(e.Meta, "$Is") && !GeneralUtil.CompareStrings(e.Meta, "$Inherits")) // to be extanded
                            if (GeneralUtil.CompareStrings(e.Meta, "$Empty")||((string)e.Meta.Value)[0] != '$') // is extanded                    
                                if (e.To.Get(false, "$VertexTarget:") != null || e.To.Get(false, "$EdgeTarget:") != null)
                                    ret.AddEdge(null,e.To);
                    }

                    return ret;
                }*/

        public static IVertex DoFilter(IVertex baseVertex, IVertex FilterQuery)
        {
            return baseVertex.GetAll(false, (string)FilterQuery.Value);
        }

        public static bool InheritanceCompare(IVertex baseVertex, string toCompare)
        {
            if (GeneralUtil.CompareStrings(baseVertex.Value, toCompare))
                return true;

            foreach (IEdge e in GraphUtil.GetQueryOut(baseVertex, "$Inherits", null))
                if (InheritanceCompare(e.To, toCompare))
                    return true;

            return false;
        }

        public static IVertex TestIfNewEdgeValid(IVertex baseVertex, IVertex metaVertex, IVertex toVertex)
        {
            int? MaxCardinality = GraphUtil.GetIntegerValue(GraphUtil.GetQueryOutFirst(metaVertex,"$MaxCardinality",null));

            if (MaxCardinality != -1 && MaxCardinality != null)
            {
                int cnt = 0;

                foreach (IEdge e in baseVertex)
                    if (e.Meta == metaVertex)
                        cnt++;

                if ((cnt + 1) > MaxCardinality)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();

                    v.Value = "Source vertex allready contains $MaxCardinality count of edges of desired meta.";

                    return v;
                }
            }

            int? MaxTargetCardinality = GraphUtil.GetIntegerValue(GraphUtil.GetQueryOutFirst(metaVertex, "$MaxTargetCardinality", null));

            if (MaxTargetCardinality != -1 && MaxTargetCardinality != null && toVertex!=null)
            {
                int cnt = 0;

                foreach (IEdge e in toVertex.InEdges)
                    if (e.Meta == metaVertex)
                        cnt++;

                if ((cnt + 1) > MaxCardinality)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();

                    v.Value = "Target vertex allready contains $MaxTargetCardinality count of in edges of desired meta.";

                    return v;
                }
            }

            return null;
        }

        public static IEdge AddEdgeOrVertexByMeta(IVertex baseVertex, IVertex metaVertex, IVertex toVertex, bool CreateEdgeOnly, bool ForceShowEditForm)
        {
            if (GraphUtil.ExistQueryOut(metaVertex,"$VertexTarget", null)
                && !CreateEdgeOnly
                )
            {                
                IVertex n = VertexOperations.AddInstance(baseVertex, metaVertex);

                IEdge e = new EasyEdge(baseVertex, metaVertex, n);

                n.AddEdge(MinusZero.Instance.EdgeTarget, toVertex);

                if (ForceShowEditForm == true)
                    MinusZero.Instance.UserInteraction.EditEdge(e.To);

                return e;
            }
            else
            {
                return baseVertex.AddEdge(metaVertex, toVertex); ;
            }
        }

        public static IVertex AddInstance(IVertex baseVertex, IVertex metaVertex, IVertex edgeVertex)
        {
            return AddInstanceAndReturnEdge(baseVertex, metaVertex, edgeVertex).To;
        }

        public static IEdge AddInstanceAndReturnEdge(IVertex baseVertex,IVertex metaVertex, IVertex edgeVertex)
        {
            IEdge ne = null;
            IVertex nv;

            if (baseVertex != null)
            {
                if (GraphUtil.ExistQueryOut(metaVertex, "$EmptyMetaInstance", null))
                    ne = baseVertex.AddVertexAndReturnEdge(null, null);
                else
                    ne = baseVertex.AddVertexAndReturnEdge(edgeVertex, null);
            }
            else
                ne = MinusZero.Instance.CreateTempEdge();

            if (ne == null)
                return null;

            nv = ne.To;

            if (MinusZero.Instance.Root.Store.DetachState == DetachStateEnum.Attached && !GeneralUtil.CompareStrings(metaVertex.Value, "$Empty")) // XXX WTF?????
                nv.AddEdge(MinusZero.Instance.Is, metaVertex);

            ///

            if (GraphUtil.ExistQueryOut(metaVertex,"$IsAggregation",null))
                nv.AddEdge(MinusZero.Instance.IsAggregation, MinusZero.Instance.Empty);

            ///

            IVertex children = metaVertex; // can use VertexOperations.GetChildEdges, but $DefaultValue: should be OK

            foreach (IEdge child in children)
            {
                bool canAdd = false;

                IVertex childMetaVertex = child.Meta;

                IVertex MinCardinality = GraphUtil.GetQueryOutFirst(child.To, "$MinCardinality", null);

                if (InstructionHelpers.CheckIfInherits_WRONG(childMetaVertex, "Selector"))
                {
                    if (MinCardinality != null)
                    {
                        if (GraphUtil.GetIntegerValueOr0(MinCardinality) == 1)
                            canAdd = true;
                        else
                            canAdd = false;
                    }
                    else
                        //canAdd = false; // it was true. 2025.04.17 we do not want new edges when mincardinality is not specified
                        canAdd = true; // aparently we need that as a lot of code depends on that :/
                }
                else
                {                    
                    if (MinCardinality != null && GraphUtil.GetIntegerValueOr0(MinCardinality) == 1)
                        canAdd = true;
                }

                if (canAdd)
                    if (GraphUtil.ExistQueryOut(child.To, "$DefaultValue", null))
                        nv.AddVertex(child.To, GraphUtil.GetQueryOutFirst(child.To, "$DefaultValue", null).Value);
                    else
                        nv.AddVertex(child.To, null); // ? XXX
            }

            return ne;
        }

        public static IVertex AddInstance(IVertex baseVertex, IVertex metaVertex)
        {
            return AddInstance(baseVertex, metaVertex, metaVertex);
        }

        public static IEdge AddInstanceAndReturnEdge(IVertex baseVertex, IVertex metaVertex)
        {
            return AddInstanceAndReturnEdge(baseVertex, metaVertex, metaVertex);
        }

        public static IVertex AddInstanceByEdgeVertex(IVertex baseVertex, IVertex edgeVertex) // by EdgeTarget or VertexTarget or by iself
        {
            // $EdgeTarget
            IVertex edgeVertexEdgeTarget = GraphUtil.GetQueryOutFirst(edgeVertex,"$EdgeTarget",null);

            if (edgeVertexEdgeTarget != null)
                return AddInstance(baseVertex, edgeVertexEdgeTarget, edgeVertex);

            // $VertexTarget
            IVertex edgeVertexVertexTarget = GraphUtil.GetQueryOutFirst(edgeVertex, "$VertexTarget", null);

            if (edgeVertexVertexTarget != null)
            {
                IVertex ret = AddInstance(baseVertex, edgeVertex, edgeVertex);

                return ret;
            }

            // EMPTY (edge+vertex one)
            AddInstance(baseVertex, edgeVertex, edgeVertex);

            return null;
        }
    }
}
