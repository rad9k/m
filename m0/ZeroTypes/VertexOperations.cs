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

            if (e.Meta.Get("$EdgeTarget:") != null && e.Meta.Get("$IsAggregation:") == null)
                //||e.Meta.Get("$VertexTarget") != null)
                return true;

            if (e.Meta.Get("$$IsLink:") != null)
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

                if (baseEdge.GetAll("\""+meta.Value.ToString() + "\":").Count() > 1)
               // if(cnt > 1)
                    return false;
                else
                    return true;
            }

           // int cnt2 = 0;

            //foreach (IEdge e in baseEdge.OutEdgesRaw)
            //    if (GraphUtil.GetValueAndCompareStrings(e.To, to.Value.ToString()) && GraphUtil.GetValueAndCompareStrings(e.Meta, meta.Value.ToString()))
             //       cnt2++;

            if (baseEdge.GetAll("\""+meta.Value.ToString() + "\":\"" + to.Value.ToString()+ "\"").Count() > 1)
           //if(cnt2 > 1)
                return false;

            return true;
        }

        public static bool IsToVertexEnoughToIdentifyEdge(IVertex baseEdge, IVertex to)
        {
            if (to.Value == null)
            {
               // int cnt = baseEdge.OutEdgesRaw.Count;

                if (baseEdge.GetAll("").Count() > 1)
               // if(cnt > 1)
                    return false;
                else
                    return true;
            }

            //  int cnt2 = 0;

            //   foreach (IEdge e in baseEdge.OutEdgesRaw)
            //   if (GraphUtil.GetValueAndCompareStrings(e.To, to.Value.ToString()))
            //      cnt2++;
            IVertex test = baseEdge.GetAll("\"" + to.Value.ToString() + "\"");
            if (test!=null && test.Count() > 1)
          // if(cnt2 > 1)
                return false;

            return true;
        }

        public static bool IsInheritedEdge(IVertex baseVertex, IVertex metaVertex)
        {
            foreach (IEdge e in baseVertex.GetAll("$Inherits:"))
                if (_IsInheritedEdge(e.To, metaVertex))
                    return true;

            return false;
        }

        private static bool _IsInheritedEdge(IVertex baseVertex, IVertex metaVertex)
        {
            if (baseVertex.Get(metaVertex.Value + ":") != null)
                return true;

            foreach (IEdge e in baseVertex.GetAll("$Inherits:"))
                if (_IsInheritedEdge(e.To, metaVertex))
                    return true;

            return false;
        }

        public static void DeleteOneEdge(IVertex source, IVertex metaVertex, IVertex toVertex)
        {
            GraphUtil.DeleteEdge(source, metaVertex, toVertex);
        }

        public static void DeleteAllEdges(IVertex toVertex)
        {
            IList<IEdge> elist = GeneralUtil.CreateAndCopyList<IEdge>(toVertex.OutEdges);

            foreach (IEdge e in elist)
                e.From.DeleteEdge(e);

            elist = GeneralUtil.CreateAndCopyList<IEdge>(toVertex.InEdges);

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
            IVertex edgeTarget = metaVertex.Get("$EdgeTarget:");
            if (edgeTarget != null && edgeTarget != metaVertex)
                return GetChildEdges(edgeTarget);

            IVertex ret = m0.MinusZero.Instance.CreateTempVertex();

            foreach (IEdge e in metaVertex)
            {
                if (GeneralUtil.CompareStrings(e.Meta, "$VertexTarget"))
                    ret.AddEdge(null, m0.MinusZero.Instance.Root.Get(@"System\Meta\Base\Vertex\$EdgeTarget"));
                else
                    if ((e.To.Value != null) && ((string)e.To.Value != "") && (((string)e.To.Value)[0] != '$') &&
                    (GeneralUtil.CompareStrings(e.Meta, "$Empty") || ((string)e.Meta.Value)[0] != '$')) // is extanded                    
                                                                                                        // if (e.To.Get("$VertexTarget:") != null || e.To.Get("$EdgeTarget:") != null)
                    ret.AddEdge(null, e.To);
            }

            return ret;
        }

        // as this is one of most important conceptual definitions, the historic version of the method. it does not support meta vertexes that creates edge+vertex
        /*        public static IVertex GetChildEdges(IVertex metaVertex)
                {                        
                    IVertex edgeTarget = metaVertex.Get("$EdgeTarget:");
                    if (edgeTarget != null && edgeTarget!=metaVertex)
                        return GetChildEdges(edgeTarget);

                    IVertex ret = m0.MinusZero.Instance.CreateTempVertex();

                    foreach (IEdge e in metaVertex)
                    {
                        if(GeneralUtil.CompareStrings(e.Meta,"$VertexTarget"))
                            ret.AddEdge(null,m0.MinusZero.Instance.Root.Get(@"System\Meta\Base\Vertex\$EdgeTarget"));
                        else
                            //if (!GeneralUtil.CompareStrings(e.Meta, "$Is") && !GeneralUtil.CompareStrings(e.Meta, "$Inherits")) // to be extanded
                            if (GeneralUtil.CompareStrings(e.Meta, "$Empty")||((string)e.Meta.Value)[0] != '$') // is extanded                    
                                if (e.To.Get("$VertexTarget:") != null || e.To.Get("$EdgeTarget:") != null)
                                    ret.AddEdge(null,e.To);
                    }

                    return ret;
                }*/

        public static IVertex DoFilter(IVertex baseVertex, IVertex FilterQuery)
        {
            return baseVertex.GetAll((string)FilterQuery.Value);
        }

        public static bool InheritanceCompare(IVertex baseVertex, string toCompare)
        {
            if (GeneralUtil.CompareStrings(baseVertex.Value, toCompare))
                return true;

            foreach (IEdge e in baseVertex.GetAll("$Inherits:"))
                if (InheritanceCompare(e.To, toCompare))
                    return true;

            return false;
        }

        public static IVertex TestIfNewEdgeValid(IVertex baseVertex, IVertex metaVertex, IVertex toVertex)
        {
            int MaxCardinality = GraphUtil.GetIntegerValue(metaVertex.Get(@"$MaxCardinality:"));

            if (MaxCardinality != -1 && MaxCardinality != GraphUtil.NullInt)
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

            int MaxTargetCardinality = GraphUtil.GetIntegerValue(metaVertex.Get(@"$MaxTargetCardinality:"));

            if (MaxTargetCardinality != -1 && MaxTargetCardinality != GraphUtil.NullInt && toVertex!=null)
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
            if (metaVertex.Get(@"$VertexTarget:") != null
                && (CreateEdgeOnly.HasValue == false||CreateEdgeOnly==false))
            {                
                IVertex n=VertexOperations.AddInstance(baseVertex,metaVertex);

                IEdge e = new EasyEdge(baseVertex, metaVertex, n);

                n.AddEdge(MinusZero.Instance.Root.Get(@"System\Meta\Base\Vertex\$EdgeTarget"), toVertex);

                if(ForceShowEditForm.HasValue==false || ForceShowEditForm==true)
                    MinusZero.Instance.DefaultShow.EditDialog(e.To, position);

                return e;
            }
            else
            {
                return baseVertex.AddEdge(metaVertex, toVertex); ;
            }
        }        

        public static IVertex AddInstance(IVertex baseVertex,IVertex metaVertex, IVertex edgeVertex){

            IVertex nv = baseVertex.AddVertex(edgeVertex, null);

            nv.AddEdge(MinusZero.Instance.Root.Get(@"System\Meta\Base\Vertex\$Is"), metaVertex);

            ///

            if (metaVertex.Get("$IsAggregation:") != null)
                nv.AddVertex(MinusZero.Instance.Root.Get(@"System\Meta\Base\Vertex\$IsAggregation"), MinusZero.Instance.Root.Get(@"System\Meta\Base\$Empty"));

            ///

            //IVertex children = metaVertex.GetAll("{$MinCardinality:1}"); 

            IVertex children = metaVertex; // can use VertexOperations.GetChildEdges, but $DefaultValue: should be OK


            foreach (IEdge child in children)
            {
                if(child.To.Get("$DefaultValue:")!=null)
                    nv.AddEdge(child.To, child.To.Get("$DefaultValue:"));
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
            IVertex edgeVertexEdgeTarget = edgeVertex.Get("$EdgeTarget:");

            if (edgeVertexEdgeTarget != null)
                return AddInstance(baseVertex, edgeVertexEdgeTarget, edgeVertex);

            // $VertexTarget
            IVertex edgeVertexVertexTarget = edgeVertex.Get("$VertexTarget:");

            if (edgeVertexVertexTarget != null)
            {
                IVertex ret = AddInstance(baseVertex, edgeVertex, edgeVertex);

                //   GraphUtil.CreateOrReplaceEdge(ret, MinusZero.Instance.Root.Get(@"System\Meta\Base\Vertex\$EdgeTarget"), edgeVertexVertexTarget);
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
