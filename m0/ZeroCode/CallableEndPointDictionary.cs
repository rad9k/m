using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    public class CallableEndPointDictionary
    {
        delegate INoInEdgeInOutVertexVertex CallableEndPointDelegate(ZeroCodeExecution exe, IVertex inputStack, IVertex instructionVertex, out bool IsStackFrameReturn);

        static Dictionary<IVertex, CallableEndPointDelegate> DotNetEndPointDictionary = new Dictionary<IVertex, CallableEndPointDelegate>();
        
        public static INoInEdgeInOutVertexVertex CallEndPoint(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex, out bool IsStackFrameReturn)
        {
            IsStackFrameReturn = false;

            CallableEndPointDelegate del = null;

            IVertex _is = GraphUtil.FindOneByMeta(instructionVertex, "$Is");

            if (_is == null)
                return null;

            if (DotNetEndPointDictionary.ContainsKey(_is))
                del = DotNetEndPointDictionary[_is];
            else
            {            
                IVertex ep = GraphUtil.FindOneByMeta(_is, "$CallableEndPoint");

                if (ep == null)
                    return null;

                if (GraphUtil.GetQueryOutFirst(ep, "$Is", "DotNetEndPoint") != null)
                {
                    string typeString = (string)GraphUtil.FindOneByMeta(ep, "TypeName").Value;
                    string methodString = (string)GraphUtil.FindOneByMeta(ep, "MethodName").Value;

                    Type type = Type.GetType(typeString);
                    MethodInfo method = type.GetMethod(methodString);

                    del = (CallableEndPointDelegate)method.CreateDelegate(typeof(CallableEndPointDelegate));
                }

                DotNetEndPointDictionary.Add(_is, del);
            }

            if (del == null)
                return null;

            return del.Invoke(exe, inputQs, instructionVertex, out IsStackFrameReturn);            
        }
    }
}
