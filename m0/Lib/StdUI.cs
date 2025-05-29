using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Lib
{
    public class StdUI
    {
        // @String "output"
        public static INoInEdgeInOutVertexVertex OutputDialog(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex output = GraphUtil.GetQueryOutFirst(stack, "output", null);

            if (output == null)
                return exe.Stack;

            m0Main.Instance.ShowInfo(GraphUtil.GetStringValue(output));

            return exe.Stack;
        }

        // @String "output"
        //
        // returns:
        // @String
        public static INoInEdgeInOutVertexVertex InputDialog(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex output = GraphUtil.GetQueryOutFirst(stack, "output", null);

            if (output == null)
                return exe.Stack;

            string input = m0Main.Instance.StringQuestionDialog(GraphUtil.GetStringValue(output), null);

            if (input == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            newStack.AddVertex(null, input);

            return newStack;
        }

        // @String "output"
        // @Vertex "option"
        //
        // returns:
        // @Vertex
        public static INoInEdgeInOutVertexVertex SelectDialog(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex output = GraphUtil.GetQueryOutFirst(stack, "output", null);

            IList<IEdge> option = GraphUtil.GetQueryOut(stack, "option", null);

            if (output == null || option.Count==0)
                return exe.Stack;

            IVertex selection = m0Main.Instance.SelectDialog(output, option, false, null);

            if (selection == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            newStack.AddEdge(null, selection);

            return newStack;
        }

        // @String "output"
        // @Vertex "option"
        //
        // returns:
        // @Vertex
        public static INoInEdgeInOutVertexVertex SelectButtonDialog(IExecution exe)
        {
            return null;
        }

        // @Edge "baseEdge"
        public static INoInEdgeInOutVertexVertex OpenDefaultVisualiser(IExecution exe)
        {
            return null;
        }

        // @Edge "baseEdge"
        // @UXItem "visualiser"
        public static INoInEdgeInOutVertexVertex OpenVisualiser(IExecution exe)
        {
            return null;
        }

        // @Edge "baseEdge"
        public static INoInEdgeInOutVertexVertex OpenFormVisualiser(IExecution exe)
        {
            return null;
        }

        // @Edge "baseEdge"
        public static INoInEdgeInOutVertexVertex OpenCodeVisualiser(IExecution exe)
        {
            return null;
        }
    }
}
