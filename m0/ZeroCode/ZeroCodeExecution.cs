using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    public class ZeroCodeExecution
    {
        public INoInEdgeInOutVertexVertex stack;

        public bool metaMode;

        public INoInEdgeInOutVertexVertex executeInstruction(IVertex inputQs, IVertex instructionVertex)
        {
            if (InstructionHelpers.GetIs(instructionVertex) == null)
            {
                INoInEdgeInOutVertexVertex stack = InstructionHelpers.CreateStack();
                stack.AddEdgeForNoInEdgeInOutVertexVertex(GraphUtil.CreateArtificialEdge(null, instructionVertex));
                return stack;
            }

            return CallableEndPointDictionary.CallEndPoint(this, inputQs, instructionVertex);
        }
    }
}
