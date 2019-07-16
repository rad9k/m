using m0.Foundation;
using m0.Graph;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace m0.ZeroTypes
{
    public class VertexOperations
    {
        public static bool IsLink(IEdge e)
        {
            if (GeneralUtil.CompareStrings(e.Meta.Value, "$EdgeTarget"))
                return true;

            if (e.Meta.Get(false, "$EdgeTarget:") != null && e.Meta.Get(false, "$IsAggregation:") == null)
                //||e.Meta.Get(false, "$VertexTarget") != null)
                return true;

            if (e.Meta.Get(false, "$IsLink:") != null)
                return true;

            return false;
        }
        public static bool IsMetaAndToVertexEnoughToIdentifyEdge(IVertex baseEdge, IVertex meta, IVertex to)
        {
            if (to.Value == null)
            {
                //int cnt=0;

                //foreach (IEdge e in baseEdge.OutEdgesRaw)
                 //   if (GraphUtil.GetValueAndCompareStrings(e.Meta, meta.Value.ToString()))
                  //      cnt++;

                if (baseEdge.GetAll(false, "\""+meta.Value.ToString() + "\":").Count() > 1)
               // if(cnt > 1)
                    return false;
                else
                    return true;
            }

            int count = 0;

            foreach(IEdge e in baseEdge.OutEdgesRaw)
            {
                if (GeneralUtil.CompareStrings(e.Meta.Value, meta.Value) && GeneralUtil.CompareStrings(e.To.Value, to.Value))
                    count++;

                if (count > 1)
                    return false;

            }

           // if (baseEdge.GetAll(false, "\""+meta.Value.ToString() + "\":\"" + to.Value.ToString()+ "\"").Count() > 1) // {} in the query
            //    return false;

            return true;
        }

        public static bool IsToVertexEnoughToIdentifyEdge(IVertex baseEdge, IVertex to)
        {
            if (to.Value == null)
            {
               // int cnt = baseEdge.OutEdgesRaw.Count;

                if (baseEdge.GetAll(false, "").Count() > 1)
               // if(cnt > 1)
                    return false;
                else
                    return true;
            }

            //  int cnt2 = 0;

            //   foreach (IEdge e in baseEdge.OutEdgesRaw)
            //   if (GraphUtil.GetValueAndCompareStrings(e.To, to.Value.ToString()))
            //      cnt2++;
            IVertex test = baseEdge.GetAll(false, "\"" + to.Value.ToString() + "\"");
            if (test!=null && test.Count() > 1)
          // if(cnt2 > 1)
                return false;

            return true;
        }

        public static bool IsInheritedEdge(IVertex baseVertex, IVertex metaVertex)
        {
            foreach (IEdge e in baseVertex.GetAll(false, "$Inherits:"))
                if (_IsInheritedEdge(e.To, metaVertex))
                    return true;

            return false;
        }

        private static bool _IsInheritedEdge(IVertex baseVertex, IVertex metaVertex)
        {
            if (baseVertex.Get(false, metaVertex.Value + ":") != null)
                return true;

            foreach (IEdge e in baseVertex.GetAll(false, "$Inherits:"))
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

        public static void DeleteAllInOutEdges(IVertex toVertex)
        {
            DeleteAllOutEdges(toVertex);
            DeleteAllInEdges(toVertex);
        }

        public static void DeleteAllOutEdges(IVertex toVertex)
        {
            if (toVertex == null)
                return;

            IList<IEdge> elist = GeneralUtil.CreateAndCopyList<IEdge>(toVertex.OutEdges);

            foreach (IEdge e in elist)
                e.From.DeleteEdge(e);
        }

        public static void DeleteAllInEdges(IVertex toVertex)
        {
            if (toVertex == null)
                return;

            IList<IEdge> elist = GeneralUtil.CreateAndCopyList<IEdge>(toVertex.InEdges);

            foreach (IEdge e in elist)
                e.From.DeleteEdge(e);
        }

        public static bool IsAtomicVertex(IVertex vertex)
        {
            if (vertex.OutEdges.Count() > 0)
                return false;

            return true;
        }

        public static IVertex GetChildEdges(IVertex metaVertex)
        {
            IVertex edgeTarget = metaVertex.Get(false, "$EdgeTarget:");
            if (edgeTarget != null && edgeTarget != metaVertex)
                return GetChildEdges(edgeTarget);

            IVertex ret = m0.MinusZero.Instance.CreateTempVertex();

            foreach (IEdge e in metaVertex)
            {
                if (GeneralUtil.CompareStrings(e.Meta, "$VertexTarget"))
                    ret.AddEdge(null, m0.MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"));
                else
                    if ((e.To.Value != null) && ((string)e.To.Value != "") && (((string)e.To.Value)[0] != '$') &&
                    (GeneralUtil.CompareStrings(e.Meta, "$Empty") || ((string)e.Meta.Value)[0] != '$')) // is extanded                    
                                                                                                        // if (e.To.Get(false, "$VertexTarget:") != null || e.To.Get(false, "$EdgeTarget:") != null)
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

            foreach (IEdge e in baseVertex.GetAll(false, "$Inherits:"))
                if (InheritanceCompare(e.To, toCompare))
                    return true;

            return false;
        }

        public static IVertex TestIfNewEdgeValid(IVertex baseVertex, IVertex metaVertex, IVertex toVertex)
        {
            int? MaxCardinality = GraphUtil.GetIntegerValue(metaVertex.Get(false, @"$MaxCardinality:"));

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

            int? MaxTargetCardinality = GraphUtil.GetIntegerValue(metaVertex.Get(false, @"$MaxTargetCardinality:"));

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

        public static IEdge AddEdgeOrVertexByMeta(IVertex baseVertex, IVertex metaVertex, IVertex toVertex, Point position, bool? CreateEdgeOnly, bool? ForceShowEditForm)
        {
            if (metaVertex.Get(false, @"$VertexTarget:") != null
                && (CreateEdgeOnly.HasValue == false||CreateEdgeOnly==false))
            {                
                IVertex n=VertexOperations.AddInstance(baseVertex,metaVertex);

                IEdge e = new EasyEdge(baseVertex, metaVertex, n);

                n.AddEdge(MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), toVertex);

                if(ForceShowEditForm.HasValue==false || ForceShowEditForm==true)
                    MinusZero.Instance.DefaultUserInteraction.EditDialog(e.To, position);

                return e;
            }
            else
            {
                return baseVertex.AddEdge(metaVertex, toVertex); ;
            }
        }        

        public static IVertex AddInstance(IVertex baseVertex,IVertex metaVertex, IVertex edgeVertex){

            IVertex nv;

            if (baseVertex != null)
                nv = baseVertex.AddVertex(edgeVertex, null);
            else
                nv = MinusZero.Instance.CreateTempVertex();

            if (MinusZero.Instance.Root.Store.DetachState == DetachStateEnum.Attached)
                nv.AddEdge(MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$Is"), metaVertex);

            ///

            if (metaVertex.Get(false, "$IsAggregation:") != null)
                nv.AddEdge(MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$IsAggregation"), MinusZero.Instance.Root.Get(false, @"System\Meta\Base\$Empty"));

            ///

            //IVertex children = metaVertex.GetAll(false, "{$MinCardinality:1}"); 

            IVertex children = metaVertex; // can use VertexOperations.GetChildEdges, but $DefaultValue: should be OK


            foreach (IEdge child in children)
            {                
                if(child.To.Get(false, "$DefaultValue:")!=null)
                    nv.AddEdge(child.To, child.To.Get(false, "$DefaultValue:"));
         //       else
           //         nv.AddVertex(child.To, null);
            }

            return nv;
        }

        public static IEdge AddInstanceAndReturnEdge(IVertex baseVertex, IVertex metaVertex, IVertex edgeVertex)
        {
            IVertex v = AddInstance(baseVertex, metaVertex, edgeVertex);

            IEdge edge = GraphUtil.FindEdge(baseVertex, metaVertex, v);

            return edge;
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
            IVertex edgeVertexEdgeTarget = edgeVertex.Get(false, "$EdgeTarget:");

            if (edgeVertexEdgeTarget != null)
                return AddInstance(baseVertex, edgeVertexEdgeTarget, edgeVertex);

            // $VertexTarget
            IVertex edgeVertexVertexTarget = edgeVertex.Get(false, "$VertexTarget:");

            if (edgeVertexVertexTarget != null)
            {
                IVertex ret = AddInstance(baseVertex, edgeVertex, edgeVertex);

                //   GraphUtil.CreateOrReplaceEdge(ret, MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), edgeVertexVertexTarget);
                //
                // ???? this is not working like this

                return ret;
            }

            // EMPTY (edge+vertex one)
            AddInstance(baseVertex, edgeVertex, edgeVertex);

            return null;
        }
    }
}
