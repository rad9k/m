using m0.DotNetIntegration;
using m0.Foundation;
using m0.ZeroCode.Helpers;
using m0.ZeroUML.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static m0.Graph.GraphUtil;

namespace m0.Graph.ExecutionFlow
{
    public class ExecutionFlowHelper
    {
        public static void StartTransaction()
        {
            Lib.Sys.StartTransaction(null);
        }

        public static void RollbackTransaction()
        {
            Lib.Sys.RollbackTransaction(null);
        }

        public static void CommitTransaction()
        {
            Lib.Sys.CommitTransaction(null);
        }

        public static INoInEdgeInOutVertexVertex ExecuteDotNetDelegate(IVertex baseVertex, IExecution exe)
        {
            IVertex dotNetDelegatePointer = GraphUtil.GetQueryOutFirst(baseVertex, "DotNetDelegatePointer", null);

            if(dotNetDelegatePointer != null && dotNetDelegatePointer.Value is DotNetDelegate)
            {
                DotNetDelegate del = (DotNetDelegate)dotNetDelegatePointer.Value;

                del.Invoke(exe);
            }

            return null;
        }

        public static INoInEdgeInOutVertexVertex ExecuteDelegate(IVertex baseVertex, IExecution exe)
        {
            IVertex _object = GraphUtil.GetQueryOutFirst(baseVertex, "Object", null);
            IVertex method = GraphUtil.GetQueryOutFirst(baseVertex, "Method", null);

            if(_object != null && method != null)
            {
                exe.AddStackFrame(_object); // this is WRONG XXX as we have method call parameters allready on stack
                // this means that _object edges will potentially overwrite call paramaters
                // no way to do it otherwise althought

                method.Execute(exe);

                exe.RemoveStackFrame();
            }

            return null;
        }

        public static INoInEdgeInOutVertexVertex Execute(IVertex baseVertex, IExecution exe)
        {
            bool dummy;

            if (InstructionHelpers.CheckIfIsInherits(baseVertex, "Executable"))
            {
                if(InstructionHelpers.CheckIfIs(baseVertex, "DotNetStaticMethod"))
                    return CallableEndPointDictionary_INIEIOV_ZCE.CallEndPoint(exe, baseVertex);

                if (InstructionHelpers.CheckIfIs(baseVertex, "DotNetDelegate"))
                    return ExecuteDotNetDelegate(baseVertex, exe);

                if (InstructionHelpers.CheckIfIs(baseVertex, "Delegate"))
                    return ExecuteDelegate(baseVertex, exe);

                return null;
            }
            else
                return InstructionHelpers.SequentiallyExecuteInstructions(exe, exe.Stack, baseVertex, out dummy, false);
        }
    }
}
