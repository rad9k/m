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
            INoInEdgeInOutVertexVertex stack = exe.stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            StringBuilder sb = new StringBuilder();

            foreach (IEdge e in inputList)
                sb.Append(e.To.ToString());

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            newStack.AddVertex(null, sb.ToString());

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Split(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputList)
            {
                string inputString = e.To.Value.ToString();

                for (int x = 0; x < inputString.Length; x++)
                    newStack.AddVertex(null, inputString[x]);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex SplitBy(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.stack;

            IList<IEdge> byList = GraphUtil.GetQueryOut(stack, "by", null);

            IList<string> splitByStringList = new List<string>();

            foreach (IEdge e in byList)
                splitByStringList.Add(e.To.Value.ToString());

            string[] slitByStringArray = splitByStringList.ToArray();

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputList)
            {
                string inputString = e.To.Value.ToString();

                string[] splitStrings = inputString.Split(slitByStringArray, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in splitStrings)
                    newStack.AddVertex(null, s);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Replace(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            IVertex fromVertex = GraphUtil.GetQueryOutFirst(stack, "from", null);
            IVertex toVertex = GraphUtil.GetQueryOutFirst(stack, "to", null);

            if (fromVertex == null || toVertex == null)
                return exe.stack;

            string from = fromVertex.Value.ToString();
            string to = toVertex.Value.ToString();

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputList)
            {
                string inputString = e.To.Value.ToString();

                string newString = inputString.Replace(from, to);

                newStack.AddVertex(null, newString);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex IndexOf(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            IVertex testVertex = GraphUtil.GetQueryOutFirst(stack, "test", null);

            if (testVertex == null)
                return exe.stack;

            string test = testVertex.Value.ToString();

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputList)
            {
                string inputString = e.To.Value.ToString();

                int indexOf = inputString.IndexOf(test);

                newStack.AddVertex(null, indexOf);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Substring(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            IVertex fromVertex = GraphUtil.GetQueryOutFirst(stack, "from", null);
            IVertex toVertex = GraphUtil.GetQueryOutFirst(stack, "to", null);

            if (fromVertex == null || toVertex == null)
                return exe.stack;

            int? _from = GraphUtil.GetIntegerValue(fromVertex);
            int? _to = GraphUtil.GetIntegerValue(toVertex);

            if (_from == null || _to == null)
                return exe.stack;

            int from = (int)_from;
            int to = (int)_to;

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputList)
            {
                string inputString = e.To.Value.ToString();

                string newString = inputString.Replace(from, to);

                newStack.AddVertex(null, newString);
            }

            return newStack;
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
