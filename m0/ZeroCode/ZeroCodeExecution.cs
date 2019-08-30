using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
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

            if(InstructionHelpers.CheckIfHasExecutableEndPoint(is_v))
                return CallableEndPointDictionary.CallEndPoint(this, inputQs, instructionVertex, out isStackFrameReturn);
            
            //if(InstructionHelpers.CheckIfIsAtomType(is_v))

            if (InstructionHelpers.GetIs(instructionVertex) == null) // new vertex
            {
                INoInEdgeInOutVertexVertex stack = InstructionHelpers.CreateStack();
                stack.AddEdgeForNoInEdgeInOutVertexVertex(GraphUtil.CreateArtificialEdge(null, instructionVertex));
                return stack;
            }

            return null;
        }
    }
}
