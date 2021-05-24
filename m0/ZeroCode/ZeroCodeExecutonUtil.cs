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
        static IVertex r = MinusZero.Instance.Root;

        static IVertex thisMeta = r.Get(false, @"System\Meta\ZeroUML\this");

        public static void CreateExecutionAndVertexMethodExecute(IVertex endPoint, IVertex theObject)
        {
            IExecution exe = new ZeroCodeExecution();

            exe.metaMode = true;

            exe.CreateEmptyStack();

            exe.NewVertexCreationSpace = exe.Stack;

            ZeroCodeExecuter.AddRootToStack(exe);

            exe.AddStackFrame(theObject);
            
            exe.AddStackFrame();

            exe.Stack.AddEdge(thisMeta, theObject);
            
            endPoint.Execute(exe);            
        }

        public static void CreateExecutionAndVertexExecute(IVertex endPoint, IVertex toBeStackVertex)
        {
            IExecution exe = new ZeroCodeExecution();

            exe.metaMode = true;

            IEnumerable<IEdge> _toBeStackVertex;

            if (toBeStackVertex == null)
                _toBeStackVertex = new List<IEdge>();
            else
                _toBeStackVertex = toBeStackVertex;

            exe.Stack = InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(_toBeStackVertex);

            exe.NewVertexCreationSpace = exe.Stack;

            ZeroCodeExecuter.AddRootToStack(exe);

            endPoint.Execute(exe);            
        }

        public static void MethodCallFromHost(IExecution exe, IVertex endPoint, IVertex theObject, IVertex paramtersStack)
        {
            exe.AddStackFrame(theObject); // ENTER NEW STACK
            exe.AddStackFrame(paramtersStack);
            exe.Stack.AddEdge(thisMeta, theObject);

            endPoint.Execute(exe);

            exe.RemoveStackFrame();
            exe.RemoveStackFrame(); // LEAVE NEW STACK
        }
    }
}
