using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.ZeroCode.Helpers;
using m0.ZeroCode;

namespace m0.ZeroUML.Instructions
{
    public class BaseInstructions
    {
        public static void AddResults(IVertex baseVertex, bool outEdges, object meta, object value, IVertex toAdd)
        {
            IEdge result;
            IList<IEdge> results;

            if (outEdges)
                baseVertex.QueryOutEdges(meta, value, out result, out results);
            else
                baseVertex.QueryInEdges(meta, value, out result, out results);

            if (result != null)
                toAdd.AddEdge(result.Meta, result.To);

            if (results != null)
                foreach (IEdge e in results)
                    toAdd.AddEdge(e.Meta, e.To);
        }

        public static IVertex stepIntoAllEdges(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionsVertex)
        {
            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateQueryStack();

            foreach (IEdge e in inputQs)
                foreach (IEdge ee in e.To)
                    newQs.AddEdgeForNoInEdgeInOutVertexVertex(ee);

            return newQs;
        }

        public static IVertex QueryOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            if(instructionVertex.Value==null)
                return inputQs;

            string v = instructionVertex.Value.ToString();

            if (v == "" || v == "\r")
                return inputQs;

            INoInEdgeInOutVertexVertex newQs = InstructionHelpers.CreateQueryStack();

            IEdge e;
            IList<IEdge> eList;

            if (exe.metaMode)                            
                inputQs.QueryOutEdges(v, null, out e, out eList);            
            else
                inputQs.QueryOutEdges(null, v, out e, out eList);

            if (e != null)
                newQs.AddEdgeForNoInEdgeInOutVertexVertex(e);

            if(eList != null)
                InstructionHelpers.AddToStack(eList, newQs);

            return newQs;
        }

        public static IVertex InnerOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }

        public static IVertex StarOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }

        public static IVertex SlashOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }

        public static IVertex ColonOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }

        public static IVertex DoubleOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }        
    }
}
