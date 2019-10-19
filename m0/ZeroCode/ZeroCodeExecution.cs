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

        public INoInEdgeInOutVertexVertex newVertexCreationSpace;

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

        public INoInEdgeInOutVertexVertex ExecuteInstructionByMontevideoPrinciples(IVertex inputQs, IVertex instructionVertex)
        {
            bool dummy;

            return ExecuteInstructionByMontevideoPrinciples(inputQs, instructionVertex, out dummy);
        }

        public INoInEdgeInOutVertexVertex ExecuteInstructionByMontevideoPrinciples(IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex is_v = InstructionHelpers.GetIs(instructionVertex);            

            if (InstructionHelpers.CheckIfHasExecutableEndPoint(is_v))  // execute if you can
                return CallableEndPointDictionary.CallEndPoint(this, inputQs, instructionVertex, out isStackFrameReturn);            

            INoInEdgeInOutVertexVertex stack_ = InstructionHelpers.CreateStack();

            stack_.AddEdgeForNoInEdgeInOutVertexVertex(GraphUtil.CreateArtificialEdge(null, instructionVertex)); // create stack and put reference

            return stack_;
        }
    }
}
