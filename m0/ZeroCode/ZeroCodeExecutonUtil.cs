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

            exe.AddStackFrame(theObject);
            
            exe.AddStackFrame();

            exe.Stack.AddEdge(thisMeta, theObject);
            
            endPoint.Execute(exe);            
        }

        public static void CreateExecutionAndVertexExecute(IVertex endPoint, IVertex toBeStackVertex)
        {
            IExecution exe = new ZeroCodeExecution(toBeStackVertex);

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
