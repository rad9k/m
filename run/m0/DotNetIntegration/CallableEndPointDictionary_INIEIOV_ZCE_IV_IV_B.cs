using m0.Foundation;
using m0.Graph;
using m0.ZeroCode;
using System;
using System.Collections.Generic;
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

        static Dictionary<IVertex, CallableEndPointDelegate> DotNetEndPointDictionary = new Dictionary<IVertex, CallableEndPointDelegate>();

        public static INoInEdgeInOutVertexVertex CallEndPoint(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex)
        {
            bool dummy;

            return CallEndPoint(exe, inputStack, instructionVertex, out dummy);
        }

        public static INoInEdgeInOutVertexVertex CallEndPoint(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool IsStackFrameReturn)
        {
            IsStackFrameReturn = false;

            CallableEndPointDelegate del = null;

            IVertex _is = GraphUtil.GetQueryOutFirst(instructionVertex, "$Is", null);

            if (_is == null)
                return null;

            if (DotNetEndPointDictionary.ContainsKey(_is))
                del = DotNetEndPointDictionary[_is];
            else
            {            
                IVertex ep = GraphUtil.GetQueryOutFirst(_is, "$ExecutableEndPoint", null);

                if (ep == null)
                    return null;

                if (GraphUtil.GetQueryOutFirst(ep, "$Is", "DotNetStaticMethod") != null)
                {
                    string typeString = (string)GraphUtil.GetQueryOutFirst(ep, "DotNetTypeName", null).Value;
                    string methodString = (string)GraphUtil.GetQueryOutFirst(ep, "DotNetMethodName", null).Value;

                    Type type = Type.GetType(typeString);
                    MethodInfo method = type.GetMethod(methodString);

                    del = (CallableEndPointDelegate)method.CreateDelegate(typeof(CallableEndPointDelegate));
                }

                if (del != null)
                    DotNetEndPointDictionary.Add(_is, del);
            }

            if (del == null)
                return null;

            return del.Invoke(exe, inputStack, instructionVertex, out IsStackFrameReturn);            
        }
    }
}
