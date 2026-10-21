using m0.Foundation;
using m0.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace m0.Lib
{
    public class Cryptography
    {
        public static INoInEdgeInOutVertexVertex HashPassword(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex passwordVertex = GraphUtil.GetQueryOutFirst(stack, "password", null);

            if (passwordVertex == null)
             return 

            return exe.Stack;
        }

        public static INoInEdgeInOutVertexVertex VerifyPassword(IExecution exe)
        {
            MinusZero.Instance.GracefullExitToken.Token.WaitHandle.WaitOne();

            return exe.Stack;
        }
    }
}