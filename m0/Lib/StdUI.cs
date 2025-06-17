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
        public static INoInEdgeInOutVertexVertex InteractionOutput(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex output = GraphUtil.GetQueryOutFirst(stack, "output", null);

            if (output == null)
                return exe.Stack;

            m0.MinusZero.Instance.UserInteraction.InteractionOutput(GraphUtil.GetStringValue(output));

            return exe.Stack;
        }

        // @String "output"
        //
        // returns:
        // @String
        public static INoInEdgeInOutVertexVertex InteractionInput(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex output = GraphUtil.GetQueryOutFirst(stack, "output", null);

            if (output == null)
                return exe.Stack;

            string input = m0.MinusZero.Instance.UserInteraction.InteractionInput(GraphUtil.GetStringValue(output));

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
        public static INoInEdgeInOutVertexVertex InteractionSelect(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex output = GraphUtil.GetQueryOutFirst(stack, "output", null);

            IList<IEdge> option = GraphUtil.GetQueryOut(stack, "option", null);

            if (output == null || option.Count==0)
                return exe.Stack;

            IVertex selection = m0.MinusZero.Instance.UserInteraction.InteractionSelect(output, option, false);

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
        public static INoInEdgeInOutVertexVertex InteractionSelectButton(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex output = GraphUtil.GetQueryOutFirst(stack, "output", null);

            IList<IEdge> option = GraphUtil.GetQueryOut(stack, "option", null);

            if (output == null || option.Count == 0)
                return exe.Stack;

            IVertex selection = m0.MinusZero.Instance.UserInteraction.InteractionSelectButton(output, option);

            if (selection == null)
                return exe.Stack;

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            newStack.AddEdge(null, selection);

            return newStack;
        }

        // @Edge "baseEdge"
        public static INoInEdgeInOutVertexVertex OpenDefaultVisualiser(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex baseEdge = GraphUtil.GetQueryOutFirst(stack, "baseEdge", null);

            if (baseEdge == null)
                return exe.Stack;

            m0.MinusZero.Instance.UserInteraction.OpenDefaultVisualiser(baseEdge, false);

            return exe.Stack;
        }

        // @Edge "baseEdge"
        // @UXItem "visualiser"
        public static INoInEdgeInOutVertexVertex OpenVisualiser(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex baseEdge = GraphUtil.GetQueryOutFirst(stack, "baseEdge", null);

            if (baseEdge == null)
                return exe.Stack;

            IVertex visualiser = GraphUtil.GetQueryOutFirst(stack, "visualiser", null);

            if (visualiser == null)
                return exe.Stack;

            m0.MinusZero.Instance.UserInteraction.OpenVisualiser(baseEdge, visualiser, true);

            return exe.Stack;
        }

        // @Edge "baseEdge"
        public static INoInEdgeInOutVertexVertex OpenFormVisualiser(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex baseEdge = GraphUtil.GetQueryOutFirst(stack, "baseEdge", null);

            if (baseEdge == null)
                return exe.Stack;

            m0.MinusZero.Instance.UserInteraction.OpenFormVisualiser(baseEdge, true);

            return exe.Stack;
        }

        // @Edge "baseEdge"
        public static INoInEdgeInOutVertexVertex OpenCodeVisualiser(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex baseEdge = GraphUtil.GetQueryOutFirst(stack, "baseEdge", null);

            if (baseEdge == null)
                return exe.Stack;

            IVertex codeVis = MinusZero.Instance.root.Get(false, @"System\Meta\Visualiser\Code");

            m0.MinusZero.Instance.UserInteraction.OpenVisualiser(baseEdge, codeVis, true);

            return exe.Stack;
        }
    }
}
