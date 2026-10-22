using m0.Foundation;
using m0.Graph;
using m0.Lib.Util;
using m0.ZeroCode.Helpers;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

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

        public static INoInEdgeInOutVertexVertex GenerateToken(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            var bytes = RandomNumberGenerator.GetBytes(32);
            string token =  $"token_{WebEncoders.Base64UrlEncode(bytes)}";            

            newStack.AddVertex(null, token);

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex HashToken(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex tokenVertex = GraphUtil.GetQueryOutFirst(stack, "token", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            string token = GraphUtil.GetStringValueOrNull(tokenVertex);

            if (tokenVertex == null)
                return newStack;

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            string hashedToken = Convert.ToHexString(hash);

            newStack.AddVertex(null, hashedToken);

            return newStack;
        }
    }
}