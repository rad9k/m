using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
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

        static public IList<IEdge> Linearize(IVertex v)
        {
            IList<IEdge> linearizedList = new List<IEdge>();

            foreach (IEdge e in v)
                if(e.Meta != dict.NextAtomMeta)
                    linearizedList.Add(e);

            foreach (IEdge e in v)
                AddNextEdges(linearizedList, e.To);

            return linearizedList;
        }

        static void AddNextEdges(IList<IEdge> linearizedList, IVertex v)
        {
            foreach(IEdge e in v)
                if(e.Meta == dict.NextAtomMeta)
                {
                    IEdge ee = new EasyEdge(e.From, MinusZero.Instance.Empty, e.To);

                    linearizedList.Add(ee);

                    AddNextEdges(linearizedList, e.To);
                }
        }
        
    }
}
