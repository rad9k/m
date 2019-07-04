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
        public static INoInEdgeInOutVertexVertex CreateStack()
        {
            return new NoInEdgeInOutVertexVertex(MinusZero.Instance.TempStore);
        }

        public static void AddToStack(IEnumerable<IEdge> source, INoInEdgeInOutVertexVertex destination)
        {            
            foreach (IEdge e in source)
                destination.AddEdgeForNoInEdgeInOutVertexVertex(e);
        }

        public static INoInEdgeInOutVertexVertex CreateStackAndCopy(IEnumerable<IEdge> source)
        {
            INoInEdgeInOutVertexVertex newStack = CreateStack();

            AddToStack(source, newStack);

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex MakeINoInEdgeInOutVertexVertex(IVertex source)
        {
            if (source is INoInEdgeInOutVertexVertex)
                return (INoInEdgeInOutVertexVertex)source;

            return CreateStackAndCopy(source);
        }

        public static bool CheckIs(IVertex v, string i)
        {
            IVertex iv = GraphUtil.GetQueryOutFirst(v, "$Is", (object)i);

            if (iv != null)
                return true;

            return false;
        }

        public static IVertex GetLeft(IVertex v)
        {
            return GraphUtil.GetQueryOutFirst(v, "LeftExpression", null);
        }

        public static IVertex GetRight(IVertex v)
        {
            return GraphUtil.GetQueryOutFirst(v, "RightExpression", null);
        }

        public static IVertex GetNextExpression(IVertex v)
        {
            return GraphUtil.GetQueryOutFirst(v, "NextExpression", null);
        }

        public static INoInEdgeInOutVertexVertex NextExpressionHandle(ZeroCodeExecution exe, IVertex inQs, IVertex instructionVertex)
        {
            IVertex nextExpression = InstructionHelpers.GetNextExpression(instructionVertex);

            if (nextExpression != null)
                return exe.executeInstruction(inQs, nextExpression);

            return MakeINoInEdgeInOutVertexVertex(inQs);            
        }

        public static INoInEdgeInOutVertexVertex SequentiallyExecuteInstructions(ZeroCodeExecution exe, INoInEdgeInOutVertexVertex inStack, IVertex baseVertex)
        {
            INoInEdgeInOutVertexVertex stack = inStack;

            foreach (IEdge e in baseVertex)
                stack = exe.executeInstruction(stack, e.To);

            return stack;
        }
        
    }
}
