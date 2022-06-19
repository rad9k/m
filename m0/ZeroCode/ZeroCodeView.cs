using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    public class ZeroCodeView
    {
        static DictionariesForFormalTextLanguage dict = DictionariesForFormalTextLanguageFactory.Get(
            MinusZero.Instance.Root.Get(false, @"System\FormalTextLanguage\ZeroCode"));

        static public INoInEdgeInOutVertexVertex ZeroCodeViewListener(IExecution exe)
        {
            if(exe.Stack.Get(false, @"event:\Type:MetaEdgeRemoved") != null)
            {
                IVertex from = exe.Stack.Get(false, @"event:\Edge:\From:");

                ProcessVertex(from);
            }

            return null;
        }

        static void ProcessVertex(IVertex v)
        {
            IList<IEdge> edgeMetaHavingNext = new List<IEdge>();

            foreach (IEdge e in v)
            {
                bool metaHasNextEdge = false;

                if (e.Meta != MinusZero.Instance.Empty)
                    continue;

                foreach (IEdge e_is in InstructionHelpers.GetAllIs(e.To))
                    if (dict.instructions_HasNextEdge.Contains(e_is.To))
                        metaHasNextEdge = true;
                
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
                    ProcessVertex(e.To);
        }        
        
        static public IList<IEdge> LinearizeVertex(IVertex v)
        {            
            IList<IEdge> linearizedList = new List<IEdge>();

            foreach (IEdge e in v)
                if (e.Meta != dict.NextAtomMeta)
                    linearizedList.Add(e);

            foreach (IEdge e in v)
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

        static public IVertex LinearizeGraph(IVertex sourceBaseVertex) // for future use cases ming return pairDict also (as a ref)
        {            
            IList<IVertex> subGraph = GraphUtil.GetSubGraphWithoutLinksAsList(sourceBaseVertex);

            IDictionary<IVertex, IVertex> sourceLinerizedDict = new Dictionary<IVertex, IVertex>();

            foreach (IVertex v in subGraph) {
                IVertex v_new = MinusZero.Instance.CreateTempVertex();

                v_new.Value = v.Value;

                sourceLinerizedDict.Add(v, v_new);
            }

            return LinearizeGraph_Reccurent()            
        }

        static IVertex LinearizeGraph_Reccurent(IVertex sourceVertex, IDictionary<IVertex, IVertex> sourceLinerizedDict)
        {
            IVertex linearizedVertex = sourceLinerizedDict[sourceVertex];

            foreach(IEdge e in LinearizeVertex(sourceVertex))
            {
                IVertex linearizedMeta = null;

                if (e.Meta == dict.NextAtomMeta)
                    linearizedMeta = MinusZero.Instance.Empty;
                else
                    linearizedMeta = e.Meta;

                IVertex linearizedTo = sourceLinerizedDict[e.To];

                IEdge newEdge = new EasyEdge(linearizedVertex,
                    linearizedMeta,
                    linearizedTo);

                LinearizeGraph_Reccurent(linearizedTo, sourceLinerizedDict);
            }

            return linearizedVertex;
        }



        static public void GraphDebug(IVertex v, string fileName)
        {
            StringBuilder file = new StringBuilder();            

            GraphDebug_reccurent(0, v, file);

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
