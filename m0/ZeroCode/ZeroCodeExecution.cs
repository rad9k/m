using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    public class ZeroCodeExecution
    {
        public INoInEdgeInOutVertexVertex stack;

        public bool metaMode;

        public void AddStackFrame()
        {
            INoInEdgeInOutVertexVertex newStackFrame = InstructionHelpers.CreateStack();

            newStackFrame.AddEdge(MinusZero.Instance.StackFrameInherits, stack);

            stack = newStackFrame;
        }

        public void RemoveStackFrame()
        {
            IVertex _prevStackFrame = stack.Get(false, @"$StackFrameInherits:");

            if(_prevStackFrame != null && _prevStackFrame is INoInEdgeInOutVertexVertex)
            {
                INoInEdgeInOutVertexVertex prevStackFrame = (INoInEdgeInOutVertexVertex)_prevStackFrame;

                stack = prevStackFrame;
            }
        }

        public INoInEdgeInOutVertexVertex ExecuteInstruction(IVertex inputQs, IVertex instructionVertex)
        {
            bool dummy;

            return ExecuteInstruction(inputQs, instructionVertex, out dummy);
        }

        public INoInEdgeInOutVertexVertex ExecuteInstruction(IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex is_v = InstructionHelpers.GetIs(instructionVertex);


            if(InstructionHelpers.CheckIfIsAtomType(is_v)) // AtomType => deep copy 
            {
                INoInEdgeInOutVertexVertex stack = InstructionHelpers.CreateStack();

                IVertex copiedVertex = stack.AddVertex(null, null);

                VertexOperations.DeepVertexCopy(instructionVertex, copiedVertex);

                return stack;
            }

            if (InstructionHelpers.CheckIfHasExecutableEndPoint(is_v))
                return CallableEndPointDictionary.CallEndPoint(this, inputQs, instructionVertex, out isStackFrameReturn);

            // non atom value subgraph => add edge to subgraph's root

            INoInEdgeInOutVertexVertex stack_ = InstructionHelpers.CreateStack();

            stack_.AddEdgeForNoInEdgeInOutVertexVertex(GraphUtil.CreateArtificialEdge(null, instructionVertex));

            return stack_;
        }
    }
}
