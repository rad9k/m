using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using m0.Lib.Util;

namespace m0.Lib
{
    public class Cryptography
    {
        public static INoInEdgeInOutVertexVertex HashPassword(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex passwordVertex = GraphUtil.GetQueryOutFirst(stack, "password", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            if (passwordVertex == null)
                return newStack;

            string hashedPassword = PasswordHasher.Hash(passwordVertex.Value.ToString());

            newStack.AddVertex(null, hashedPassword);

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex VerifyPassword(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex passwordToCheckVertex = GraphUtil.GetQueryOutFirst(stack, "passwordToCheck", null);
            IVertex passwordStoredVertex = GraphUtil.GetQueryOutFirst(stack, "passwordStored", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            if (passwordToCheckVertex == null || passwordStoredVertex == null)
                return newStack;

            bool isMatch = PasswordHasher.Verify(passwordToCheckVertex.Value.ToString(), passwordStoredVertex.Value.ToString());

            newStack.AddVertex(null, isMatch);

            return newStack;
        }
    }
}