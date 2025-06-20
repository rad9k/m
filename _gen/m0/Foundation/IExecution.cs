using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    public interface IExecution
    {
        INoInEdgeInOutVertexVertex Stack { get; set; }

        IVertex NewVertexCreationSpace { get; set; }

        bool MetaMode { get; set; }

        void AddStackFrame();

        void AddStackFrame(IVertex newStackFrame);

        void RemoveStackFrame();

        void CreateEmptyStack();

        INoInEdgeInOutVertexVertex ExecuteInstructionByMontevideoPrinciples(IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn);

        INoInEdgeInOutVertexVertex ExecuteInstruction(IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn);        
    }
}
