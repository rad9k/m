using m0.Foundation;
using m0.Graph;
using m0.ZeroCode;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace m0.DotNetIntegration
{
    // EasyVertex.Get/GetAll > ZeroCodeExecuter
    // ZeroCodeExecution.ExecuteInstructionByMontevideoPrinciples
    // ZeroCodeExecution.ExecuteInstruction
    //
    // CallEndPoint(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
    // CallEndPoint(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool IsStackFrameReturn)

    public class CallableEndPointDictionary_INIEIOV_ZCE_IV_IV_B
    {
        delegate INoInEdgeInOutVertexVertex CallableEndPointDelegate(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool IsStackFrameReturn);

        static ConcurrentDictionary<IVertex, CallableEndPointDelegate> DotNetEndPointDictionary =
            new ConcurrentDictionary<IVertex, CallableEndPointDelegate>();

        public static INoInEdgeInOutVertexVertex CallEndPoint(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            bool dummy;

            return CallEndPoint(exe, inputStack, instructionVertex, out dummy);
        }

        public static INoInEdgeInOutVertexVertex CallEndPoint(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool IsStackFrameReturn)
        {
            IVertex _is = GraphUtil.GetQueryOutFirst(instructionVertex, "$Is", null);

            if (_is == null)
            {
                IsStackFrameReturn = false;
                return null;
            }

            return CallEndPoint(
                exe,
                inputStack,
                instructionVertex,
                _is,
                out IsStackFrameReturn);
        }

        internal static INoInEdgeInOutVertexVertex CallEndPoint(
            ZeroCodeExecution exe,
            IVertex inputStack,
            IVertex instructionVertex,
            IVertex instructionType,
            out bool IsStackFrameReturn)
        {
            TryCallEndPoint(
                exe,
                inputStack,
                instructionVertex,
                instructionType,
                out INoInEdgeInOutVertexVertex result,
                out IsStackFrameReturn);
            return result;
        }

        internal static bool TryCallEndPoint(
            ZeroCodeExecution exe,
            IVertex inputStack,
            IVertex instructionVertex,
            IVertex instructionType,
            out INoInEdgeInOutVertexVertex result,
            out bool IsStackFrameReturn)
        {
            IsStackFrameReturn = false;
            result = null;

            CallableEndPointDelegate del;

            bool endpointCacheHit = DotNetEndPointDictionary.TryGetValue(
                instructionType,
                out del);
            ZeroCodePerformanceCounters.RecordEndpointCacheLookup(
                endpointCacheHit);

            if (!endpointCacheHit)
            {            
                IVertex ep = GraphUtil.GetQueryOutFirst(instructionType, "$ExecutableEndPoint", null);

                if (ep == null)
                    return false;

                if (GraphUtil.GetQueryOutFirst(ep, "$Is", "DotNetStaticMethod") != null)
                {
                    string typeString = (string)GraphUtil.GetQueryOutFirst(ep, "DotNetTypeName", null).Value;
                    string methodString = (string)GraphUtil.GetQueryOutFirst(ep, "DotNetMethodName", null).Value;

                    Type type = Type.GetType(typeString);
                    MethodInfo method = type.GetMethod(methodString);

                    del = (CallableEndPointDelegate)method.CreateDelegate(typeof(CallableEndPointDelegate));
                }

                if (del != null)
                    DotNetEndPointDictionary.TryAdd(
                        instructionType,
                        del);
            }

            if (del == null)
                return true;

            result = del.Invoke(
                exe,
                inputStack,
                instructionVertex,
                out IsStackFrameReturn);
            return true;
        }
    }
}
