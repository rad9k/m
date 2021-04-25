using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    public interface IExecution
    {
        INoInEdgeInOutVertexVertex stack { get; set; }

        IVertex newVertexCreationSpace { get; set; }

        bool metaMode { get; set; }

        void AddStackFrame();

        void AddStackFrame(IVertex newStackFrame);

        void RemoveStackFrame();

        void CreateEmptyStack();

        INoInEdgeInOutVertexVertex ExecuteInstructionByMontevideoPrinciples(IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn);

        INoInEdgeInOutVertexVertex ExecuteInstruction(IVertex inputQs, IVertex instructionVertex, out bool isStackFrameReturn);        
    }
}
