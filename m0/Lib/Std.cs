using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Lib
{
    public class Std
    {
        public static INoInEdgeInOutVertexVertex Concatenate(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Split(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.stack;

            IVertex input = GraphUtil.GetQueryOutFirst(stack, "input", null);

            if (input != null)
            {
                string inputString = input.Value.ToString();

                INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

                for (int x = 0; x < inputString.Length; x++)
                    newStack.AddVertex(null, inputString[x]);

                return newStack;
            }
            

            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex SplitBy(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Replace(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex IndexOf(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Substring(IExecution exe)
        {
            return exe.stack;
        }
        public static INoInEdgeInOutVertexVertex Sqrt(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Pow(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Abs(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Celling(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Floor(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Sin(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Cos(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Exp(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Log(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Log10(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Max(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Min(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Sign(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Tan(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Randomize(IExecution exe)
        {
            return exe.stack;
        }

        public static INoInEdgeInOutVertexVertex Random(IExecution exe)
        {
            return exe.stack;
        }
    }
}
