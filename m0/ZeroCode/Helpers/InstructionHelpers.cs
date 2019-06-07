using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode.Helpers
{
    public class InstructionHelpers
    {
        public static IVertex CreateQueryStack()
        {
            return new NoInEdgeInOutVertexVertex(MinusZero.Instance.TempStore);
        }
        public static bool CheckIs(IVertex v, string i)
        {
            IVertex iv = GraphUtil.GetOutFirst(v, "$Is", (object)i);

            if (iv != null)
                return true;

            return false;
        }

        public static IVertex GetLeft(IVertex v)
        {
            return GraphUtil.GetOutFirst(v, "LeftExpression", null);
        }

        public static IVertex GetRight(IVertex v)
        {
            return GraphUtil.GetOutFirst(v, "RightExpression", null);
        }

        public static IVertex GetNextExpression(IVertex v)
        {
            return GraphUtil.GetOutFirst(v, "NextExpression", null);
        }
    }
}
