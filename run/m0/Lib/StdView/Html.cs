using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Lib.StdView
{
    public class Html
    {
        public static INoInEdgeInOutVertexVertex AddColorsToCode(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex FormalTextLanguage_Vertex = GraphUtil.GetQueryOutFirst(stack, "FormalTextLanguage", null);
            IVertex text_Vertex = GraphUtil.GetQueryOutFirst(stack, "text", null);

            string FormalTextLanguage = GraphUtil.GetStringValue(FormalTextLanguage_Vertex);
            string text = GraphUtil.GetStringValue(text_Vertex);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            newStack.AddVertex(null, "kotek" + text);

            return newStack;
        }
    }
}
