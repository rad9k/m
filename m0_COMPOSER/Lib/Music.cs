using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{
    public class Music
    {
        public static INoInEdgeInOutVertexVertex NoteOn(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.stack;

            /*IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            StringBuilder sb = new StringBuilder();

            foreach (IEdge e in inputList)
                sb.Append(e.To.ToString());

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            newStack.AddVertex(null, sb.ToString());*/

            return stack;
        }
    }
}
