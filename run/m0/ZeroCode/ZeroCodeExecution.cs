using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0.DotNetIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace m0.ZeroCode
{
    public class ZeroCodeExecution: IExecution
    {
        public INoInEdgeInOutVertexVertex Stack { get; set; }

        public IVertex NewVertexCreationSpace { get; set; }

        public bool MetaMode { get; set; }        

        public ZeroCodeExecution()
        {
            MetaMode = true;

            CreateEmptyStack();

            NewVertexCreationSpace = Stack;

            AddRootToStack();
        }

        public ZeroCodeExecution(IVertex expression)
        {
            MetaMode = true;

            CreateEmptyStack();

            NewVertexCreationSpace = Stack;

            AddRootToStack();

            AddDolarToStack(expression);
        }

        public ZeroCodeExecution(IVertex toBeStackVertex, IVertex expression)
        {
            MetaMode = true;

            IEnumerable<IEdge> _toBeStackVertex;

            if (toBeStackVertex == null)
                _toBeStackVertex = new List<IEdge>();
            else
                _toBeStackVertex = toBeStackVertex;

            Stack = InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(_toBeStackVertex);

            NewVertexCreationSpace = Stack;

            AddRootToStack();

            AddDolarToStack(expression);
        }

        private void AddDolarToStack(IVertex expression)
        {
            Stack.AddEdge(MinusZero.Instance.Dolar, expression);
        }

        public void AddRootToStack()
        {
            Stack.AddEdge(MinusZero.Instance.StackFrameInherits, MinusZero.Instance.Root);
        }

        public void CreateEmptyStack()
        {
            Stack = InstructionHelpers.CreateStack();
        }

        public void AddStackFrame()
        {
            INoInEdgeInOutVertexVertex newStackFrame = InstructionHelpers.CreateStack();

            newStackFrame.AddEdge(MinusZero.Instance.StackFrameInherits, Stack);

            Stack = newStackFrame;            
        }

        public void AddStackFrame(IVertex newStackFrame)
        {            
            INoInEdgeInOutVertexVertex newStackFrameINIEIOV = InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(newStackFrame);
            
            newStackFrameINIEIOV.AddEdge(MinusZero.Instance.StackFrameInherits, Stack);

            Stack = newStackFrameINIEIOV;
        }

        public void RemoveStackFrame()
        {
            IEdge stackFrameInheritsEdge = GraphUtil.GetQueryOutFirstEdge(Stack, "$StackFrameInherits", null);

            if (stackFrameInheritsEdge == null)
                throw new Exception("Can not remove stack frame. No $StackFrameInherits");

            IVertex _prevStackFrame = stackFrameInheritsEdge.To;

            Stack.DeleteEdge(stackFrameInheritsEdge);

            if(_prevStackFrame != null && _prevStackFrame is INoInEdgeInOutVertexVertex)
            {
                INoInEdgeInOutVertexVertex prevStackFrame = (INoInEdgeInOutVertexVertex)_prevStackFrame;

                Stack = prevStackFrame;                
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

            if (InstructionHelpers.CheckIfHasExecutableEndPoint(is_v))  // execute if you can.....
                return CallableEndPointDictionary_INIEIOV_ZCE_IV_IV_B.CallEndPoint(this, inputQs, instructionVertex, out isStackFrameReturn);

            // ...OR...
            INoInEdgeInOutVertexVertex stack_ = InstructionHelpers.CreateStack();

            stack_.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(GraphUtil.CreateArtificialEdge(null, instructionVertex)); // create stack and put reference

            return stack_;
        }

        public INoInEdgeInOutVertexVertex ExecuteInstruction(IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            IVertex is_v = InstructionHelpers.GetIs(instructionVertex);

            if (InstructionHelpers.CheckIfHasExecutableEndPoint(is_v))  // execute if you can
                return CallableEndPointDictionary_INIEIOV_ZCE_IV_IV_B.CallEndPoint(this, inputQs, instructionVertex, out isStackFrameReturn);           

            return InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(inputQs);
        }
    }
}
