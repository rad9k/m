using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Lib.StdView
{
    public class MdStringToTokenVertexes
    {
        public static INoInEdgeInOutVertexVertex MdStringToTokenVertexes_Transform(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex from = GraphUtil.GetQueryOutFirst(stack, "from", null);
            IVertex to = GraphUtil.GetQueryOutFirst(stack, "to", null);

            if (from == null || to == null)
                return exe.Stack;

            MdStringToTokenVertexes_Process(GraphUtil.GetStringValue(from), to);

            return exe.Stack;
        }

        public static void MdStringToTokenVertexes_Process(string md, IVertex to)
        {
            return MarkdownTokenizor.TokenizeMarkdownToJson(md);
        }


    }
}
