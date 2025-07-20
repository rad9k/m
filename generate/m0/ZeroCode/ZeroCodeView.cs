using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    public class ZeroCodeView
    {
        static FormalTextLanguageDictinaries dict = DictionariesForFormalTextLanguageFactory.Get(
            MinusZero.Instance.Root.Get(false, @"System\FormalTextLanguage\ZeroCode"));

        // WHO USES:
        // - Text2GraphProcessing.CodeViewProcess

        // Linear exection form => Next based execution form
        // Vertexes are moved
        static public INoInEdgeInOutVertexVertex ZeroCodeViewListener(IExecution exe)
        {
            if(exe.Stack.Get(false, @"event:\Type:MetaEdgeRemoved") != null)
            {
                IVertex from = exe.Stack.Get(false, @"event:\Edge:\From:");

                LinearExecutionForm_to_NextBasedExecutionForm_ProcessGraph(from);
            }

            return null;
        }

        static public void LinearExecutionForm_to_NextBasedExecutionForm_ProcessGraph(IVertex v)
        {
            // for generate BEG
            dict = DictionariesForFormalTextLanguageFactory.Get(MinusZero.Instance.Root.Get(false, @"System\FormalTextLanguage\ZeroCode"));
            // for generate END

            IList<IEdge> edgeMetaHavingNext = new List<IEdge>();

            foreach (IEdge e in v)
            {
                bool metaHasNextEdge = false;

                if (e.Meta != MinusZero.Instance.Empty)
                    continue;

                foreach (IEdge e_is in InstructionHelpers.GetAllIs(e.To))
                    if (dict.instructions_HasNextEdge.Contains(e_is.To))
                    {
                        metaHasNextEdge = true;
                        break;
                    }
                    else
                    {
                        if (e_is.To.Identifier is long && ((long)e_is.To.Identifier) >500)
                            MinusZero.Instance.Log(-2, "10000", e_is.To.ToString() + " " + e_is.To.Identifier);
                    }

                if (metaHasNextEdge)
                        edgeMetaHavingNext.Add(e);
            }

            IEdge addingBase = null;
            bool wasFirst = false;

            foreach (IEdge e in edgeMetaHavingNext)
            {
                if (!wasFirst)
                {
                    addingBase = e;
                    wasFirst = true;
                }
                else
                {
                    addingBase.To.AddEdge(dict.NextAtomMeta, e.To);
                    v.DeleteEdge(e);
                    addingBase = e;
                }
            }

            foreach (IEdge e in v)
                if (e.Meta.Value.ToString() != "$Is"
                    //&& e.Meta != dict.NextAtomMeta
                    && !VertexOperations.IsLink(e))
                    LinearExecutionForm_to_NextBasedExecutionForm_ProcessGraph(e.To);
        }


        /////////////////////////////////////////////////////////////////////////////

        static public IList<IEdge> LinearizeVertex(IVertex v)
        {            
            IList<IEdge> linearizedList = new List<IEdge>();

            foreach (IEdge e in v.OutEdgesRaw)
                if (e.Meta != dict.NextAtomMeta)
                    linearizedList.Add(e);

            foreach (IEdge e in v.OutEdgesRaw)
                if (e.Meta != dict.NextAtomMeta)
                    AddNextEdges(linearizedList, e.To);

            return linearizedList;
        }

        static void AddNextEdges(IList<IEdge> linearizedList, IVertex v)
        {
            foreach(IEdge e in v)
                if(e.Meta == dict.NextAtomMeta)
                {
                    //IEdge ee = new EasyEdge(e.From, MinusZero.Instance.Empty, e.To);

                    linearizedList.Add(e);                    

                    AddNextEdges(linearizedList, e.To);
                }
        }

        // WHO USES:
        // - Graph2TextProcessing.prepareBaseEdge

        // Next based execution form => Linear exection form
        // Vertexes are moved?

        static public IVertex NextBasedExecutionForm_to_LinearExecutionForm_ProcessGraph(IVertex sourceBaseVertex) // for future use cases ming return pairDict also (as a ref)
        {
            IDictionary<IVertex, IVertex> sourceLinerizedDict = new Dictionary<IVertex, IVertex>();
            IList<IVertex> beenList = new List<IVertex>();

            IEnumerable<IVertex> subGraph = GraphUtil.GetSubGraphWithoutLinksAsList(sourceBaseVertex);

            foreach (IVertex v in subGraph) {
                IVertex v_new = MinusZero.Instance.CreateTempVertex();

                v_new.Value = v.Value;

                sourceLinerizedDict.Add(v, v_new);
            }
            

            return LinearizeGraph_Reccurent(sourceBaseVertex, sourceLinerizedDict, beenList);
        }

        static IVertex LinearizeGraph_Reccurent(IVertex sourceVertex, IDictionary<IVertex, IVertex> sourceLinerizedDict, IList<IVertex> beenList)
        {
            if (!sourceLinerizedDict.ContainsKey(sourceVertex))
                return sourceVertex;

            if (beenList.Contains(sourceVertex))
                return sourceVertex;

            beenList.Add(sourceVertex);

            IVertex linearizedVertex = sourceLinerizedDict[sourceVertex];

            foreach(IEdge e in LinearizeVertex(sourceVertex))
            {
                IVertex linearizedMeta = null;

                if (e.Meta == dict.NextAtomMeta)
                    linearizedMeta = MinusZero.Instance.Empty;
                else
                {
                    if (sourceLinerizedDict.ContainsKey(e.Meta))
                        linearizedMeta = sourceLinerizedDict[e.Meta];
                    else
                        linearizedMeta = e.Meta;
                }

                IVertex linearizedTo;

                if (sourceLinerizedDict.ContainsKey(e.To))
                    linearizedTo = sourceLinerizedDict[e.To];
                else
                    linearizedTo = e.To;

               if (VertexOperations.CanCopyMeta(linearizedMeta))
                    linearizedVertex.AddEdge(linearizedMeta, linearizedTo);

                LinearizeGraph_Reccurent(e.To, sourceLinerizedDict, beenList);
            }

            return linearizedVertex;
        }

        /////////////////////////////////////////////////////////////////////////////

        static public void GraphDebug(IVertex v, string fileName)
        {
            StringBuilder file = new StringBuilder();

            file.Append(v);

            GraphDebug_reccurent(1, v, file);

            File.WriteAllText(fileName, file.ToString());
        }

        static void GraphDebug_reccurent(int level, IVertex v, StringBuilder file)
        {
            string pre = "";

            for (int x = 0; x < level; x++)
                pre += "    ";
            
            foreach (IEdge e in v)
            {
                file.Append("\r\n" + pre + e.Meta + " : " + e.To);

                if(!VertexOperations.IsLink(e))
                    GraphDebug_reccurent(level + 1, e.To, file);
            }
        }
        
    }
}
