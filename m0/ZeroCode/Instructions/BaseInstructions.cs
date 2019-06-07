using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.ZeroCode.Helpers;

namespace m0.ZeroCode.Instructions
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
            IVertex newQs = InstructionHelpers.CreateQueryStack();

            foreach (IEdge e in inputQs)
                foreach (IEdge ee in e.To)
                    newQs.AddEdge(ee.Meta, ee.To);

            return newQs;
        }

        public static IVertex InnerOperator(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            return inputQs;
        }
    }
}
