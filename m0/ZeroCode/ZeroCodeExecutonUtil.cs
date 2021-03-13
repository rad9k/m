using m0.Foundation;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    public class ZeroCodeExecutonUtil
    {
        public static void CreateExecutionAndVertexExecute(IVertex endPoint, IVertex toBeStackVertex)
        {
            IExecution exe = new ZeroCodeExecution();

            exe.metaMode = true;

            IEnumerable<IEdge> _toBeStackVertex;

            if (toBeStackVertex == null)
                _toBeStackVertex = new List<IEdge>();
            else
                _toBeStackVertex = toBeStackVertex;

            exe.stack = InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(_toBeStackVertex);

            exe.newVertexCreationSpace = exe.stack;

            ZeroCodeExecuter.AddRootToStack(exe);

            endPoint.Execute(exe);            
        }

        public static void MethodCallFromHost(IExecution exe, IVertex endPoint, IVertex theObject, IVertex paramtersStack)
        {
            exe.AddStackFrame(theObject); // ENTER NEW STACK
            exe.AddStackFrame(paramtersStack);

            endPoint.Execute(exe);

            exe.RemoveStackFrame();
            exe.RemoveStackFrame(); // LEAVE NEW STACK
        }
    }
}
